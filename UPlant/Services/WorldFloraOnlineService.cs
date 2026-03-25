using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using UPlant.Models.DB;

namespace UPlant.Services;

public interface IWorldFloraOnlineService
{
    Task<WfoCheckResult> CheckAsync(Specie specie, string genusName, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

public sealed class WorldFloraOnlineService : IWorldFloraOnlineService
{
    private const string MatchingEndpoint = "https://list.worldfloraonline.org/matching_rest.php";
    private const string StableDataEndpoint = "https://list.worldfloraonline.org/sw_data.php?format=json&wfo={0}";
    private const string TaxonPageEndpoint = "https://list.worldfloraonline.org/taxon/{0}";

    private readonly HttpClient _httpClient;

    public WorldFloraOnlineService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WfoCheckResult> CheckAsync(Specie specie, string genusName, CancellationToken cancellationToken = default)
    {
        var result = new WfoCheckResult
        {
            CurrentScientificName = SpecieScientificNameHelper.Compose(specie, genusName),
            TriedQueries = SpecieScientificNameHelper.BuildWfoQueries(specie, genusName).ToList()
        };

        foreach (var query in result.TriedQueries)
        {
            var matchResponse = await MatchAsync(query, cancellationToken);
            if (matchResponse == null)
            {
                continue;
            }

            result.Narrative.AddRange(matchResponse.narrative ?? Enumerable.Empty<string>());

            if (matchResponse.match != null)
            {
                var matched = ToCandidate(matchResponse.match);
                matched.QueryUsed = query;

                var details = await GetNameDetailsAsync(matched.WfoId, cancellationToken);
                matched.Lsid = details.Lsid;
                matched.SuggestedAcceptedName = details.AcceptedName;
                matched.IsAccepted = details.IsAccepted;
                matched.IucnGlobalCode = details.IucnGlobalCode;
                matched.IucnGlobalLabel = details.IucnGlobalLabel;

                result.Match = matched;
                result.Status = details.IsAccepted ? WfoMatchStatus.Accepted : WfoMatchStatus.Synonym;
                return result;
            }

            foreach (var candidate in matchResponse.candidates ?? Enumerable.Empty<WfoMatchName>())
            {
                var mapped = ToCandidate(candidate);
                mapped.QueryUsed = query;
                await PopulateCandidateDetailsAsync(mapped, cancellationToken);
                if (!IsRelevantCandidate(query, mapped))
                {
                    continue;
                }

                if (result.Candidates.All(c => !string.Equals(c.WfoId, mapped.WfoId, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Candidates.Add(mapped);
                }
            }
        }

        result.Candidates = result.Candidates
            .OrderByDescending(c => c.IsAccepted)
            .ThenBy(c => string.IsNullOrWhiteSpace(c.SuggestedAcceptedName) ? 1 : 0)
            .ThenBy(c => c.FullName)
            .ToList();

        var exactAcceptedCandidate = result.Candidates.FirstOrDefault(candidate =>
            candidate.IsAccepted &&
            string.Equals(
                SpecieScientificNameHelper.NormalizeSpacing(candidate.FullName),
                SpecieScientificNameHelper.NormalizeSpacing(result.CurrentScientificName),
                StringComparison.OrdinalIgnoreCase));

        if (exactAcceptedCandidate != null)
        {
            result.Match = exactAcceptedCandidate;
            result.Status = WfoMatchStatus.Accepted;
            return result;
        }

        result.Status = result.Candidates.Count > 0 ? WfoMatchStatus.Ambiguous : WfoMatchStatus.NotFound;
        return result;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var url = $"{MatchingEndpoint}?input_string={Uri.EscapeDataString("Abies alba")}&check_homonyms=true&check_rank=true";
            using var response = await _httpClient.GetAsync(url, timeoutCts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<WfoMatchResponse> MatchAsync(string query, CancellationToken cancellationToken)
    {
        var url = $"{MatchingEndpoint}?input_string={Uri.EscapeDataString(query)}&check_homonyms=true&check_rank=true";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<WfoMatchResponse>(stream, cancellationToken: cancellationToken);
    }

    private async Task<WfoNameDetails> GetNameDetailsAsync(string wfoId, CancellationToken cancellationToken)
    {
        var url = string.Format(StableDataEndpoint, Uri.EscapeDataString(wfoId));
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(payload);

        var details = new WfoNameDetails();
        var allUris = new List<string>();

        foreach (var node in doc.RootElement.EnumerateObject())
        {
            if (node.Name.EndsWith("/" + wfoId, StringComparison.OrdinalIgnoreCase) ||
                node.Name.EndsWith("/" + wfoId + "-2025-12", StringComparison.OrdinalIgnoreCase) ||
                node.Name.EndsWith("/" + wfoId + "-2025-06", StringComparison.OrdinalIgnoreCase))
            {
                if (TryGetFirstLiteral(node.Value, "https://list.worldfloraonline.org/terms/fullName", out var currentName))
                {
                    details.CurrentName = currentName;
                }

                if (TryGetFirstLiteral(node.Value, "https://list.worldfloraonline.org/terms/rank", out var rank))
                {
                    details.Rank = rank;
                }

                if (node.Value.TryGetProperty("https://list.worldfloraonline.org/terms/acceptedNameFor", out _))
                {
                    details.IsAccepted = true;
                }

                if (TryGetFirstUri(node.Value, "https://list.worldfloraonline.org/terms/currentPreferredUsage", out var preferredUsage))
                {
                    details.PreferredUsageUri = preferredUsage;
                }
            }

            CollectUris(node.Value, allUris);
        }

        details.Lsid = ExtractLsid(allUris);
        if (string.IsNullOrWhiteSpace(details.Lsid))
        {
            details.Lsid = ExtractLsidFromLiterals(doc.RootElement);
        }

        await PopulateIucnDetailsAsync(details, wfoId, cancellationToken);

        if (!details.IsAccepted && !string.IsNullOrWhiteSpace(details.PreferredUsageUri))
        {
            var acceptedDetails = await ResolveAcceptedNameFromUsageAsync(details.PreferredUsageUri, cancellationToken);
            details.AcceptedName = acceptedDetails.FullName;
            if (string.IsNullOrWhiteSpace(details.Lsid))
            {
                details.Lsid = acceptedDetails.Lsid;
            }
        }

        if (string.IsNullOrWhiteSpace(details.AcceptedName))
        {
            details.AcceptedName = details.CurrentName;
        }

        return details;
    }

    private async Task<WfoResolvedName> ResolveAcceptedNameFromUsageAsync(string preferredUsageUri, CancellationToken cancellationToken)
    {
        var versionedWfoId = preferredUsageUri.Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(versionedWfoId))
        {
            return new WfoResolvedName();
        }

        var url = string.Format(StableDataEndpoint, Uri.EscapeDataString(versionedWfoId));
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(payload);

        foreach (var node in doc.RootElement.EnumerateObject())
        {
            if (!node.Name.EndsWith("/" + versionedWfoId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryGetFirstUri(node.Value, "https://list.worldfloraonline.org/terms/hasName", out var acceptedNameUri))
            {
                var acceptedId = acceptedNameUri.Split('/').LastOrDefault();
                if (string.IsNullOrWhiteSpace(acceptedId))
                {
                    return new WfoResolvedName();
                }

                return await ResolveFullNameAsync(acceptedId, cancellationToken);
            }
        }

        return new WfoResolvedName();
    }

    private async Task<WfoResolvedName> ResolveFullNameAsync(string wfoId, CancellationToken cancellationToken)
    {
        var url = string.Format(StableDataEndpoint, Uri.EscapeDataString(wfoId));
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(payload);
        var allUris = new List<string>();

        foreach (var node in doc.RootElement.EnumerateObject())
        {
            CollectUris(node.Value, allUris);
        }

        var lsid = ExtractLsid(allUris);
        if (string.IsNullOrWhiteSpace(lsid))
        {
            lsid = ExtractLsidFromLiterals(doc.RootElement);
        }

        foreach (var node in doc.RootElement.EnumerateObject())
        {
            if (node.Name.EndsWith("/" + wfoId, StringComparison.OrdinalIgnoreCase) &&
                TryGetFirstLiteral(node.Value, "https://list.worldfloraonline.org/terms/fullName", out var fullName))
            {
                return new WfoResolvedName
                {
                    FullName = fullName,
                    Lsid = lsid
                };
            }
        }

        return new WfoResolvedName
        {
            Lsid = lsid
        };
    }

    private static WfoCandidate ToCandidate(WfoMatchName name)
    {
        return new WfoCandidate
        {
            WfoId = name.wfo_id,
            FullName = name.full_name_plain,
            Placement = name.placement,
            Rank = name.rank,
            FamilyName = ExtractFamilyFromPlacement(name.placement)
        };
    }

    private async Task PopulateCandidateDetailsAsync(WfoCandidate candidate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(candidate.WfoId))
        {
            return;
        }

        var details = await GetNameDetailsAsync(candidate.WfoId, cancellationToken);
        candidate.Lsid = details.Lsid;
        candidate.IsAccepted = details.IsAccepted;
        candidate.SuggestedAcceptedName = details.AcceptedName;
        candidate.IucnGlobalCode = details.IucnGlobalCode;
        candidate.IucnGlobalLabel = details.IucnGlobalLabel;
        if (string.IsNullOrWhiteSpace(candidate.Rank))
        {
            candidate.Rank = details.Rank;
        }
    }

    private async Task PopulateIucnDetailsAsync(WfoNameDetails details, string wfoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(wfoId))
        {
            return;
        }

        try
        {
            var url = string.Format(TaxonPageEndpoint, Uri.EscapeDataString(wfoId));
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            if (TryExtractIucnStatus(html, out var code, out var label))
            {
                details.IucnGlobalCode = code;
                details.IucnGlobalLabel = label;
            }
        }
        catch
        {
            // Best effort only.
        }
    }

    private static bool TryGetFirstLiteral(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array || property.GetArrayLength() == 0)
        {
            return false;
        }

        var first = property[0];
        if (!first.TryGetProperty("value", out var literalValue))
        {
            return false;
        }

        value = literalValue.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryGetFirstUri(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array || property.GetArrayLength() == 0)
        {
            return false;
        }

        var first = property[0];
        if (!first.TryGetProperty("value", out var uriValue))
        {
            return false;
        }

        value = uriValue.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static void CollectUris(JsonElement element, ICollection<string> uris)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("value", out var valueElement) &&
                    valueElement.ValueKind == JsonValueKind.String)
                {
                    var value = valueElement.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        uris.Add(value);
                    }
                }
            }
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                CollectUris(property.Value, uris);
            }
        }
    }

    private static string ExtractLsid(IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            var match = Regex.Match(value, @"urn:lsid:[^""'\s]+", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value;
            }

            var ipniLsid = TryBuildIpniLsid(value);
            if (!string.IsNullOrWhiteSpace(ipniLsid))
            {
                return ipniLsid;
            }
        }

        return string.Empty;
    }

    private static string ExtractLsidFromLiterals(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("value", out var valueElement) &&
                    valueElement.ValueKind == JsonValueKind.String)
                {
                    var value = valueElement.GetString() ?? string.Empty;
                    var lsidMatch = Regex.Match(value, @"urn:lsid:[^""'\s]+", RegexOptions.IgnoreCase);
                    if (lsidMatch.Success)
                    {
                        return lsidMatch.Value;
                    }

                    var ipniLsid = TryBuildIpniLsid(value);
                    if (!string.IsNullOrWhiteSpace(ipniLsid))
                    {
                        return ipniLsid;
                    }
                }
            }

            return string.Empty;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nested = ExtractLsidFromLiterals(property.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return string.Empty;
    }

    private static bool IsRelevantCandidate(string query, WfoCandidate candidate)
    {
        var parsedQuery = SpecieScientificNameHelper.ParseWfoName(query);
        var parsedCandidate = SpecieScientificNameHelper.ParseWfoName(candidate.FullName);
        if (string.IsNullOrWhiteSpace(parsedQuery.Genus) || string.IsNullOrWhiteSpace(parsedQuery.Nome))
        {
            return true;
        }

        return string.Equals(parsedQuery.Genus, parsedCandidate.Genus, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(parsedQuery.Nome, parsedCandidate.Nome, StringComparison.OrdinalIgnoreCase);
    }

    private static string TryBuildIpniLsid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var ipniRecordMatch = Regex.Match(value, @"IPNI record:\s*([0-9-]+)", RegexOptions.IgnoreCase);
        if (ipniRecordMatch.Success)
        {
            return $"urn:lsid:ipni.org:names:{ipniRecordMatch.Groups[1].Value}";
        }

        var urnMatch = Regex.Match(value, @"urn:lsid:ipni\.org:names:([0-9-]+)", RegexOptions.IgnoreCase);
        if (urnMatch.Success)
        {
            return $"urn:lsid:ipni.org:names:{urnMatch.Groups[1].Value}";
        }

        var urlMatch = Regex.Match(value, @"ipni\.org(?:/[^?""'\s]*)?(?:\?[^""'\s]*\b(?:id|idPlantName)\s*=\s*|/names/)([0-9-]+)", RegexOptions.IgnoreCase);
        if (urlMatch.Success)
        {
            return $"urn:lsid:ipni.org:names:{urlMatch.Groups[1].Value}";
        }

        return string.Empty;
    }

    private static string ExtractFamilyFromPlacement(string placement)
    {
        if (string.IsNullOrWhiteSpace(placement))
        {
            return string.Empty;
        }

        var normalized = placement.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return string.Empty;
        }

        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Contains("aceae", StringComparison.OrdinalIgnoreCase))
            {
                return segments[i];
            }
        }

        return string.Empty;
    }

    private static bool TryExtractIucnStatus(string html, out string code, out string label)
    {
        code = string.Empty;
        label = string.Empty;

        if (string.IsNullOrWhiteSpace(html) || !html.Contains("IUCN", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalized = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", " "));
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        var anchorIndex = normalized.IndexOf("IUCN Red List Status", StringComparison.OrdinalIgnoreCase);
        var scope = anchorIndex >= 0
            ? normalized.Substring(anchorIndex, Math.Min(300, normalized.Length - anchorIndex))
            : normalized;

        var knownStatuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Least Concern"] = "LC",
            ["Near Threatened"] = "NT",
            ["Vulnerable"] = "VU",
            ["Endangered"] = "EN",
            ["Critically Endangered"] = "CR",
            ["Extinct in the Wild"] = "EW",
            ["Extinct"] = "EX",
            ["Data Deficient"] = "DD",
            ["Not Evaluated"] = "NE",
            ["Not Applicable"] = "NA",
            ["Non applicabile"] = "NA"
        };

        foreach (var status in knownStatuses)
        {
            if (scope.Contains(status.Key, StringComparison.OrdinalIgnoreCase))
            {
                code = status.Value;
                label = status.Key;
                return true;
            }
        }

        var codeMatch = Regex.Match(scope, @"\b(LC|NT|VU|EN|CR|EW|EX|DD|NE|NA)\b", RegexOptions.IgnoreCase);
        if (codeMatch.Success)
        {
            code = codeMatch.Groups[1].Value.ToUpperInvariant();
            label = code;
            return true;
        }

        return false;
    }

    private sealed class WfoNameDetails
    {
        public bool IsAccepted { get; set; }

        public string CurrentName { get; set; } = string.Empty;

        public string AcceptedName { get; set; } = string.Empty;

        public string Lsid { get; set; } = string.Empty;

        public string PreferredUsageUri { get; set; } = string.Empty;

        public string Rank { get; set; } = string.Empty;

        public string IucnGlobalCode { get; set; } = string.Empty;

        public string IucnGlobalLabel { get; set; } = string.Empty;
    }

    private sealed class WfoResolvedName
    {
        public string FullName { get; set; } = string.Empty;

        public string Lsid { get; set; } = string.Empty;
    }

    private sealed class WfoMatchResponse
    {
        public string inputString { get; set; }

        public string searchString { get; set; }

        public WfoMatchName match { get; set; }

        public List<WfoMatchName> candidates { get; set; }

        public List<string> narrative { get; set; }
    }

    private sealed class WfoMatchName
    {
        [JsonPropertyName("wfo_id")]
        public string wfo_id { get; set; }

        [JsonPropertyName("full_name_plain")]
        public string full_name_plain { get; set; }

        [JsonPropertyName("placement")]
        public string placement { get; set; }

        [JsonPropertyName("rank")]
        public string rank { get; set; }
    }
}

public sealed class WfoCheckResult
{
    public WfoMatchStatus Status { get; set; }

    public string CurrentScientificName { get; set; } = string.Empty;

    public WfoCandidate Match { get; set; }

    public List<WfoCandidate> Candidates { get; set; } = new();

    public List<string> TriedQueries { get; set; } = new();

    public List<string> Narrative { get; set; } = new();
}

public sealed class WfoCandidate
{
    public string WfoId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Placement { get; set; } = string.Empty;

    public string Lsid { get; set; } = string.Empty;

    public bool IsAccepted { get; set; }

    public string SuggestedAcceptedName { get; set; } = string.Empty;

    public string Rank { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public string QueryUsed { get; set; } = string.Empty;

    public string IucnGlobalCode { get; set; } = string.Empty;

    public string IucnGlobalLabel { get; set; } = string.Empty;
}

public enum WfoMatchStatus
{
    Accepted,
    Synonym,
    Ambiguous,
    NotFound
}
