using UPlant.Models.DB;
using UPlant.Services;

namespace UPlant.Models.ViewModels;

public sealed class SpecieWfoReviewViewModel
{
    public Specie Specie { get; set; }

    public string CurrentGenusName { get; set; } = string.Empty;

    public WfoCheckResult CheckResult { get; set; } = new();

    public ApplyWfoDecisionInput Form { get; set; } = new();

    public WfoApplicationOption AcceptedOption { get; set; }

    public WfoApplicationOption MatchedOption { get; set; }

    public List<WfoApplicationOption> CandidateOptions { get; set; } = new();

    public List<WfoApplicationOption> AcceptedCandidateOptions { get; set; } = new();

    public List<WfoApplicationOption> SynonymCandidateOptions { get; set; } = new();

    public List<WfoAcceptedCandidateGroup> AcceptedCandidateGroups { get; set; } = new();

    public string SuggestedMissingGenusName { get; set; } = string.Empty;

    public string SuggestedMissingFamilyName { get; set; } = string.Empty;

    public bool CanCreateSuggestedGenus { get; set; }

    public string CurrentIucnGlobalCode { get; set; } = string.Empty;

    public string CurrentIucnGlobalDescription { get; set; } = string.Empty;
}

public sealed class ApplyWfoDecisionInput
{
    public Guid SpecieId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string ValidationStatusName { get; set; } = string.Empty;

    public string SelectedWfoId { get; set; } = string.Empty;

    public string AcceptedFullName { get; set; } = string.Empty;

    public string Lsid { get; set; } = string.Empty;

    public string GenusName { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Autori { get; set; } = string.Empty;

    public string Subspecie { get; set; } = string.Empty;

    public string AutoriSub { get; set; } = string.Empty;

    public string Varieta { get; set; } = string.Empty;

    public string AutoriVar { get; set; } = string.Empty;

    public string Cult { get; set; } = string.Empty;

    public string AutoriCult { get; set; } = string.Empty;

    public bool ApplySuggestedIucnGlobal { get; set; }

    public string SuggestedIucnGlobalCode { get; set; } = string.Empty;

    public string SuggestedIucnGlobalLabel { get; set; } = string.Empty;
}

public sealed class WfoApplicationOption
{
    public string ButtonLabel { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ValidationStatusName { get; set; } = string.Empty;

    public bool IsAccepted { get; set; }

    public string AcceptedName { get; set; } = string.Empty;

    public string Rank { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public string WfoId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Lsid { get; set; } = string.Empty;

    public string GenusName { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Autori { get; set; } = string.Empty;

    public string Subspecie { get; set; } = string.Empty;

    public string AutoriSub { get; set; } = string.Empty;

    public string Varieta { get; set; } = string.Empty;

    public string AutoriVar { get; set; } = string.Empty;

    public string Cult { get; set; } = string.Empty;

    public string AutoriCult { get; set; } = string.Empty;

    public string IucnGlobalCode { get; set; } = string.Empty;

    public string IucnGlobalLabel { get; set; } = string.Empty;
}

public sealed class CreateMissingGenusInput
{
    public Guid SpecieId { get; set; }

    public string GenusName { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public string PendingFormJson { get; set; } = string.Empty;
}

public sealed class WfoAcceptedCandidateGroup
{
    public WfoApplicationOption AcceptedOption { get; set; }

    public List<WfoApplicationOption> SupportingSynonyms { get; set; } = new();

    public int MatchScore { get; set; }

    public bool HasExactAcceptedMatch { get; set; }
}

public sealed class SpecieBulkImportWfoViewModel
{
    public string NamesText { get; set; } = string.Empty;

    public List<SpecieBulkImportResultRow> Results { get; set; } = new();
}

public sealed class SpecieBulkImportResultRow
{
    public string InputName { get; set; } = string.Empty;

    public string FinalName { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
}
