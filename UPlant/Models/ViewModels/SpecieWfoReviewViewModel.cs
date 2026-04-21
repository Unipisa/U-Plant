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

    public bool ReturnToAudit { get; set; }

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

    public bool ReturnToAudit { get; set; }

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

public sealed class SpecieWfoNomenclatureImportViewModel
{
    public string SourceUrl { get; set; } = string.Empty;

    public bool ForceImport { get; set; }

    public string DownloadedFileToken { get; set; } = string.Empty;

    public string DownloadedFileName { get; set; } = string.Empty;

    public string DownloadedFileUrl { get; set; } = string.Empty;

    public string DownloadedFileMessage { get; set; } = string.Empty;

    public string OfficialDatasetLabel { get; set; } = string.Empty;

    public bool IsLatestDatasetAvailable { get; set; }

    public bool CachedSnapshotIsCurrent { get; set; }

    public string LocalDatasetLabel { get; set; } = string.Empty;

    public bool CanImport { get; set; }

    public bool CanStartFreshImport { get; set; }

    public bool HasInterruptedImport { get; set; }

    public string InterruptedImportMessage { get; set; } = string.Empty;

    public DateTime? InterruptedImportUpdatedAtUtc { get; set; }

    public int ExistingFamiliesCount { get; set; }

    public int ExistingGeneraCount { get; set; }

    public int ExistingSpeciesCount { get; set; }

    public int ExistingAccessioniCount { get; set; }

    public bool HasAccessionRelations { get; set; }

    public SpecieWfoNomenclatureImportSummary Summary { get; set; } = new();
}

public sealed class SpecieWfoNomenclatureImportSummary
{
    public int ImportedFamilies { get; set; }

    public int ImportedGenera { get; set; }

    public int ImportedSpecies { get; set; }

    public int SkippedRows { get; set; }

    public List<string> Messages { get; set; } = new();
}

public sealed class SpecieWfoDatabaseAuditViewModel
{
    public string OfficialDatasetLabel { get; set; } = string.Empty;

    public int TotalSpeciesCount { get; set; }

    public int AlreadyWfoCount { get; set; }

    public int DefaultMaxSpeciesToProcess { get; set; } = 20;

    public bool DefaultIncludePerfectAccepted { get; set; } = true;

    public bool DefaultIncludePerfectSynonym { get; set; } = true;

    public bool DefaultIncludeAmbiguous { get; set; } = true;

    public bool DefaultIncludeNoMatch { get; set; } = true;

    public bool DefaultIncludeUnchecked { get; set; } = true;

    public bool DefaultIncludeUnplaced { get; set; } = true;

    public bool HasCachedAudit { get; set; }

    public DateTime? CachedAuditUpdatedAtUtc { get; set; }

    public string CachedAuditJson { get; set; } = string.Empty;
}

public sealed class ApplyWfoAuditAcceptedInput
{
    public Guid SpecieId { get; set; }

    public string Lsid { get; set; } = string.Empty;

    public string AcceptedFullName { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public bool AutoCreateMissingGenus { get; set; }
}

public sealed class StartWfoDatabaseAuditInput
{
    public int MaxSpeciesToProcess { get; set; } = 20;

    public bool IncludePerfectAccepted { get; set; } = true;

    public bool IncludePerfectSynonym { get; set; } = true;

    public bool IncludeAmbiguous { get; set; } = true;

    public bool IncludeNoMatch { get; set; } = true;

    public bool IncludeUnchecked { get; set; } = true;

    public bool IncludeUnplaced { get; set; } = true;
}
