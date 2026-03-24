using System.Text.RegularExpressions;
using UPlant.Models.DB;

namespace UPlant.Services;

public static class SpecieScientificNameHelper
{
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string Compose(string genus, string nome, string autori, string subspecie, string autorisub, string varieta, string autorivar, string cult, string autoricult)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(genus))
        {
            parts.Add(genus.Trim());
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            parts.Add(nome.Trim());
        }

        if (!string.IsNullOrWhiteSpace(autori))
        {
            parts.Add(autori.Trim());
        }

        if (!string.IsNullOrWhiteSpace(subspecie))
        {
            parts.Add($"subsp. {subspecie.Trim()}");
            if (!string.IsNullOrWhiteSpace(autorisub))
            {
                parts.Add(autorisub.Trim());
            }
        }

        if (!string.IsNullOrWhiteSpace(varieta))
        {
            parts.Add($"var. {varieta.Trim()}");
            if (!string.IsNullOrWhiteSpace(autorivar))
            {
                parts.Add(autorivar.Trim());
            }
        }

        if (!string.IsNullOrWhiteSpace(cult))
        {
            parts.Add($"'{cult.Trim()}'");
            if (!string.IsNullOrWhiteSpace(autoricult))
            {
                parts.Add(autoricult.Trim());
            }
        }

        return NormalizeSpacing(string.Join(" ", parts));
    }

    public static string Compose(Specie specie, string genus)
    {
        return Compose(genus, specie.nome, specie.autori, specie.subspecie, specie.autorisub, specie.varieta, specie.autorivar, specie.cult, specie.autoricult);
    }

    public static IReadOnlyList<string> BuildWfoQueries(Specie specie, string genus)
    {
        var queries = new List<string>();

        if (!string.IsNullOrWhiteSpace(genus) && !string.IsNullOrWhiteSpace(specie.nome))
        {
            queries.Add(NormalizeSpacing($"{genus} {specie.nome} {specie.autori}"));

            if (!string.IsNullOrWhiteSpace(specie.subspecie))
            {
                queries.Add(NormalizeSpacing($"{genus} {specie.nome} subsp. {specie.subspecie} {specie.autorisub}"));
            }

            if (!string.IsNullOrWhiteSpace(specie.varieta))
            {
                queries.Add(NormalizeSpacing($"{genus} {specie.nome} var. {specie.varieta} {specie.autorivar}"));
            }
        }

        return queries
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static ParsedScientificName ParseWfoName(string fullName)
    {
        var parsed = new ParsedScientificName();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return parsed;
        }

        var clean = NormalizeSpacing(fullName);
        var cultivarMatch = Regex.Match(clean, @"'(?<cult>[^']+)'(?:\s+(?<autoricult>.+))?$");
        if (cultivarMatch.Success)
        {
            parsed.Cult = cultivarMatch.Groups["cult"].Value.Trim();
            parsed.AutoriCult = cultivarMatch.Groups["autoricult"].Value.Trim();
            clean = NormalizeSpacing(clean[..cultivarMatch.Index]);
        }

        var tokens = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (tokens.Count == 0)
        {
            return parsed;
        }

        parsed.Genus = tokens[0];
        if (tokens.Count > 1)
        {
            parsed.Nome = tokens[1];
        }

        var rankIndex = tokens.FindIndex(t => t.Equals("subsp.", StringComparison.OrdinalIgnoreCase) || t.Equals("var.", StringComparison.OrdinalIgnoreCase));
        if (rankIndex >= 0)
        {
            parsed.Autori = rankIndex > 2 ? string.Join(" ", tokens.Skip(2).Take(rankIndex - 2)) : string.Empty;

            if (tokens[rankIndex].Equals("subsp.", StringComparison.OrdinalIgnoreCase))
            {
                if (tokens.Count > rankIndex + 1)
                {
                    parsed.Subspecie = tokens[rankIndex + 1];
                }
                parsed.AutoriSub = tokens.Count > rankIndex + 2 ? string.Join(" ", tokens.Skip(rankIndex + 2)) : string.Empty;
            }
            else
            {
                if (tokens.Count > rankIndex + 1)
                {
                    parsed.Varieta = tokens[rankIndex + 1];
                }
                parsed.AutoriVar = tokens.Count > rankIndex + 2 ? string.Join(" ", tokens.Skip(rankIndex + 2)) : string.Empty;
            }
        }
        else
        {
            parsed.Autori = tokens.Count > 2 ? string.Join(" ", tokens.Skip(2)) : string.Empty;
        }

        return parsed;
    }

    public static string NormalizeSpacing(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : MultiSpaceRegex.Replace(value.Trim(), " ");
    }
}

public sealed class ParsedScientificName
{
    public string Genus { get; set; }

    public string Nome { get; set; }

    public string Autori { get; set; }

    public string Subspecie { get; set; }

    public string AutoriSub { get; set; }

    public string Varieta { get; set; }

    public string AutoriVar { get; set; }

    public string Cult { get; set; }

    public string AutoriCult { get; set; }
}
