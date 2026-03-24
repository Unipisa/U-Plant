using UPlant.Models.DB;
using UPlant.Services;

namespace UPlant.Models.ViewModels;

public sealed class SpecieWfoReviewViewModel
{
    public Specie Specie { get; set; }

    public string CurrentGenusName { get; set; } = string.Empty;

    public WfoCheckResult CheckResult { get; set; } = new();
}

public sealed class ApplyWfoDecisionInput
{
    public Guid SpecieId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string SelectedWfoId { get; set; } = string.Empty;

    public string AcceptedFullName { get; set; } = string.Empty;

    public string Lsid { get; set; } = string.Empty;
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
