using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UPlant.Models.DB;

namespace UPlant.Services;

public interface IWorldFloraOnlineService
{
    Task<WfoCheckResult> CheckAsync(Specie specie, string genusName, CancellationToken cancellationToken = default);
}

public sealed class WorldFloraOnlineService : IWorldFloraOnlineService
{
    private const string MatchingEndpoint = "https://list.worldfloraonline.org/matching_rest.php";
    private const string StableDataEndpoint = "https://list.worldfloraonline.org/sw_data.php?format=json&wfo={0}";

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

                result.Match = matched;
                result.Status = details.IsAccepted ? WfoMatchStatus.Accepted : WfoMatchStatus.Synonym;
                return result;
            }

            foreach (var candidate in matchResponse.candidates ?? Enumerable.Empty<WfoMatchName>())
            {
                var mapped = ToCandidate(candidate);
                mapped.QueryUsed = query;
                if (result.Candidates.All(c => !string.Equals(c.WfoId, mapped.WfoId, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Candidates.Add(mapped);
                }
            }
        }

        result.Status = result.Candidates.Count > 0 ? WfoMatchStatus.Ambiguous : WfoMatchStatus.NotFound;
        return result;
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

        if (!details.IsAccepted && !string.IsNullOrWhiteSpace(details.PreferredUsageUri))
        {
            details.AcceptedName = await ResolveAcceptedNameFromUsageAsync(details.PreferredUsageUri, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(details.AcceptedName))
        {
            details.AcceptedName = details.CurrentName;
        }

        return details;
    }

    private async Task<string> ResolveAcceptedNameFromUsageAsync(string preferredUsageUri, CancellationToken cancellationToken)
    {
        var versionedWfoId = preferredUsageUri.Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(versionedWfoId))
        {
            return string.Empty;
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
                    return string.Empty;
                }

                return await ResolveFullNameAsync(acceptedId, cancellationToken);
            }
        }

        return string.Empty;
    }

    private async Task<string> ResolveFullNameAsync(string wfoId, CancellationToken cancellationToken)
    {
        var url = string.Format(StableDataEndpoint, Uri.EscapeDataString(wfoId));
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(payload);

        foreach (var node in doc.RootElement.EnumerateObject())
        {
            if (node.Name.EndsWith("/" + wfoId, StringComparison.OrdinalIgnoreCase) &&
                TryGetFirstLiteral(node.Value, "https://list.worldfloraonline.org/terms/fullName", out var fullName))
            {
                return fullName;
            }
        }

        return string.Empty;
    }

    private static WfoCandidate ToCandidate(WfoMatchName name)
    {
        return new WfoCandidate
        {
            WfoId = name.wfo_id,
            FullName = name.full_name_plain,
            Placement = name.placement
        };
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
        }

        return string.Empty;
    }

    private sealed class WfoNameDetails
    {
        public bool IsAccepted { get; set; }

        public string CurrentName { get; set; } = string.Empty;

        public string AcceptedName { get; set; } = string.Empty;

        public string Lsid { get; set; } = string.Empty;

        public string PreferredUsageUri { get; set; } = string.Empty;
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

    public string QueryUsed { get; set; } = string.Empty;
}

public enum WfoMatchStatus
{
    Accepted,
    Synonym,
    Ambiguous,
    NotFound
}
