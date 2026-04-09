using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using UPlant.Models.DB;
using UPlant.Models.ViewModels;
using UPlant.Services;

namespace UPlant.Controllers
{
    public class SpecieController : BaseController
    {
        private const int MaxPageSize = 100;
        private const string DefaultWfoDatasetPageUrl = "https://zenodo.org/records/18007552";
        private const string DefaultWfoDatasetDownloadUrl = "https://zenodo.org/records/18007552/files/wfo_plantlist_2025-12.zip?download=1";
        private const string DefaultWfoDatasetFileName = "wfo_plantlist_2025-12.zip";
        private const string DefaultWfoDatasetLabel = "December 2025";
        private readonly Entities _context;
        private readonly IWorldFloraOnlineService _worldFloraOnlineService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SpecieController> _logger;
        private static readonly ConcurrentDictionary<string, WfoNomenclatureImportJobState> _wfoImportJobs = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, WfoDatabaseAuditJobState> _wfoAuditJobs = new(StringComparer.OrdinalIgnoreCase);

        public SpecieController(
            Entities context,
            IWorldFloraOnlineService worldFloraOnlineService,
            IHttpClientFactory httpClientFactory,
            IServiceScopeFactory scopeFactory,
            ILogger<SpecieController> logger)
        {
            _context = context;
            _worldFloraOnlineService = worldFloraOnlineService;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var latestCachedSnapshot = TryLoadLatestCachedWfoSnapshotAsync(CancellationToken.None).GetAwaiter().GetResult();
            var localDatasetLabel = latestCachedSnapshot == null
                ? "nessun dataset locale"
                : GetDatasetLabelFromFileName(latestCachedSnapshot.FileName);
            ViewBag.WfoDatasetLabel = DefaultWfoDatasetLabel;
            ViewBag.WfoDatasetIsLatest = latestCachedSnapshot != null && IsCurrentOfficialWfoDataset(latestCachedSnapshot.FileName);
            ViewBag.WfoLocalDatasetLabel = localDatasetLabel;
            ViewBag.WfoDatasetMessage = latestCachedSnapshot == null
                ? "Dataset WFO ufficiale non ancora scaricato in cache."
                : (IsCurrentOfficialWfoDataset(latestCachedSnapshot.FileName)
                    ? $"Dataset WFO in cache allineato all'ultima versione ({DefaultWfoDatasetLabel})."
                    : $"In cache hai un dataset WFO precedente. E' disponibile una versione piu aggiornata: {DefaultWfoDatasetLabel}. Riscarica.");
            return View();
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> IndexAllData()
        {
            try
            {
                var isAdministrator = User.IsInRole("Administrator");

                var data = await _context.Specie
                    .AsNoTracking()
                    .Select(s => new SpecieIndexRow
                    {
                        Id = s.id,
                        ScientificName = s.nome_scientifico ?? string.Empty,
                        ValidationStatus = s.validazione_tassonomicaNavigation != null
                            ? s.validazione_tassonomicaNavigation.descrizione ?? string.Empty
                            : string.Empty,
                        InsertedAt = s.data_inserimento,
                        Lsid = s.lsid ?? string.Empty,
                        CommonName = s.nome_comune ?? string.Empty,
                        EnglishCommonName = s.nome_comune_en ?? string.Empty,
                        Family = s.genereNavigation != null && s.genereNavigation.famigliaNavigation != null
                            ? s.genereNavigation.famigliaNavigation.descrizione ?? string.Empty
                            : string.Empty,
                        Genus = s.genereNavigation != null
                            ? s.genereNavigation.descrizione ?? string.Empty
                            : string.Empty,
                        Kingdom = s.regnoNavigation != null
                            ? s.regnoNavigation.descrizione ?? string.Empty
                            : string.Empty,
                        Range = s.arealeNavigation != null
                            ? s.arealeNavigation.descrizione ?? string.Empty
                            : string.Empty,
                        Cites = s.citesNavigation != null
                            ? s.citesNavigation.codice ?? string.Empty
                            : string.Empty,
                        IucnGlobal = s.iucn_globaleNavigation != null
                            ? s.iucn_globaleNavigation.codice ?? string.Empty
                            : string.Empty,
                        IucnLocal = s.iucn_italiaNavigation != null
                            ? s.iucn_italiaNavigation.codice ?? string.Empty
                            : string.Empty,
                        Note = s.note ?? string.Empty,
                        HasAccessioni = s.Accessioni.Any()
                    })
                    .OrderBy(x => x.ScientificName)
                    .ToListAsync();

                var result = new
                {
                    data = data.Select(item => new
                    {
                        id = item.Id,
                        scientificName = item.ScientificName,
                        validationStatus = item.ValidationStatus,
                        insertedAt = item.InsertedAt.ToString("dd/MM/yyyy HH:mm"),
                        lsid = item.Lsid,
                        commonName = item.CommonName,
                        englishCommonName = item.EnglishCommonName,
                        family = item.Family,
                        genus = item.Genus,
                        kingdom = item.Kingdom,
                        range = item.Range,
                        cites = item.Cites,
                        iucnGlobal = item.IucnGlobal,
                        iucnLocal = item.IucnLocal,
                        notePreview = TruncateNote(item.Note),
                        canDelete = !item.HasAccessioni,
                        showActions = isAdministrator
                    }).ToList()
                };

                return Content(JsonSerializer.Serialize(result), "application/json");
            }
            catch (Exception ex)
            {
                var errorResult = new
                {
                    data = new List<object>(),
                    error = ex.Message
                };

                return StatusCode(500, Content(JsonSerializer.Serialize(errorResult), "application/json").Content);
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexData()
        {
            var draw = ParseInt(Request.Query["draw"], 1);
            var start = Math.Max(ParseInt(Request.Query["start"], 0), 0);
            var length = ParseInt(Request.Query["length"], 25);
            if (length <= 0)
            {
                length = 25;
            }

            length = Math.Min(length, MaxPageSize);

            var search = Request.Query["search[value]"].FirstOrDefault()?.Trim();
            var orderColumn = ParseInt(Request.Query["order[0][column]"], 1);
            var orderDirection = Request.Query["order[0][dir]"].FirstOrDefault();
            var descending = string.Equals(orderDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var isAdministrator = User.IsInRole("Administrator");

            var query = _context.Specie
                .AsNoTracking()
                .Select(s => new SpecieIndexRow
                {
                    Id = s.id,
                    ScientificName = s.nome_scientifico ?? string.Empty,
                    ValidationStatus = s.validazione_tassonomicaNavigation != null
                        ? s.validazione_tassonomicaNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    InsertedAt = s.data_inserimento,
                    Lsid = s.lsid ?? string.Empty,
                    CommonName = s.nome_comune ?? string.Empty,
                    EnglishCommonName = s.nome_comune_en ?? string.Empty,
                    Family = s.genereNavigation != null && s.genereNavigation.famigliaNavigation != null
                        ? s.genereNavigation.famigliaNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Genus = s.genereNavigation != null
                        ? s.genereNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Kingdom = s.regnoNavigation != null
                        ? s.regnoNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Range = s.arealeNavigation != null
                        ? s.arealeNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Cites = s.citesNavigation != null
                        ? s.citesNavigation.codice ?? string.Empty
                        : string.Empty,
                    IucnGlobal = s.iucn_globaleNavigation != null
                        ? s.iucn_globaleNavigation.codice ?? string.Empty
                        : string.Empty,
                    IucnLocal = s.iucn_italiaNavigation != null
                        ? s.iucn_italiaNavigation.codice ?? string.Empty
                        : string.Empty,
                    Note = s.note ?? string.Empty,
                    HasAccessioni = s.Accessioni.Any()
                });

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.ScientificName.Contains(search) ||
                    s.ValidationStatus.Contains(search) ||
                    s.Lsid.Contains(search) ||
                    s.CommonName.Contains(search) ||
                    s.EnglishCommonName.Contains(search) ||
                    s.Family.Contains(search) ||
                    s.Genus.Contains(search) ||
                    s.Kingdom.Contains(search) ||
                    s.Range.Contains(search) ||
                    s.Cites.Contains(search) ||
                    s.IucnGlobal.Contains(search) ||
                    s.IucnLocal.Contains(search) ||
                    s.Note.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();

            query = ApplyOrdering(query, orderColumn, descending);

            var page = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            var result = new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = page.Select(item => new
                {
                    id = item.Id,
                    scientificName = item.ScientificName,
                    validationStatus = item.ValidationStatus,
                    insertedAt = item.InsertedAt.ToString("dd/MM/yyyy HH:mm"),
                    lsid = item.Lsid,
                    commonName = item.CommonName,
                    englishCommonName = item.EnglishCommonName,
                    family = item.Family,
                    genus = item.Genus,
                    kingdom = item.Kingdom,
                    range = item.Range,
                    cites = item.Cites,
                    iucnGlobal = item.IucnGlobal,
                    iucnLocal = item.IucnLocal,
                    notePreview = TruncateNote(item.Note),
                    canDelete = !item.HasAccessioni,
                    showActions = isAdministrator
                }).ToList()
            };

            return Json(result);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie
                .Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation)
                .Include(s => s.validazione_tassonomicaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);

            if (specie == null)
            {
                return NotFound();
            }

            return View(specie);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateSelectionsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,genere,validazione_tassonomica,nome,nome_scientifico,data_inserimento,lsid,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
        {
            if (ModelState.IsValid)
            {
                specie.id = Guid.NewGuid();
                specie.validazione_tassonomica = await EnsureDefaultValidazioneTassonomicaAsync("N.D.");
                specie.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                specie.data_inserimento = DateTime.Now;
                _context.Add(specie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectionsAsync(specie);
            return View(specie);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie.FindAsync(id);
            if (specie == null)
            {
                return NotFound();
            }

            await PopulateSelectionsAsync(specie);
            return View(specie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,genere,validazione_tassonomica,nome,nome_scientifico,data_inserimento,lsid,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
        {
            if (id != specie.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Specie.FirstOrDefaultAsync(x => x.id == id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.genere = specie.genere;
                existing.validazione_tassonomica = specie.validazione_tassonomica;
                existing.nome = specie.nome;
                existing.autori = specie.autori;
                existing.regno = specie.regno;
                existing.areale = specie.areale;
                existing.subspecie = specie.subspecie;
                existing.autorisub = specie.autorisub;
                existing.varieta = specie.varieta;
                existing.autorivar = specie.autorivar;
                existing.cult = specie.cult;
                existing.autoricult = specie.autoricult;
                existing.note = specie.note;
                existing.nome_comune = specie.nome_comune;
                existing.nome_comune_en = specie.nome_comune_en;
                existing.iucn_globale = specie.iucn_globale;
                existing.iucn_italia = specie.iucn_italia;
                existing.cites = specie.cites;
                existing.lsid = specie.lsid;
                existing.validazione_tassonomica = await EnsureDefaultValidazioneTassonomicaAsync("A.A.");
                existing.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                existing.data_inserimento = DateTime.Now;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecieExists(specie.id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectionsAsync(specie);
            return View(specie);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie
                .Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation)
                .Include(s => s.validazione_tassonomicaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);

            if (specie == null)
            {
                return NotFound();
            }

            return View(specie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Specie == null)
            {
                return Problem("Entity set 'Entities.Specie'  is null.");
            }

            var specie = await _context.Specie.FindAsync(id);
            if (specie != null)
            {
                _context.Specie.Remove(specie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckWfo(Guid id, CancellationToken cancellationToken)
        {
            var specie = await _context.Specie
                .Include(s => s.genereNavigation)
                .FirstOrDefaultAsync(s => s.id == id, cancellationToken);

            if (specie == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _worldFloraOnlineService.CheckAsync(specie, specie.genereNavigation?.descrizione ?? string.Empty, cancellationToken);
                var reviewUrl = Url.Action(nameof(ReviewWfo), new { id });

                return Json(new
                {
                    serviceAvailable = true,
                    status = result.Status.ToString().ToLowerInvariant(),
                    reviewUrl,
                    currentScientificName = result.CurrentScientificName,
                    triedQueries = result.TriedQueries,
                    narrative = result.Narrative,
                    match = result.Match == null ? null : new
                    {
                        wfoId = result.Match.WfoId,
                        fullName = result.Match.FullName,
                        acceptedName = result.Match.SuggestedAcceptedName,
                        lsid = result.Match.Lsid,
                        isAccepted = result.Match.IsAccepted
                    },
                    candidates = result.Candidates.Select(c => new
                    {
                        wfoId = c.WfoId,
                        fullName = c.FullName,
                        placement = c.Placement
                    })
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    serviceAvailable = false,
                    message = "Il servizio World Flora Online non risponde al momento."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> WfoStatus(CancellationToken cancellationToken)
        {
            var isAvailable = await _worldFloraOnlineService.IsAvailableAsync(cancellationToken);
            return Json(new
            {
                serviceAvailable = isAvailable,
                message = isAvailable
                    ? "Servizio World Flora Online disponibile."
                    : "Servizio World Flora Online non disponibile."
            });
        }

        [HttpGet]
        public async Task<IActionResult> OpenWfo(Guid id, CancellationToken cancellationToken)
        {
            var specie = await _context.Specie
                .Include(s => s.genereNavigation)
                .Include(s => s.validazione_tassonomicaNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .FirstOrDefaultAsync(s => s.id == id, cancellationToken);

            if (specie == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var scientificName = SpecieScientificNameHelper.Compose(specie, specie.genereNavigation?.descrizione ?? string.Empty);
            var validationName = specie.validazione_tassonomicaNavigation?.descrizione ?? string.Empty;

            if (string.Equals(validationName, "WFO", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var result = await _worldFloraOnlineService.CheckAsync(specie, specie.genereNavigation?.descrizione ?? string.Empty, cancellationToken);
                    var wfoId = result.Match?.WfoId;

                    if (string.IsNullOrWhiteSpace(wfoId))
                    {
                        wfoId = result.Candidates
                            .Where(x => x.IsAccepted)
                            .OrderBy(x => string.Equals(x.FullName, scientificName, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                            .ThenBy(x => x.FullName)
                            .Select(x => x.WfoId)
                            .FirstOrDefault();
                    }

                    if (!string.IsNullOrWhiteSpace(wfoId))
                    {
                        return Redirect(BuildWfoStableUrl(wfoId));
                    }
                }
                catch
                {
                    return Redirect(BuildWfoSearchUrl(scientificName));
                }
            }

            return Redirect(BuildWfoSearchUrl(scientificName));
        }

        [HttpGet]
        public async Task<IActionResult> ReviewWfo(Guid id, CancellationToken cancellationToken)
        {
            var specie = await _context.Specie
                .Include(s => s.genereNavigation)
                .Include(s => s.validazione_tassonomicaNavigation)
                .FirstOrDefaultAsync(s => s.id == id, cancellationToken);

            if (specie == null)
            {
                return NotFound();
            }

            var checkResult = await _worldFloraOnlineService.CheckAsync(specie, specie.genereNavigation?.descrizione ?? string.Empty, cancellationToken);
            var model = new SpecieWfoReviewViewModel
            {
                Specie = specie,
                CurrentGenusName = specie.genereNavigation?.descrizione ?? string.Empty,
                CheckResult = checkResult,
                Form = BuildFormInput(specie, specie.genereNavigation?.descrizione ?? string.Empty, specie.validazione_tassonomicaNavigation?.descrizione),
                CurrentIucnGlobalCode = specie.iucn_globaleNavigation?.codice ?? string.Empty,
                CurrentIucnGlobalDescription = specie.iucn_globaleNavigation?.descrizione ?? string.Empty
            };

            if (TempData.TryGetValue("PendingWfoForm", out var pendingWfoFormJsonObj) &&
                pendingWfoFormJsonObj is string pendingWfoFormJson &&
                !string.IsNullOrWhiteSpace(pendingWfoFormJson))
            {
                try
                {
                    var pendingForm = JsonSerializer.Deserialize<ApplyWfoDecisionInput>(pendingWfoFormJson);
                    if (pendingForm != null && pendingForm.SpecieId == specie.id)
                    {
                        model.Form = pendingForm;
                    }
                }
                catch (JsonException)
                {
                    TempData.Remove("PendingWfoForm");
                }
            }

            if (checkResult.Match != null)
            {
                model.MatchedOption = BuildOption(
                    "Aggiorna il form con il match WFO",
                    "Usa il nome trovato da WFO e valorizza anche LSID.",
                    checkResult.Match.IsAccepted ? "WFO" : "A.A.",
                    checkResult.Match.WfoId,
                    checkResult.Match.FullName,
                    checkResult.Match.Lsid,
                    iucnGlobalCode: checkResult.Match.IucnGlobalCode,
                    iucnGlobalLabel: checkResult.Match.IucnGlobalLabel);

                if (!string.IsNullOrWhiteSpace(checkResult.Match.SuggestedAcceptedName))
                {
                    model.AcceptedOption = BuildOption(
                        "Applica il nome accettato WFO",
                        "Carica nel form il nome accettato suggerito da WFO senza salvare subito.",
                        "WFO",
                        checkResult.Match.WfoId,
                        checkResult.Match.SuggestedAcceptedName,
                        checkResult.Match.Lsid,
                        iucnGlobalCode: checkResult.Match.IucnGlobalCode,
                        iucnGlobalLabel: checkResult.Match.IucnGlobalLabel);
                }
                else
                {
                    model.AcceptedOption = model.MatchedOption;
                }
            }

            model.CandidateOptions = checkResult.Candidates
                .Select(candidate => BuildOption(
                    "Carica nel form",
                    string.IsNullOrWhiteSpace(candidate.Placement) ? candidate.FullName : $"{candidate.FullName} ({candidate.Placement})",
                    candidate.IsAccepted ? "WFO" : "A.A.",
                    candidate.WfoId,
                    candidate.FullName,
                    candidate.Lsid,
                    candidate.IsAccepted,
                    candidate.SuggestedAcceptedName,
                    candidate.Rank,
                    candidate.FamilyName,
                    candidate.IucnGlobalCode,
                    candidate.IucnGlobalLabel))
                .ToList();

            model.AcceptedCandidateOptions = model.CandidateOptions
                .Where(x => x.IsAccepted)
                .ToList();

            var normalizedCurrentScientificName = SpecieScientificNameHelper.NormalizeSpacing(specie.nome_scientifico);
            var hasExactAcceptedCandidate = model.AcceptedCandidateOptions.Any(x =>
                string.Equals(
                    SpecieScientificNameHelper.NormalizeSpacing(x.FullName),
                    normalizedCurrentScientificName,
                    StringComparison.OrdinalIgnoreCase));

            var derivedAcceptedOptions = model.CandidateOptions
                .Where(x => !x.IsAccepted && !string.IsNullOrWhiteSpace(x.AcceptedName))
                .GroupBy(x => x.AcceptedName, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var source = group.First();
                    return BuildOption(
                        "Carica nel form",
                        "Nome accettato derivato dal sinonimo trovato in WFO.",
                        "WFO",
                        source.WfoId,
                        source.AcceptedName,
                        source.Lsid,
                        true,
                        source.AcceptedName,
                        source.Rank,
                        source.FamilyName,
                        source.IucnGlobalCode,
                        source.IucnGlobalLabel);
                })
                .ToList();

            if (!hasExactAcceptedCandidate)
            {
                foreach (var acceptedOption in derivedAcceptedOptions)
                {
                    if (model.AcceptedCandidateOptions.All(x => !string.Equals(x.FullName, acceptedOption.FullName, StringComparison.OrdinalIgnoreCase)))
                    {
                        model.AcceptedCandidateOptions.Add(acceptedOption);
                    }
                }
            }

            model.AcceptedCandidateOptions = model.AcceptedCandidateOptions
                .OrderBy(x => x.FullName)
                .ToList();

            model.SynonymCandidateOptions = model.CandidateOptions
                .Where(x => !x.IsAccepted)
                .ToList();

            model.AcceptedCandidateGroups = BuildAcceptedCandidateGroups(
                specie,
                model.AcceptedCandidateOptions,
                model.SynonymCandidateOptions);

            await PopulateMissingGenusSuggestionAsync(model, cancellationToken);

            return View(model);
        }

        [HttpGet]
        public IActionResult BulkImportWfo()
        {
            return View(new SpecieBulkImportWfoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImportWfo(SpecieBulkImportWfoViewModel model, CancellationToken cancellationToken)
        {
            model.Results = new List<SpecieBulkImportResultRow>();

            var inputNames = (model.NamesText ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var wfoStatusId = await EnsureDefaultValidazioneTassonomicaAsync("WFO");

            foreach (var inputName in inputNames)
            {
                var parsedInput = SpecieScientificNameHelper.ParseWfoName(inputName);
                if (string.IsNullOrWhiteSpace(parsedInput.Genus) || string.IsNullOrWhiteSpace(parsedInput.Nome))
                {
                    model.Results.Add(new SpecieBulkImportResultRow
                    {
                        InputName = inputName,
                        Outcome = "Scartato",
                        Details = "Nome non parsabile."
                    });
                    continue;
                }

                var tempSpecie = new Specie
                {
                    nome = parsedInput.Nome,
                    autori = parsedInput.Autori,
                    subspecie = parsedInput.Subspecie,
                    autorisub = parsedInput.AutoriSub,
                    varieta = parsedInput.Varieta,
                    autorivar = parsedInput.AutoriVar,
                    cult = parsedInput.Cult,
                    autoricult = parsedInput.AutoriCult
                };

                var wfoResult = await _worldFloraOnlineService.CheckAsync(tempSpecie, parsedInput.Genus, cancellationToken);
                var finalName = wfoResult.Match?.IsAccepted == true
                    ? wfoResult.Match.FullName
                    : (string.IsNullOrWhiteSpace(wfoResult.Match?.SuggestedAcceptedName) ? inputName : wfoResult.Match.SuggestedAcceptedName);

                var finalParsed = SpecieScientificNameHelper.ParseWfoName(finalName);
                var genusId = await _context.Generi
                    .Where(g => g.descrizione == finalParsed.Genus)
                    .Select(g => (Guid?)g.id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!genusId.HasValue)
                {
                    model.Results.Add(new SpecieBulkImportResultRow
                    {
                        InputName = inputName,
                        FinalName = finalName,
                        Outcome = "Scartato",
                        Details = $"Genere '{finalParsed.Genus}' non presente in archivio."
                    });
                    continue;
                }

                var normalizedName = SpecieScientificNameHelper.Compose(finalParsed.Genus, finalParsed.Nome, finalParsed.Autori, finalParsed.Subspecie, finalParsed.AutoriSub, finalParsed.Varieta, finalParsed.AutoriVar, finalParsed.Cult, finalParsed.AutoriCult);
                var alreadyExists = await _context.Specie.AnyAsync(s => s.nome_scientifico == normalizedName, cancellationToken);
                if (alreadyExists)
                {
                    model.Results.Add(new SpecieBulkImportResultRow
                    {
                        InputName = inputName,
                        FinalName = normalizedName,
                        Outcome = "Saltato",
                        Details = "Specie già presente."
                    });
                    continue;
                }

                var newSpecie = new Specie
                {
                    id = Guid.NewGuid(),
                    genere = genusId.Value,
                    nome = finalParsed.Nome,
                    autori = finalParsed.Autori,
                    subspecie = finalParsed.Subspecie,
                    autorisub = finalParsed.AutoriSub,
                    varieta = finalParsed.Varieta,
                    autorivar = finalParsed.AutoriVar,
                    cult = finalParsed.Cult,
                    autoricult = finalParsed.AutoriCult,
                    nome_scientifico = normalizedName,
                    lsid = wfoResult.Match?.Lsid,
                    data_inserimento = DateTime.Now,
                    validazione_tassonomica = wfoStatusId
                };

                _context.Specie.Add(newSpecie);

                model.Results.Add(new SpecieBulkImportResultRow
                {
                    InputName = inputName,
                    FinalName = normalizedName,
                    Outcome = "Importato",
                    Details = wfoResult.Status == WfoMatchStatus.Synonym ? "Importato usando il nome accettato WFO." : "Importato da WFO."
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ImportWfoNomenclature(bool forceImport = false, CancellationToken cancellationToken = default)
        {
            return View(await BuildWfoNomenclatureImportViewModelAsync(forceImport, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> WfoDatabaseAudit(CancellationToken cancellationToken)
        {
            var totalSpeciesCount = await CountPendingWfoAuditSpeciesAsync(_context, cancellationToken);
            var alreadyWfoId = await EnsureDefaultValidazioneTassonomicaAsync("WFO");
            var alreadyWfoCount = alreadyWfoId == Guid.Empty
                ? 0
                : await _context.Specie.CountAsync(x => x.validazione_tassonomica == alreadyWfoId, cancellationToken);
            var cachedAudit = await LoadWfoDatabaseAuditCacheAsync(cancellationToken);
            cachedAudit = await FilterWfoDatabaseAuditSnapshotAsync(_context, cachedAudit, cancellationToken);

            return View(new SpecieWfoDatabaseAuditViewModel
            {
                TotalSpeciesCount = totalSpeciesCount,
                AlreadyWfoCount = alreadyWfoCount,
                DefaultMaxSpeciesToProcess = Math.Min(20, Math.Max(1, totalSpeciesCount)),
                DefaultIncludePerfectAccepted = true,
                DefaultIncludePerfectSynonym = true,
                DefaultIncludeAmbiguous = true,
                DefaultIncludeNoMatch = true,
                HasCachedAudit = cachedAudit != null,
                CachedAuditUpdatedAtUtc = cachedAudit?.UpdatedAtUtc,
                CachedAuditJson = cachedAudit == null ? string.Empty : JsonSerializer.Serialize(cachedAudit)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWfoDatabaseAudit(StartWfoDatabaseAuditInput input, bool force = false, CancellationToken cancellationToken = default)
        {
            input ??= new StartWfoDatabaseAuditInput();
            NormalizeWfoAuditInput(input);

            var totalPendingSpeciesCount = await CountPendingWfoAuditSpeciesAsync(_context, cancellationToken);
            var totalSpeciesCount = Math.Min(totalPendingSpeciesCount, input.MaxSpeciesToProcess);

            if (!force)
            {
                var cachedAudit = await LoadWfoDatabaseAuditCacheAsync(cancellationToken);
                cachedAudit = await FilterWfoDatabaseAuditSnapshotAsync(_context, cachedAudit, cancellationToken);
                if (cachedAudit != null &&
                    SnapshotMatchesAuditInput(cachedAudit, input) &&
                    cachedAudit.TotalSpecies == totalSpeciesCount &&
                    GetWfoDatabaseAuditSnapshotCount(cachedAudit) >= totalSpeciesCount)
                {
                    return Json(new
                    {
                        started = false,
                        cached = true,
                        snapshot = cachedAudit
                    });
                }
            }

            CleanupCompletedWfoAuditJobs();

            var jobId = Guid.NewGuid().ToString("N");
            var job = new WfoDatabaseAuditJobState(jobId, totalSpeciesCount, input);
            job.AddMessage($"Audit avviato su {totalSpeciesCount} specie.");
            _wfoAuditJobs[jobId] = job;

            _ = Task.Run(() => RunWfoDatabaseAuditInBackgroundAsync(jobId));

            return Json(new
            {
                started = true,
                jobId,
                statusUrl = Url.Action(nameof(GetWfoDatabaseAuditStatus), new { jobId })
            });
        }

        [HttpGet]
        public IActionResult GetWfoDatabaseAuditStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId) || !_wfoAuditJobs.TryGetValue(jobId, out var job))
            {
                return NotFound(new { message = "Job di audit non trovato." });
            }

            return Json(job.CreateSnapshot());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyWfoAcceptedFromAudit([FromForm] ApplyWfoAuditAcceptedInput input)
        {
            if (input.SpecieId == Guid.Empty)
            {
                return BadRequest(new { message = "Specie non valida." });
            }

            var specie = await _context.Specie.FirstOrDefaultAsync(x => x.id == input.SpecieId);
            if (specie == null)
            {
                return NotFound(new { message = "Specie non trovata." });
            }

            if (!string.IsNullOrWhiteSpace(input.AcceptedFullName))
            {
                if (!await ApplyAcceptedNameAsync(specie, input.AcceptedFullName, input.Lsid))
                {
                    var parsedAccepted = SpecieScientificNameHelper.ParseWfoName(input.AcceptedFullName);
                    var genusName = SpecieScientificNameHelper.NormalizeSpacing(parsedAccepted.Genus);
                    var familyName = SpecieScientificNameHelper.NormalizeSpacing(input.FamilyName);
                    var genusMissing = !string.IsNullOrWhiteSpace(genusName) &&
                        !await _context.Generi.AnyAsync(g => g.descrizione == genusName);

                    if (genusMissing && !string.IsNullOrWhiteSpace(familyName) && !input.AutoCreateMissingGenus)
                    {
                        return Conflict(new
                        {
                            requiresGenusCreation = true,
                            genusName,
                            familyName,
                            message = $"Manca il genere {genusName} (famiglia {familyName}). Vuoi che lo inserisca automaticamente prima di applicare il nome accettato?"
                        });
                    }

                    if (!input.AutoCreateMissingGenus)
                    {
                        return BadRequest(new { message = "Non riesco ad applicare automaticamente il nome accettato con i dati disponibili (genere o famiglia mancanti)." });
                    }

                    var genusCreated = await EnsureGenusForAcceptedNameAsync(input.AcceptedFullName, input.FamilyName);
                    if (!genusCreated || !await ApplyAcceptedNameAsync(specie, input.AcceptedFullName, input.Lsid))
                    {
                        return BadRequest(new { message = "Non riesco ad applicare automaticamente il nome accettato con i dati disponibili (genere o famiglia mancanti)." });
                    }
                }
            }

            specie.validazione_tassonomica = await EnsureDefaultValidazioneTassonomicaAsync("WFO");
            specie.lsid = SpecieScientificNameHelper.NormalizeSpacing(input.Lsid);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportWfoNomenclature(SpecieWfoNomenclatureImportViewModel model, CancellationToken cancellationToken)
        {
            var viewModel = await BuildWfoNomenclatureImportViewModelAsync(model.ForceImport, cancellationToken);
            viewModel.SourceUrl = model.SourceUrl;
            viewModel.ForceImport = model.ForceImport;
            viewModel.DownloadedFileToken = model.DownloadedFileToken;
            viewModel.DownloadedFileName = model.DownloadedFileName;
            viewModel.DownloadedFileUrl = string.IsNullOrWhiteSpace(model.DownloadedFileToken)
                ? string.Empty
                : Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token = model.DownloadedFileToken }) ?? string.Empty;

            if (IsAjaxRequest())
            {
                if (!viewModel.CanImport)
                {
                    return BadRequest(new { message = "L'import non puo partire in questo stato. Usa il checkpoint rilevato oppure riforza il processo dalla pagina import." });
                }

                if (string.IsNullOrWhiteSpace(model.DownloadedFileToken))
                {
                    return BadRequest(new { message = "Scarica prima il file WFO e poi avvia l'import." });
                }

                var ajaxCachedFilePath = GetCachedWfoSnapshotPath(model.DownloadedFileToken);
                if (!System.IO.File.Exists(ajaxCachedFilePath))
                {
                    return BadRequest(new { message = "Il file scaricato non e piu disponibile. Riesegui il download." });
                }

                var organizationId = await GetCurrentUserOrganizationAsync();
                if (!organizationId.HasValue)
                {
                    return BadRequest(new { message = "Non riesco a determinare l'organizzazione corrente per salvare la nomenclatura." });
                }

                CleanupCompletedWfoImportJobs();

                var jobId = Guid.NewGuid().ToString("N");
                var job = new WfoNomenclatureImportJobState(jobId, model.DownloadedFileName, model.DownloadedFileToken);
                job.AddMessage($"Job avviato dal file '{model.DownloadedFileName}'.");
                _wfoImportJobs[jobId] = job;

                _ = Task.Run(() => RunWfoNomenclatureImportInBackgroundAsync(
                    jobId,
                    ajaxCachedFilePath,
                    model.DownloadedFileName,
                    model.DownloadedFileToken,
                    organizationId.Value));

                return Json(new
                {
                    started = true,
                    jobId,
                    statusUrl = Url.Action(nameof(GetWfoNomenclatureImportStatus), new { jobId })
                });
            }

            if (!viewModel.CanImport)
            {
                ModelState.AddModelError(string.Empty, "L'import non puo partire in questo stato. Usa il checkpoint rilevato oppure riforza il processo dalla pagina import.");
                return View(viewModel);
            }

            if (string.IsNullOrWhiteSpace(model.DownloadedFileToken))
            {
                ModelState.AddModelError(nameof(model.DownloadedFileToken), "Scarica prima il file WFO e poi avvia l'import.");
                return View(viewModel);
            }

            var cachedFilePath = GetCachedWfoSnapshotPath(model.DownloadedFileToken);
            if (!System.IO.File.Exists(cachedFilePath))
            {
                ModelState.AddModelError(nameof(model.DownloadedFileToken), "Il file scaricato non e piu disponibile. Riesegui il download.");
                return View(viewModel);
            }

            List<WfoNomenclatureImportRow> rows;
            await using (var stream = System.IO.File.OpenRead(cachedFilePath))
            {
                rows = await ParseWfoNomenclatureSnapshotAsync(stream, model.DownloadedFileName, cancellationToken);
            }

            if (rows.Count == 0)
            {
                ModelState.AddModelError(nameof(model.DownloadedFileToken), "Non ho trovato righe valide da importare nel file scaricato.");
                return View(viewModel);
            }

            var validazioneWfoId = await EnsureDefaultValidazioneTassonomicaAsync("WFO");
            var now = DateTime.Now;

            var importedFamilies = 0;
            var importedGenera = 0;
            var importedSpecies = 0;
            var removedOrphanGenera = 0;
            var removedOrphanFamilies = 0;

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                var familyMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                foreach (var familyName in rows
                    .Select(x => x.FamilyName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x))
                {
                    var family = new Famiglie
                    {
                        id = Guid.NewGuid(),
                        descrizione = TruncateForDb(familyName, 100)
                    };

                    _context.Famiglie.Add(family);
                    familyMap[familyName] = family.id;
                    importedFamilies++;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var genusMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                foreach (var genusRow in rows
                    .Where(x => !string.IsNullOrWhiteSpace(x.GenusName) && !string.IsNullOrWhiteSpace(x.FamilyName))
                    .GroupBy(x => $"{x.FamilyName}|{x.GenusName}", StringComparer.OrdinalIgnoreCase)
                    .Select(group => new { group.First().FamilyName, group.First().GenusName })
                    .OrderBy(x => x.FamilyName)
                    .ThenBy(x => x.GenusName))
                {
                    if (!familyMap.TryGetValue(genusRow.FamilyName, out var familyId))
                    {
                        continue;
                    }

                    var genus = new Generi
                    {
                        id = Guid.NewGuid(),
                        descrizione = TruncateForDb(genusRow.GenusName, 100),
                        famiglia = familyId
                    };

                    _context.Generi.Add(genus);
                    genusMap[$"{genusRow.FamilyName}|{genusRow.GenusName}"] = genus.id;
                    importedGenera++;
                }

                await _context.SaveChangesAsync(cancellationToken);

                foreach (var row in rows)
                {
                    if (!genusMap.TryGetValue($"{row.FamilyName}|{row.GenusName}", out var genusId))
                    {
                        viewModel.Summary.SkippedRows++;
                        continue;
                    }

                    _context.Specie.Add(new Specie
                    {
                        id = Guid.NewGuid(),
                        genere = genusId,
                        validazione_tassonomica = validazioneWfoId,
                        nome = TruncateForDb(row.Nome, 100),
                        nome_scientifico = TruncateForDb(row.ScientificName, 200),
                        data_inserimento = now,
                        lsid = TruncateForDb(row.Lsid, 255),
                        autori = TruncateForDb(row.Autori, 100),
                        subspecie = TruncateForDb(row.Subspecie, 100),
                        autorisub = TruncateForDb(row.AutoriSub, 100),
                        varieta = TruncateForDb(row.Varieta, 100),
                        autorivar = TruncateForDb(row.AutoriVar, 100),
                        cult = TruncateForDb(row.Cult, 100),
                        autoricult = TruncateForDb(row.AutoriCult, 100),
                        note = string.Empty,
                        nome_comune = string.Empty,
                        nome_comune_en = string.Empty
                    });

                    importedSpecies++;
                }

                (removedOrphanGenera, removedOrphanFamilies) = await CleanupOrphanTaxonomyAsync(_context, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Import interrotto: {ex.GetBaseException().Message}");
                viewModel.Summary = new SpecieWfoNomenclatureImportSummary();
                viewModel.SourceUrl = model.SourceUrl;
                viewModel.DownloadedFileToken = model.DownloadedFileToken;
                viewModel.DownloadedFileName = model.DownloadedFileName;
                viewModel.DownloadedFileUrl = string.IsNullOrWhiteSpace(model.DownloadedFileToken)
                    ? string.Empty
                    : Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token = model.DownloadedFileToken }) ?? string.Empty;
                return View(viewModel);
            }

            viewModel = await BuildWfoNomenclatureImportViewModelAsync(model.ForceImport, cancellationToken);
            viewModel.Summary.ImportedFamilies = importedFamilies;
            viewModel.Summary.ImportedGenera = importedGenera;
            viewModel.Summary.ImportedSpecies = importedSpecies;
            viewModel.SourceUrl = model.SourceUrl;
            viewModel.ForceImport = model.ForceImport;
            viewModel.Summary.Messages.Add($"Import completato dal file '{model.DownloadedFileName}'.");
            viewModel.Summary.Messages.Add("Ho creato prima le famiglie, poi i generi collegati, poi le specie.");
            viewModel.Summary.Messages.Add($"Pulizia finale completata: rimossi {removedOrphanGenera} generi senza specie e {removedOrphanFamilies} famiglie senza generi.");
            if (viewModel.Summary.SkippedRows > 0)
            {
                viewModel.Summary.Messages.Add($"Ho saltato {viewModel.Summary.SkippedRows} righe non utilizzabili o incomplete.");
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetWfoNomenclatureImportStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId) || !_wfoImportJobs.TryGetValue(jobId, out var job))
            {
                return NotFound(new { message = "Job di import non trovato." });
            }

            return Json(job.CreateSnapshot());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelWfoNomenclatureImport(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId) || !_wfoImportJobs.TryGetValue(jobId, out var job))
            {
                return NotFound(new { message = "Job di import non trovato." });
            }

            job.RequestCancellation();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResumeWfoNomenclatureImport(SpecieWfoNomenclatureImportViewModel model, CancellationToken cancellationToken)
        {
            if (!IsAjaxRequest())
            {
                return RedirectToAction(nameof(ImportWfoNomenclature));
            }

            if (string.IsNullOrWhiteSpace(model.DownloadedFileToken))
            {
                return BadRequest(new { message = "Manca il riferimento al file scaricato da riprendere." });
            }

            var cachedFilePath = GetCachedWfoSnapshotPath(model.DownloadedFileToken);
            if (!System.IO.File.Exists(cachedFilePath))
            {
                return BadRequest(new { message = "Il file scaricato non e piu disponibile. Riesegui il download." });
            }

            var checkpoint = await LoadWfoImportCheckpointAsync(model.DownloadedFileToken, cancellationToken);
            if (checkpoint == null)
            {
                return BadRequest(new { message = "Non ho trovato un checkpoint valido da cui riprendere l'import." });
            }

            var organizationId = await GetCurrentUserOrganizationAsync();
            if (!organizationId.HasValue)
            {
                return BadRequest(new { message = "Non riesco a determinare l'organizzazione corrente per riprendere l'import." });
            }

            CleanupCompletedWfoImportJobs();

            var jobId = Guid.NewGuid().ToString("N");
            var job = new WfoNomenclatureImportJobState(jobId, model.DownloadedFileName, model.DownloadedFileToken);
            job.RestoreFromCheckpoint(checkpoint);
            job.AddMessage($"Ripresa import dal file '{model.DownloadedFileName}'.");
            _wfoImportJobs[jobId] = job;

            _ = Task.Run(() => RunWfoNomenclatureImportInBackgroundAsync(
                jobId,
                cachedFilePath,
                model.DownloadedFileName,
                model.DownloadedFileToken,
                organizationId.Value));

            return Json(new
            {
                started = true,
                resumed = true,
                jobId,
                statusUrl = Url.Action(nameof(GetWfoNomenclatureImportStatus), new { jobId })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadWfoNomenclatureSnapshot(SpecieWfoNomenclatureImportViewModel model, CancellationToken cancellationToken)
        {
            var viewModel = await BuildWfoNomenclatureImportViewModelAsync(model.ForceImport, cancellationToken);
            viewModel.SourceUrl = SpecieScientificNameHelper.NormalizeSpacing(model.SourceUrl);
            viewModel.ForceImport = model.ForceImport;

            if (!viewModel.CanImport)
            {
                ModelState.AddModelError(string.Empty, "L'import non puo partire in questo stato. Usa il checkpoint rilevato oppure riforza il processo dalla pagina import.");
                return View(nameof(ImportWfoNomenclature), viewModel);
            }

            if (string.IsNullOrWhiteSpace(viewModel.SourceUrl))
            {
                viewModel.SourceUrl = DefaultWfoDatasetDownloadUrl;
            }

            if (string.IsNullOrWhiteSpace(viewModel.SourceUrl))
            {
                ModelState.AddModelError(nameof(model.SourceUrl), "Non riesco a determinare la sorgente ufficiale WFO da scaricare.");
                return View(nameof(ImportWfoNomenclature), viewModel);
            }

            if (!Uri.TryCreate(viewModel.SourceUrl, UriKind.Absolute, out var sourceUri))
            {
                ModelState.AddModelError(nameof(model.SourceUrl), "L'URL indicato non e valido.");
                return View(nameof(ImportWfoNomenclature), viewModel);
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UPlant/1.0 (+https://dev3.adm.unipi.it/UPlant)");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                httpClient.DefaultRequestHeaders.Referrer = new Uri(DefaultWfoDatasetPageUrl);

                using var response = await httpClient.GetAsync(sourceUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var fileName = GetDownloadFileName(response, sourceUri);
                var existingSnapshot = await TryLoadLatestCachedWfoSnapshotAsync(cancellationToken);
                if (existingSnapshot != null &&
                    existingSnapshot.SourceUrl.Equals(viewModel.SourceUrl, StringComparison.OrdinalIgnoreCase) &&
                    existingSnapshot.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                    IsCurrentOfficialWfoDataset(existingSnapshot.FileName))
                {
                    viewModel.DownloadedFileToken = existingSnapshot.FileToken;
                    viewModel.DownloadedFileName = existingSnapshot.FileName;
                    viewModel.DownloadedFileUrl = existingSnapshot.FileUrl;
                    viewModel.DownloadedFileMessage = "Ho trovato in cache l'ultimo dataset ufficiale gia scaricato: riuso quello senza riscaricarlo.";
                    return View(nameof(ImportWfoNomenclature), viewModel);
                }

                var token = BuildCachedWfoSnapshotToken(fileName);
                var storagePath = GetCachedWfoSnapshotPath(token);
                Directory.CreateDirectory(Path.GetDirectoryName(storagePath)!);

                await using (var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (var targetStream = System.IO.File.Create(storagePath))
                {
                    await sourceStream.CopyToAsync(targetStream, cancellationToken);
                }

                await SaveWfoSnapshotMetadataAsync(new WfoSnapshotCacheInfo
                {
                    FileToken = token,
                    FileName = fileName,
                    SourceUrl = viewModel.SourceUrl,
                    DownloadedAtUtc = DateTime.UtcNow
                }, cancellationToken);

                viewModel.DownloadedFileToken = token;
                viewModel.DownloadedFileName = fileName;
                viewModel.DownloadedFileUrl = Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token }) ?? string.Empty;
                viewModel.DownloadedFileMessage = $"File scaricato con successo e pronto per l'import. Versione: {DefaultWfoDatasetLabel} (ultima versione).";
                viewModel.Summary.Messages.Add($"File scaricato con successo da {viewModel.SourceUrl}.");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Non sono riuscito a scaricare il dataset WFO ({(int?)ex.StatusCode} {ex.StatusCode}). Posso riprovare o usare una sorgente alternativa.");
            }

            return View(nameof(ImportWfoNomenclature), viewModel);
        }

        [HttpGet]
        public IActionResult PreviewWfoNomenclatureSnapshot(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(ImportWfoNomenclature));
            }

            var filePath = GetCachedWfoSnapshotPath(token);
            if (!System.IO.File.Exists(filePath))
            {
                return RedirectToAction(nameof(ImportWfoNomenclature));
            }

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, "application/octet-stream", fileName);
        }

        [HttpPost("/Specie/ApplyWfoDecision")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyWfoDecision(ApplyWfoDecisionInput input)
        {
            var specie = await _context.Specie
                .Include(s => s.genereNavigation)
                .FirstOrDefaultAsync(s => s.id == input.SpecieId);

            if (specie == null)
            {
                return NotFound();
            }

            specie.validazione_tassonomica = await EnsureDefaultValidazioneTassonomicaAsync(ResolveValidationStatusName(input));
            specie.data_inserimento = DateTime.Now;

            if (string.IsNullOrWhiteSpace(input.GenusName))
            {
                var acceptedFullName = SpecieScientificNameHelper.NormalizeSpacing(input.AcceptedFullName);
                var currentScientificName = SpecieScientificNameHelper.NormalizeSpacing(specie.nome_scientifico);

                if (!string.IsNullOrWhiteSpace(acceptedFullName) &&
                    string.Equals(acceptedFullName, currentScientificName, StringComparison.OrdinalIgnoreCase))
                {
                    specie.lsid = SpecieScientificNameHelper.NormalizeSpacing(input.Lsid);
                    specie.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);

                    if (input.ApplySuggestedIucnGlobal)
                    {
                        specie.iucn_globale = await ResolveIucnIdByCodeAsync(input.SuggestedIucnGlobalCode);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                if (string.Equals(input.ActionType, "keep_current", StringComparison.OrdinalIgnoreCase))
                {
                    specie.lsid = SpecieScientificNameHelper.NormalizeSpacing(input.Lsid);
                    specie.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                if (!await ApplyAcceptedNameAsync(specie, input.AcceptedFullName, input.Lsid))
                {
                    TempData["PendingWfoForm"] = JsonSerializer.Serialize(input);
                    TempData["WfoError"] = "Il nome proposto da WFO usa un genere non presente in archivio. Inserisci prima il genere oppure correggi manualmente la specie.";
                    return RedirectToAction(nameof(ReviewWfo), new { id = specie.id });
                }

                await _context.SaveChangesAsync();
                input.ValidationStatusName = "A.A.";
                return RedirectToAction(nameof(Index));
            }

            if (!await ApplyReviewFormAsync(specie, input))
            {
                TempData["PendingWfoForm"] = JsonSerializer.Serialize(input);
                TempData["WfoError"] = "Il genere indicato non esiste in archivio. Correggi il campo Genere oppure inserisci prima il genere mancante.";
                return RedirectToAction(nameof(ReviewWfo), new { id = specie.id });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Specie/ApplyWfoDecision")]
        public IActionResult ApplyWfoDecision()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Specie/CreateMissingGenusFromWfo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMissingGenusFromWfo(CreateMissingGenusInput input, CancellationToken cancellationToken)
        {
            if (input.SpecieId == Guid.Empty)
            {
                return RedirectToAction(nameof(Index));
            }

            var genusName = SpecieScientificNameHelper.NormalizeSpacing(input.GenusName);
            var familyName = SpecieScientificNameHelper.NormalizeSpacing(input.FamilyName);

            if (string.IsNullOrWhiteSpace(genusName) || string.IsNullOrWhiteSpace(familyName))
            {
                TempData["WfoError"] = "Non ho abbastanza dati WFO per creare automaticamente il genere.";
                TempData["PendingWfoForm"] = input.PendingFormJson ?? string.Empty;
                return RedirectToAction(nameof(ReviewWfo), new { id = input.SpecieId });
            }

            var existingGenusId = await _context.Generi
                .Where(g => g.descrizione == genusName)
                .Select(g => (Guid?)g.id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingGenusId.HasValue)
            {
                TempData["WfoError"] = $"Il genere {genusName} esiste gia in archivio.";
                TempData["PendingWfoForm"] = input.PendingFormJson ?? string.Empty;
                return RedirectToAction(nameof(ReviewWfo), new { id = input.SpecieId });
            }

            var familyId = await _context.Famiglie
                .Where(f => f.descrizione == familyName)
                .Select(f => (Guid?)f.id)
                .FirstOrDefaultAsync(cancellationToken);

            var familyCreated = false;
            if (!familyId.HasValue)
            {
                var newFamily = new Famiglie
                {
                    id = Guid.NewGuid(),
                    descrizione = familyName,
                    descrizione_en = familyName
                };
                _context.Famiglie.Add(newFamily);
                familyId = newFamily.id;
                familyCreated = true;
            }

            _context.Generi.Add(new Generi
            {
                id = Guid.NewGuid(),
                descrizione = genusName,
                famiglia = familyId.Value
            });

            await _context.SaveChangesAsync(cancellationToken);
            TempData["PendingWfoForm"] = input.PendingFormJson ?? string.Empty;
            TempData["WfoSuccess"] = familyCreated
                ? $"Ho inserito la famiglia {familyName} e il genere {genusName}."
                : $"Ho inserito il genere {genusName} collegato alla famiglia {familyName}.";
            return RedirectToAction(nameof(ReviewWfo), new { id = input.SpecieId });
        }

        private async Task<SpecieWfoNomenclatureImportViewModel> BuildWfoNomenclatureImportViewModelAsync(bool forceImport, CancellationToken cancellationToken)
        {
            var existingFamiliesCount = await _context.Famiglie.CountAsync(cancellationToken);
            var existingGeneraCount = await _context.Generi.CountAsync(cancellationToken);
            var existingSpeciesCount = await _context.Specie.CountAsync(cancellationToken);
            var existingAccessioniCount = await _context.Accessioni.CountAsync(cancellationToken);
            var interruptedImport = await TryLoadInterruptedWfoImportAsync(cancellationToken);
            var latestDownloadedSnapshot = interruptedImport == null
                ? await TryLoadLatestCachedWfoSnapshotAsync(cancellationToken)
                : null;
            var canStartFreshImport = existingFamiliesCount == 0 && existingGeneraCount == 0 && existingSpeciesCount == 0;
            var canImport = canStartFreshImport || interruptedImport != null || forceImport;

            return new SpecieWfoNomenclatureImportViewModel
            {
                SourceUrl = DefaultWfoDatasetDownloadUrl,
                ForceImport = forceImport,
                DownloadedFileToken = interruptedImport?.FileToken ?? latestDownloadedSnapshot?.FileToken ?? string.Empty,
                DownloadedFileName = interruptedImport?.FileName ?? latestDownloadedSnapshot?.FileName ?? string.Empty,
                DownloadedFileUrl = interruptedImport?.FileUrl ?? latestDownloadedSnapshot?.FileUrl ?? string.Empty,
                DownloadedFileMessage = interruptedImport != null
                    ? "Ho rilevato un file con checkpoint valido: puoi riprendere l'import."
                    : latestDownloadedSnapshot?.Message ?? string.Empty,
                OfficialDatasetLabel = DefaultWfoDatasetLabel,
                IsLatestDatasetAvailable = true,
                CachedSnapshotIsCurrent = latestDownloadedSnapshot != null && IsCurrentOfficialWfoDataset(latestDownloadedSnapshot.FileName),
                LocalDatasetLabel = latestDownloadedSnapshot == null ? "nessun dataset locale" : GetDatasetLabelFromFileName(latestDownloadedSnapshot.FileName),
                ExistingFamiliesCount = existingFamiliesCount,
                ExistingGeneraCount = existingGeneraCount,
                ExistingSpeciesCount = existingSpeciesCount,
                ExistingAccessioniCount = existingAccessioniCount,
                HasAccessionRelations = existingAccessioniCount > 0,
                CanImport = canImport,
                CanStartFreshImport = canStartFreshImport,
                HasInterruptedImport = interruptedImport != null,
                InterruptedImportMessage = interruptedImport?.Message ?? string.Empty,
                InterruptedImportUpdatedAtUtc = interruptedImport?.UpdatedAtUtc
            };
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
                   Request.Headers.Accept.Any(x => x?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
        }

        private async Task RunWfoNomenclatureImportInBackgroundAsync(
            string jobId,
            string cachedFilePath,
            string fileName,
            string fileToken,
            Guid organizationId)
        {
            if (!_wfoImportJobs.TryGetValue(jobId, out var job))
            {
                return;
            }

            try
            {
                job.MarkRunning("Lettura snapshot WFO");
                List<WfoNomenclatureImportRow> rows;

                await using (var stream = System.IO.File.OpenRead(cachedFilePath))
                {
                    rows = await ParseWfoNomenclatureSnapshotAsync(stream, fileName, CancellationToken.None);
                }

                if (rows.Count == 0)
                {
                    job.Fail("Non ho trovato righe valide da importare nel file scaricato.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Entities>();
                var checkpoint = await LoadWfoImportCheckpointAsync(fileToken, CancellationToken.None)
                    ?? new WfoNomenclatureImportCheckpoint();

                var importedFamilies = 0;
                var importedGenera = 0;
                var importedSpecies = 0;
                var skippedRows = 0;
                var removedOrphanGenera = 0;
                var removedOrphanFamilies = 0;
                var resumeSpeciesIndex = Math.Max(0, checkpoint.NextSpeciesIndex);

                var familiesToImport = rows
                    .Select(x => x.FamilyName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();

                var generaToImport = rows
                    .Where(x => !string.IsNullOrWhiteSpace(x.GenusName) && !string.IsNullOrWhiteSpace(x.FamilyName))
                    .GroupBy(x => $"{x.FamilyName}|{x.GenusName}", StringComparer.OrdinalIgnoreCase)
                    .Select(group => new { group.First().FamilyName, group.First().GenusName })
                    .OrderBy(x => x.FamilyName)
                    .ThenBy(x => x.GenusName)
                    .ToList();

                job.SetPlannedWork(familiesToImport.Count, generaToImport.Count, rows.Count);
                job.AddMessage($"Righe valide trovate: {rows.Count}.");
                job.SetImportedCounts(checkpoint.ImportedFamilies, checkpoint.ImportedGenera, checkpoint.ImportedSpecies, checkpoint.SkippedRows, resumeSpeciesIndex);

                var validazioneWfoId = await EnsureDefaultValidazioneTassonomicaAsync(context, organizationId, "WFO", CancellationToken.None);
                var now = DateTime.Now;

                var familyMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                job.MarkRunning("Import famiglie");
                foreach (var familyName in familiesToImport)
                {
                    if (job.IsCancellationRequested)
                    {
                        checkpoint.NextSpeciesIndex = resumeSpeciesIndex;
                        checkpoint.ImportedFamilies = importedFamilies;
                        checkpoint.ImportedGenera = importedGenera;
                        checkpoint.ImportedSpecies = importedSpecies;
                        checkpoint.SkippedRows = skippedRows;
                        await SaveWfoImportCheckpointAsync(fileToken, checkpoint, CancellationToken.None);
                        job.MarkPaused("Interruzione richiesta durante l'import famiglie.");
                        return;
                    }

                    var existingFamilyId = await context.Famiglie
                        .Where(x => x.descrizione == TruncateForDb(familyName, 100))
                        .Select(x => (Guid?)x.id)
                        .FirstOrDefaultAsync(CancellationToken.None);

                    if (existingFamilyId.HasValue)
                    {
                        familyMap[familyName] = existingFamilyId.Value;
                        continue;
                    }

                    var family = new Famiglie
                    {
                        id = Guid.NewGuid(),
                        descrizione = TruncateForDb(familyName, 100)
                    };

                    context.Famiglie.Add(family);
                    familyMap[familyName] = family.id;
                    importedFamilies++;
                    job.ReportFamilyImported(familyName, importedFamilies);
                }

                await context.SaveChangesAsync(CancellationToken.None);

                var genusMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                job.MarkRunning("Import generi");
                foreach (var genusRow in generaToImport)
                {
                    if (job.IsCancellationRequested)
                    {
                        checkpoint.NextSpeciesIndex = resumeSpeciesIndex;
                        checkpoint.ImportedFamilies = importedFamilies;
                        checkpoint.ImportedGenera = importedGenera;
                        checkpoint.ImportedSpecies = importedSpecies;
                        checkpoint.SkippedRows = skippedRows;
                        await SaveWfoImportCheckpointAsync(fileToken, checkpoint, CancellationToken.None);
                        job.MarkPaused("Interruzione richiesta durante l'import generi.");
                        return;
                    }

                    if (!familyMap.TryGetValue(genusRow.FamilyName, out var familyId))
                    {
                        continue;
                    }

                    var normalizedGenusName = TruncateForDb(genusRow.GenusName, 100);
                    var existingGenusId = await context.Generi
                        .Where(x => x.descrizione == normalizedGenusName && x.famiglia == familyId)
                        .Select(x => (Guid?)x.id)
                        .FirstOrDefaultAsync(CancellationToken.None);

                    if (existingGenusId.HasValue)
                    {
                        genusMap[$"{genusRow.FamilyName}|{genusRow.GenusName}"] = existingGenusId.Value;
                        continue;
                    }

                    var genus = new Generi
                    {
                        id = Guid.NewGuid(),
                        descrizione = normalizedGenusName,
                        famiglia = familyId
                    };

                    context.Generi.Add(genus);
                    genusMap[$"{genusRow.FamilyName}|{genusRow.GenusName}"] = genus.id;
                    importedGenera++;
                    job.ReportGenusImported(genusRow.GenusName, genusRow.FamilyName, importedGenera);
                }

                await context.SaveChangesAsync(CancellationToken.None);

                job.MarkRunning("Import specie");
                for (var rowIndex = resumeSpeciesIndex; rowIndex < rows.Count; rowIndex++)
                {
                    if (job.IsCancellationRequested)
                    {
                        checkpoint.NextSpeciesIndex = rowIndex;
                        checkpoint.ImportedFamilies = importedFamilies;
                        checkpoint.ImportedGenera = importedGenera;
                        checkpoint.ImportedSpecies = importedSpecies;
                        checkpoint.SkippedRows = skippedRows;
                        checkpoint.LastScientificName = rowIndex > 0 && rowIndex - 1 < rows.Count ? rows[rowIndex - 1].ScientificName : string.Empty;
                        checkpoint.UpdatedAtUtc = DateTime.UtcNow;
                        await SaveWfoImportCheckpointAsync(fileToken, checkpoint, CancellationToken.None);
                        job.MarkPaused("Interruzione richiesta. Puoi riprendere dal punto raggiunto.");
                        return;
                    }

                    var row = rows[rowIndex];
                    if (!genusMap.TryGetValue($"{row.FamilyName}|{row.GenusName}", out var genusId))
                    {
                        skippedRows++;
                        job.ReportSpeciesProcessed(row.ScientificName, importedSpecies, skippedRows, imported: false);
                        job.ReportFailedItem(row.ScientificName, "Genere o famiglia non risolti nel dataset da importare.");
                        checkpoint.NextSpeciesIndex = rowIndex + 1;
                        checkpoint.ImportedFamilies = importedFamilies;
                        checkpoint.ImportedGenera = importedGenera;
                        checkpoint.ImportedSpecies = importedSpecies;
                        checkpoint.SkippedRows = skippedRows;
                        checkpoint.LastScientificName = row.ScientificName;
                        checkpoint.UpdatedAtUtc = DateTime.UtcNow;
                        continue;
                    }

                    var normalizedScientificName = TruncateForDb(row.ScientificName, 200);
                    var existingSpecies = await context.Specie
                        .AsNoTracking()
                        .AnyAsync(x => x.nome_scientifico == normalizedScientificName, CancellationToken.None);

                    if (existingSpecies)
                    {
                        checkpoint.NextSpeciesIndex = rowIndex + 1;
                        checkpoint.ImportedFamilies = importedFamilies;
                        checkpoint.ImportedGenera = importedGenera;
                        checkpoint.ImportedSpecies = importedSpecies;
                        checkpoint.SkippedRows = skippedRows;
                        checkpoint.LastScientificName = row.ScientificName;
                        checkpoint.UpdatedAtUtc = DateTime.UtcNow;
                        continue;
                    }

                    var entity = new Specie
                    {
                        id = Guid.NewGuid(),
                        genere = genusId,
                        validazione_tassonomica = validazioneWfoId,
                        nome = TruncateForDb(row.Nome, 100),
                        nome_scientifico = normalizedScientificName,
                        data_inserimento = now,
                        lsid = TruncateForDb(row.Lsid, 255),
                        autori = TruncateForDb(row.Autori, 100),
                        subspecie = TruncateForDb(row.Subspecie, 100),
                        autorisub = TruncateForDb(row.AutoriSub, 100),
                        varieta = TruncateForDb(row.Varieta, 100),
                        autorivar = TruncateForDb(row.AutoriVar, 100),
                        cult = TruncateForDb(row.Cult, 100),
                        autoricult = TruncateForDb(row.AutoriCult, 100),
                        note = string.Empty,
                        nome_comune = string.Empty,
                        nome_comune_en = string.Empty
                    };

                    context.Specie.Add(entity);

                    try
                    {
                        await context.SaveChangesAsync(CancellationToken.None);
                        importedSpecies++;
                        job.ReportSpeciesProcessed(row.ScientificName, importedSpecies, skippedRows, imported: true);
                    }
                    catch (Exception ex)
                    {
                        context.Entry(entity).State = EntityState.Detached;
                        skippedRows++;
                        job.ReportSpeciesProcessed(row.ScientificName, importedSpecies, skippedRows, imported: false);
                        job.ReportFailedItem(row.ScientificName, ex.GetBaseException().Message);
                    }

                    checkpoint.NextSpeciesIndex = rowIndex + 1;
                    checkpoint.ImportedFamilies = importedFamilies;
                    checkpoint.ImportedGenera = importedGenera;
                    checkpoint.ImportedSpecies = importedSpecies;
                    checkpoint.SkippedRows = skippedRows;
                    checkpoint.LastScientificName = row.ScientificName;
                    checkpoint.UpdatedAtUtc = DateTime.UtcNow;

                    if ((rowIndex + 1) % 25 == 0)
                    {
                        await SaveWfoImportCheckpointAsync(fileToken, checkpoint, CancellationToken.None);
                    }
                }

                (removedOrphanGenera, removedOrphanFamilies) = await CleanupOrphanTaxonomyAsync(context, CancellationToken.None);
                job.AddMessage($"Pulizia finale: rimossi {removedOrphanGenera} generi senza specie e {removedOrphanFamilies} famiglie senza generi.");
                await DeleteWfoImportCheckpointAsync(fileToken);
                job.Complete(importedFamilies, importedGenera, importedSpecies, skippedRows, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'import nomenclatura WFO background job {JobId}", jobId);
                job.Fail($"Import interrotto: {ex.GetBaseException().Message}");
            }
        }

        private static void CleanupCompletedWfoImportJobs()
        {
            var threshold = DateTime.UtcNow.AddHours(-12);
            foreach (var entry in _wfoImportJobs)
            {
                var snapshot = entry.Value.CreateSnapshot();
                if ((snapshot.Status == "completed" || snapshot.Status == "failed") && snapshot.UpdatedAtUtc < threshold)
                {
                    _wfoImportJobs.TryRemove(entry.Key, out _);
                }
            }
        }

        private async Task RunWfoDatabaseAuditInBackgroundAsync(string jobId)
        {
            if (!_wfoAuditJobs.TryGetValue(jobId, out var job))
            {
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Entities>();
                var wfoService = scope.ServiceProvider.GetRequiredService<IWorldFloraOnlineService>();

                const int batchSize = 20;
                var interRequestDelay = TimeSpan.FromMilliseconds(500);
                var processed = 0;
                var consecutiveServiceFailures = 0;

                job.MarkRunning("Lettura specie database");

                while (processed < job.TotalSpecies)
                {
                    var remaining = job.TotalSpecies - processed;
                    var speciesBatch = await context.Specie
                        .AsNoTracking()
                        .Where(x => x.validazione_tassonomicaNavigation != null && x.validazione_tassonomicaNavigation.descrizione == "N.D.")
                        .OrderBy(x => x.nome_scientifico)
                        .ThenBy(x => x.id)
                        .Skip(processed)
                        .Take(Math.Min(batchSize, remaining))
                        .Select(x => new WfoDatabaseAuditSourceRow
                        {
                            Id = x.id,
                            ScientificName = x.nome_scientifico ?? string.Empty,
                            Nome = x.nome ?? string.Empty,
                            Autori = x.autori ?? string.Empty,
                            Subspecie = x.subspecie ?? string.Empty,
                            AutoriSub = x.autorisub ?? string.Empty,
                            Varieta = x.varieta ?? string.Empty,
                            AutoriVar = x.autorivar ?? string.Empty,
                            Cult = x.cult ?? string.Empty,
                            AutoriCult = x.autoricult ?? string.Empty,
                            GenusName = x.genereNavigation != null ? x.genereNavigation.descrizione ?? string.Empty : string.Empty,
                            CurrentValidationStatus = x.validazione_tassonomicaNavigation != null ? x.validazione_tassonomicaNavigation.descrizione ?? string.Empty : string.Empty
                        })
                        .ToListAsync(CancellationToken.None);

                    if (speciesBatch.Count == 0)
                    {
                        break;
                    }

                    job.MarkRunning($"Analisi nominativi ({Math.Min(processed + speciesBatch.Count, job.TotalSpecies)}/{job.TotalSpecies})");

                    foreach (var row in speciesBatch)
                    {
                        try
                        {
                            var fakeSpecie = new Specie
                            {
                                id = row.Id,
                                nome = row.Nome,
                                nome_scientifico = row.ScientificName,
                                autori = row.Autori,
                                subspecie = row.Subspecie,
                                autorisub = row.AutoriSub,
                                varieta = row.Varieta,
                                autorivar = row.AutoriVar,
                                cult = row.Cult,
                                autoricult = row.AutoriCult
                            };

                            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                            var checkResult = await wfoService.CheckAsync(fakeSpecie, row.GenusName, timeoutCts.Token);
                            var item = BuildWfoDatabaseAuditItem(row, checkResult);
                            consecutiveServiceFailures = 0;
                            job.ReportItem(item, ShouldKeepWfoDatabaseAuditItem(item, job.Options));
                        }
                        catch (Exception ex)
                        {
                            consecutiveServiceFailures++;
                            if (await ShouldAbortWfoAuditForServiceFailureAsync(wfoService, ex, consecutiveServiceFailures))
                            {
                                job.Fail($"Audit interrotto: {BuildWfoServiceUnavailableMessage(ex)}");
                                await SaveWfoDatabaseAuditCacheAsync(job.CreateSnapshot(), CancellationToken.None);
                                return;
                            }

                            var item = BuildWfoDatabaseAuditErrorItem(row, FormatWfoAuditExceptionMessage(ex));
                            job.ReportItem(item, ShouldKeepWfoDatabaseAuditItem(item, job.Options));
                        }

                        if (job.CheckedSpecies < job.TotalSpecies)
                        {
                            await Task.Delay(interRequestDelay, CancellationToken.None);
                        }
                    }

                    processed += speciesBatch.Count;
                    await SaveWfoDatabaseAuditCacheAsync(job.CreateSnapshot(), CancellationToken.None);
                }

                job.Complete();
                await SaveWfoDatabaseAuditCacheAsync(job.CreateSnapshot(), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'audit globale WFO {JobId}", jobId);
                job.Fail($"Audit interrotto: {ex.GetBaseException().Message}");
                await SaveWfoDatabaseAuditCacheAsync(job.CreateSnapshot(), CancellationToken.None);
            }
        }

        private static WfoDatabaseAuditItem BuildWfoDatabaseAuditItem(WfoDatabaseAuditSourceRow row, WfoCheckResult checkResult)
        {
            var currentScientificName = SpecieScientificNameHelper.NormalizeSpacing(row.ScientificName);
            var allCandidates = new List<WfoCandidate>();
            if (checkResult.Match != null)
            {
                allCandidates.Add(checkResult.Match);
            }

            allCandidates.AddRange(checkResult.Candidates);

            var exactAccepted = allCandidates.FirstOrDefault(candidate =>
                candidate.IsAccepted &&
                string.Equals(
                    SpecieScientificNameHelper.NormalizeSpacing(candidate.FullName),
                    currentScientificName,
                    StringComparison.OrdinalIgnoreCase));

            if (exactAccepted != null)
            {
                return new WfoDatabaseAuditItem
                {
                    SpecieId = row.Id,
                    ScientificName = row.ScientificName,
                    CurrentValidationStatus = row.CurrentValidationStatus,
                    Section = "perfectAccepted",
                    AcceptedName = exactAccepted.FullName,
                    Lsid = exactAccepted.Lsid,
                    Notes = "Match perfetto con nome accettato WFO."
                };
            }

            var exactSynonym = allCandidates.FirstOrDefault(candidate =>
                !candidate.IsAccepted &&
                string.Equals(
                    SpecieScientificNameHelper.NormalizeSpacing(candidate.FullName),
                    currentScientificName,
                    StringComparison.OrdinalIgnoreCase));

            if (exactSynonym != null)
            {
                var acceptedFromSynonym = allCandidates.FirstOrDefault(candidate =>
                    candidate.IsAccepted &&
                    string.Equals(
                        SpecieScientificNameHelper.NormalizeSpacing(candidate.FullName),
                        SpecieScientificNameHelper.NormalizeSpacing(exactSynonym.SuggestedAcceptedName),
                        StringComparison.OrdinalIgnoreCase));

                var synonymSupportCount = allCandidates.Count(candidate =>
                    !string.IsNullOrWhiteSpace(candidate.SuggestedAcceptedName) &&
                    string.Equals(
                        SpecieScientificNameHelper.NormalizeSpacing(candidate.SuggestedAcceptedName),
                        SpecieScientificNameHelper.NormalizeSpacing(exactSynonym.SuggestedAcceptedName),
                        StringComparison.OrdinalIgnoreCase));

                return new WfoDatabaseAuditItem
                {
                    SpecieId = row.Id,
                    ScientificName = row.ScientificName,
                    CurrentValidationStatus = row.CurrentValidationStatus,
                    Section = "perfectSynonym",
                    AcceptedName = acceptedFromSynonym?.FullName ?? exactSynonym.SuggestedAcceptedName,
                    Lsid = acceptedFromSynonym?.Lsid ?? exactSynonym.Lsid,
                    FamilyName = acceptedFromSynonym?.FamilyName ?? exactSynonym.FamilyName,
                    Notes = synonymSupportCount > 1
                        ? $"Il nome attuale esiste in WFO ma risulta sinonimo. Conferme trovate: {synonymSupportCount}."
                        : "Il nome attuale esiste in WFO ma risulta sinonimo."
                };
            }

            var topCandidate = allCandidates.FirstOrDefault();
            return new WfoDatabaseAuditItem
            {
                SpecieId = row.Id,
                ScientificName = row.ScientificName,
                CurrentValidationStatus = row.CurrentValidationStatus,
                Section = checkResult.Status == WfoMatchStatus.Ambiguous ? "ambiguous" : "noMatch",
                AcceptedName = topCandidate?.SuggestedAcceptedName ?? topCandidate?.FullName ?? string.Empty,
                Lsid = topCandidate?.Lsid ?? string.Empty,
                Notes = checkResult.Status switch
                {
                    WfoMatchStatus.NotFound => "Nessun match affidabile trovato in WFO.",
                    WfoMatchStatus.Ambiguous => "WFO ha trovato piu candidati ma nessun match perfetto.",
                    _ => "Il nome non rientra nei casi di match perfetto."
                }
            };
        }

        private static WfoDatabaseAuditItem BuildWfoDatabaseAuditErrorItem(WfoDatabaseAuditSourceRow row, string errorMessage)
        {
            return new WfoDatabaseAuditItem
            {
                SpecieId = row.Id,
                ScientificName = row.ScientificName,
                CurrentValidationStatus = row.CurrentValidationStatus,
                Section = "error",
                AcceptedName = string.Empty,
                Lsid = string.Empty,
                Notes = $"Verifica WFO non completata: {errorMessage}"
            };
        }

        private static string FormatWfoAuditExceptionMessage(Exception exception)
        {
            if (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                return "timeout WFO dopo 20 secondi, specie saltata";
            }

            if (exception is HttpRequestException httpRequestException && httpRequestException.StatusCode.HasValue)
            {
                return $"WFO ha risposto {(int)httpRequestException.StatusCode.Value} {httpRequestException.StatusCode.Value}, specie saltata";
            }

            var baseMessage = exception.GetBaseException().Message;
            if (string.IsNullOrWhiteSpace(baseMessage))
            {
                return "errore non specificato, specie saltata";
            }

            return baseMessage;
        }

        private static async Task<bool> ShouldAbortWfoAuditForServiceFailureAsync(
            IWorldFloraOnlineService wfoService,
            Exception exception,
            int consecutiveServiceFailures)
        {
            if (exception is HttpRequestException httpRequestException && httpRequestException.StatusCode.HasValue)
            {
                var statusCode = (int)httpRequestException.StatusCode.Value;
                if (statusCode == 429 || statusCode >= 500)
                {
                    return true;
                }
            }

            if (consecutiveServiceFailures < 3)
            {
                return false;
            }

            try
            {
                return !await wfoService.IsAvailableAsync(CancellationToken.None);
            }
            catch
            {
                return true;
            }
        }

        private static string BuildWfoServiceUnavailableMessage(Exception exception)
        {
            if (exception is HttpRequestException httpRequestException && httpRequestException.StatusCode.HasValue)
            {
                return $"servizio WFO offline o non disponibile ({(int)httpRequestException.StatusCode.Value} {httpRequestException.StatusCode.Value})";
            }

            if (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                return "servizio WFO non raggiungibile o troppo lento in modo sistematico";
            }

            return "servizio WFO offline o non disponibile";
        }

        private static void NormalizeWfoAuditInput(StartWfoDatabaseAuditInput input)
        {
            input.MaxSpeciesToProcess = Math.Min(500, Math.Max(1, input.MaxSpeciesToProcess));

            if (!input.IncludePerfectAccepted &&
                !input.IncludePerfectSynonym &&
                !input.IncludeAmbiguous &&
                !input.IncludeNoMatch)
            {
                input.IncludePerfectAccepted = true;
                input.IncludePerfectSynonym = true;
                input.IncludeAmbiguous = true;
                input.IncludeNoMatch = true;
            }
        }

        private static bool ShouldKeepWfoDatabaseAuditItem(WfoDatabaseAuditItem item, StartWfoDatabaseAuditInput options)
        {
            return item.Section switch
            {
                "perfectAccepted" => options.IncludePerfectAccepted,
                "perfectSynonym" => options.IncludePerfectSynonym,
                "ambiguous" => options.IncludeAmbiguous,
                "error" => options.IncludeNoMatch,
                _ => options.IncludeNoMatch
            };
        }

        private static bool SnapshotMatchesAuditInput(WfoDatabaseAuditJobSnapshot snapshot, StartWfoDatabaseAuditInput input)
        {
            return snapshot.MaxSpeciesToProcess == input.MaxSpeciesToProcess &&
                snapshot.IncludePerfectAccepted == input.IncludePerfectAccepted &&
                snapshot.IncludePerfectSynonym == input.IncludePerfectSynonym &&
                snapshot.IncludeAmbiguous == input.IncludeAmbiguous &&
                snapshot.IncludeNoMatch == input.IncludeNoMatch;
        }

        private static void CleanupCompletedWfoAuditJobs()
        {
            var threshold = DateTime.UtcNow.AddHours(-12);
            foreach (var entry in _wfoAuditJobs)
            {
                var snapshot = entry.Value.CreateSnapshot();
                if ((snapshot.Status == "completed" || snapshot.Status == "failed") && snapshot.UpdatedAtUtc < threshold)
                {
                    _wfoAuditJobs.TryRemove(entry.Key, out _);
                }
            }
        }

        private static async Task<List<WfoNomenclatureImportRow>> ParseWfoNomenclatureSnapshotAsync(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return await ParseWfoNomenclatureZipAsync(stream, cancellationToken);
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var content = await reader.ReadToEndAsync(cancellationToken);
            var lines = content
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (lines.Count < 2)
            {
                return new List<WfoNomenclatureImportRow>();
            }

            var delimiter = DetectDelimiter(lines[0], fileName);
            var headers = ParseDelimitedLine(lines[0], delimiter);
            var headerMap = headers
                .Select((header, index) => new { Header = NormalizeHeaderName(header), Index = index })
                .GroupBy(x => x.Header)
                .ToDictionary(x => x.Key, x => x.First().Index, StringComparer.OrdinalIgnoreCase);

            var rows = new List<WfoNomenclatureImportRow>();
            var scientificNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 1; i < lines.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var values = ParseDelimitedLine(lines[i], delimiter);
                if (values.Count == 0)
                {
                    continue;
                }

                string GetValue(params string[] aliases)
                {
                    foreach (var alias in aliases)
                    {
                        if (headerMap.TryGetValue(alias, out var index) && index < values.Count)
                        {
                            return SpecieScientificNameHelper.NormalizeSpacing(values[index]);
                        }
                    }

                    return string.Empty;
                }

                var familyName = GetValue("family", "famiglia");
                var genusName = GetValue("genus", "genere");
                var scientificName = GetValue("scientificname", "fullname", "full_name", "fullscientificname");
                var specificEpithet = GetValue("specificepithet", "species", "nome");
                var infraspecificEpithet = GetValue("infraspecificepithet", "subspecie", "subspecies", "variety", "cultivar");
                var authorship = GetValue("scientificnameauthorship", "authorship", "authors", "author", "autori");
                var rank = GetValue("taxonrank", "rank");
                var taxonId = GetValue("taxonid", "taxon_id", "lsid", "identifier");

                var parsed = SpecieScientificNameHelper.ParseWfoName(scientificName);
                genusName = string.IsNullOrWhiteSpace(parsed.Genus) ? genusName : parsed.Genus;
                var nome = string.IsNullOrWhiteSpace(parsed.Nome) ? specificEpithet : parsed.Nome;

                var subspecie = parsed.Subspecie;
                var autoriSub = parsed.AutoriSub;
                var varieta = parsed.Varieta;
                var autoriVar = parsed.AutoriVar;
                var cult = parsed.Cult;
                var autoriCult = parsed.AutoriCult;
                var autori = parsed.Autori;

                if (string.IsNullOrWhiteSpace(subspecie) && IsSubspeciesRank(rank))
                {
                    subspecie = infraspecificEpithet;
                }

                if (string.IsNullOrWhiteSpace(varieta) && IsVarietyRank(rank))
                {
                    varieta = infraspecificEpithet;
                }

                if (string.IsNullOrWhiteSpace(cult) && IsCultivarRank(rank))
                {
                    cult = infraspecificEpithet;
                }

                if (string.IsNullOrWhiteSpace(autori) && !IsSubspeciesRank(rank) && !IsVarietyRank(rank) && !IsCultivarRank(rank))
                {
                    autori = authorship;
                }

                if (string.IsNullOrWhiteSpace(autoriSub) && !string.IsNullOrWhiteSpace(subspecie) && IsSubspeciesRank(rank))
                {
                    autoriSub = authorship;
                }

                if (string.IsNullOrWhiteSpace(autoriVar) && !string.IsNullOrWhiteSpace(varieta) && IsVarietyRank(rank))
                {
                    autoriVar = authorship;
                }

                if (string.IsNullOrWhiteSpace(autoriCult) && !string.IsNullOrWhiteSpace(cult) && IsCultivarRank(rank))
                {
                    autoriCult = authorship;
                }

                if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(genusName) || string.IsNullOrWhiteSpace(nome))
                {
                    continue;
                }

                scientificName = SpecieScientificNameHelper.Compose(genusName, nome, autori, subspecie, autoriSub, varieta, autoriVar, cult, autoriCult);
                if (string.IsNullOrWhiteSpace(scientificName) || !scientificNames.Add(scientificName))
                {
                    continue;
                }

                rows.Add(new WfoNomenclatureImportRow
                {
                    FamilyName = familyName,
                    GenusName = genusName,
                    ScientificName = scientificName,
                    Nome = nome,
                    Autori = autori,
                    Subspecie = subspecie,
                    AutoriSub = autoriSub,
                    Varieta = varieta,
                    AutoriVar = autoriVar,
                    Cult = cult,
                    AutoriCult = autoriCult,
                    Lsid = ExtractLsidValue(taxonId)
                });
            }

            return rows;
        }

        private static async Task<List<WfoNomenclatureImportRow>> ParseWfoNomenclatureZipAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            var taxonEntry = archive.Entries.FirstOrDefault(e => string.Equals(e.Name, "taxon.tsv", StringComparison.OrdinalIgnoreCase));
            var nameEntry = archive.Entries.FirstOrDefault(e => string.Equals(e.Name, "name.tsv", StringComparison.OrdinalIgnoreCase));

            if (taxonEntry != null && nameEntry != null)
            {
                return await ParseWfoDwcaArchiveAsync(archive, cancellationToken);
            }

            var bestRows = new List<WfoNomenclatureImportRow>();

            foreach (var entry in archive.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Name) &&
                            (e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
                             e.Name.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase) ||
                             e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.Length))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var entryStream = entry.Open();
                using var memory = new MemoryStream();
                await entryStream.CopyToAsync(memory, cancellationToken);
                memory.Position = 0;

                var rows = await ParseWfoNomenclatureSnapshotAsync(memory, entry.Name, cancellationToken);
                if (rows.Count > bestRows.Count)
                {
                    bestRows = rows;
                }
            }

            return bestRows;
        }

        private static async Task<List<WfoNomenclatureImportRow>> ParseWfoDwcaArchiveAsync(ZipArchive archive, CancellationToken cancellationToken)
        {
            var taxonEntry = archive.Entries.First(e => string.Equals(e.Name, "taxon.tsv", StringComparison.OrdinalIgnoreCase));
            var nameEntry = archive.Entries.First(e => string.Equals(e.Name, "name.tsv", StringComparison.OrdinalIgnoreCase));

            var names = await ParseWfoNameTableAsync(nameEntry, cancellationToken);
            var taxa = await ParseWfoTaxonTableAsync(taxonEntry, cancellationToken);

            var rows = new List<WfoNomenclatureImportRow>();
            var seenScientificNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var taxon in taxa.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!names.TryGetValue(taxon.NameId, out var name))
                {
                    continue;
                }

                if (!IsImportableWfoRank(name.Rank))
                {
                    continue;
                }

                var familyName = ResolveAncestorName(taxon.Id, taxa, names, "family");
                var genusName = !string.IsNullOrWhiteSpace(name.Genus)
                    ? name.Genus
                    : ResolveAncestorName(taxon.Id, taxa, names, "genus");

                if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(genusName))
                {
                    continue;
                }

                var fullName = BuildFullScientificName(name);
                var parsed = SpecieScientificNameHelper.ParseWfoName(fullName);
                if (string.IsNullOrWhiteSpace(parsed.Nome))
                {
                    continue;
                }

                var normalizedScientificName = SpecieScientificNameHelper.Compose(
                    genusName,
                    parsed.Nome,
                    parsed.Autori,
                    parsed.Subspecie,
                    parsed.AutoriSub,
                    parsed.Varieta,
                    parsed.AutoriVar,
                    parsed.Cult,
                    parsed.AutoriCult);

                if (string.IsNullOrWhiteSpace(normalizedScientificName) || !seenScientificNames.Add(normalizedScientificName))
                {
                    continue;
                }

                rows.Add(new WfoNomenclatureImportRow
                {
                    FamilyName = familyName,
                    GenusName = genusName,
                    ScientificName = normalizedScientificName,
                    Nome = parsed.Nome,
                    Autori = parsed.Autori,
                    Subspecie = parsed.Subspecie,
                    AutoriSub = parsed.AutoriSub,
                    Varieta = parsed.Varieta,
                    AutoriVar = parsed.AutoriVar,
                    Cult = parsed.Cult,
                    AutoriCult = parsed.AutoriCult,
                    Lsid = ExtractAlternativeLsid(name.AlternativeId)
                });
            }

            return rows;
        }

        private static async Task<Dictionary<string, WfoArchiveNameRow>> ParseWfoNameTableAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return new Dictionary<string, WfoArchiveNameRow>(StringComparer.OrdinalIgnoreCase);
            }

            var headers = ParseDelimitedLine(headerLine, '\t');
            var headerMap = headers
                .Select((header, index) => new { Header = NormalizeHeaderName(header), Index = index })
                .GroupBy(x => x.Header)
                .ToDictionary(x => x.Key, x => x.First().Index, StringComparer.OrdinalIgnoreCase);

            string GetValue(List<string> values, params string[] aliases)
            {
                foreach (var alias in aliases)
                {
                    if (headerMap.TryGetValue(alias, out var index) && index < values.Count)
                    {
                        return SpecieScientificNameHelper.NormalizeSpacing(values[index]);
                    }
                }

                return string.Empty;
            }

            var result = new Dictionary<string, WfoArchiveNameRow>(StringComparer.OrdinalIgnoreCase);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = ParseDelimitedLine(line, '\t');
                var id = GetValue(values, "id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                result[id] = new WfoArchiveNameRow
                {
                    Id = id,
                    ScientificName = GetValue(values, "scientificname"),
                    Authorship = GetValue(values, "authorship"),
                    Rank = GetValue(values, "rank"),
                    Uninomial = GetValue(values, "uninomial"),
                    Genus = GetValue(values, "genus"),
                    SpecificEpithet = GetValue(values, "specificepithet"),
                    InfraspecificEpithet = GetValue(values, "infraspecificepithet"),
                    AlternativeId = GetValue(values, "alternativeid")
                };
            }

            return result;
        }

        private static async Task<Dictionary<string, WfoArchiveTaxonRow>> ParseWfoTaxonTableAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return new Dictionary<string, WfoArchiveTaxonRow>(StringComparer.OrdinalIgnoreCase);
            }

            var headers = ParseDelimitedLine(headerLine, '\t');
            var headerMap = headers
                .Select((header, index) => new { Header = NormalizeHeaderName(header), Index = index })
                .GroupBy(x => x.Header)
                .ToDictionary(x => x.Key, x => x.First().Index, StringComparer.OrdinalIgnoreCase);

            string GetValue(List<string> values, params string[] aliases)
            {
                foreach (var alias in aliases)
                {
                    if (headerMap.TryGetValue(alias, out var index) && index < values.Count)
                    {
                        return SpecieScientificNameHelper.NormalizeSpacing(values[index]);
                    }
                }

                return string.Empty;
            }

            var result = new Dictionary<string, WfoArchiveTaxonRow>(StringComparer.OrdinalIgnoreCase);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = ParseDelimitedLine(line, '\t');
                var id = GetValue(values, "id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                result[id] = new WfoArchiveTaxonRow
                {
                    Id = id,
                    NameId = GetValue(values, "nameid"),
                    ParentId = GetValue(values, "parentid")
                };
            }

            return result;
        }

        private static bool IsImportableWfoRank(string rank)
        {
            var normalized = NormalizeHeaderName(rank);
            return normalized is "species" or "subspecies" or "variety" or "cultivar";
        }

        private static string ResolveAncestorName(
            string taxonId,
            IReadOnlyDictionary<string, WfoArchiveTaxonRow> taxa,
            IReadOnlyDictionary<string, WfoArchiveNameRow> names,
            string targetRank)
        {
            var currentId = taxonId;
            var normalizedTargetRank = NormalizeHeaderName(targetRank);

            while (!string.IsNullOrWhiteSpace(currentId) && taxa.TryGetValue(currentId, out var currentTaxon))
            {
                if (names.TryGetValue(currentTaxon.NameId, out var currentName) &&
                    NormalizeHeaderName(currentName.Rank) == normalizedTargetRank)
                {
                    return normalizedTargetRank == "genus"
                        ? (currentName.Genus ?? currentName.Uninomial ?? string.Empty)
                        : currentName.ScientificName ?? currentName.Uninomial ?? string.Empty;
                }

                currentId = currentTaxon.ParentId;
            }

            return string.Empty;
        }

        private static string BuildFullScientificName(WfoArchiveNameRow name)
        {
            var scientificName = SpecieScientificNameHelper.NormalizeSpacing(name.ScientificName);
            var authorship = SpecieScientificNameHelper.NormalizeSpacing(name.Authorship);
            return string.IsNullOrWhiteSpace(authorship) ? scientificName : $"{scientificName} {authorship}";
        }

        private static string ExtractAlternativeLsid(string alternativeIds)
        {
            var tokens = (alternativeIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var ipni = tokens.FirstOrDefault(x => x.StartsWith("urn:lsid:ipni.org:", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(ipni))
            {
                return ipni;
            }

            var anyLsid = tokens.FirstOrDefault(x => x.StartsWith("urn:lsid:", StringComparison.OrdinalIgnoreCase));
            return anyLsid ?? string.Empty;
        }

        private static string TruncateForDb(string value, int maxLength)
        {
            value = SpecieScientificNameHelper.NormalizeSpacing(value);
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength].Trim();
        }

        private static char DetectDelimiter(string headerLine, string fileName)
        {
            if (fileName.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
            {
                return '\t';
            }

            var tabCount = headerLine.Count(c => c == '\t');
            var semicolonCount = headerLine.Count(c => c == ';');
            var commaCount = headerLine.Count(c => c == ',');

            if (tabCount >= semicolonCount && tabCount >= commaCount)
            {
                return '\t';
            }

            return semicolonCount > commaCount ? ';' : ',';
        }

        private static List<string> ParseDelimitedLine(string line, char delimiter)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var insideQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }

                    continue;
                }

                if (ch == delimiter && !insideQuotes)
                {
                    values.Add(SpecieScientificNameHelper.NormalizeSpacing(current.ToString()));
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            values.Add(SpecieScientificNameHelper.NormalizeSpacing(current.ToString()));
            return values;
        }

        private static string NormalizeHeaderName(string header)
        {
            return new string((header ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        private static bool IsSubspeciesRank(string rank)
        {
            var normalized = NormalizeHeaderName(rank);
            return normalized.Contains("subspecies") || normalized.Contains("subsp") || normalized.Contains("ssp");
        }

        private static bool IsVarietyRank(string rank)
        {
            var normalized = NormalizeHeaderName(rank);
            return normalized.Contains("variety") || normalized == "var";
        }

        private static bool IsCultivarRank(string rank)
        {
            var normalized = NormalizeHeaderName(rank);
            return normalized.Contains("cultivar") || normalized == "cult";
        }

        private static string ExtractLsidValue(string rawValue)
        {
            rawValue = SpecieScientificNameHelper.NormalizeSpacing(rawValue);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return string.Empty;
            }

            if (rawValue.StartsWith("urn:lsid:", StringComparison.OrdinalIgnoreCase))
            {
                return rawValue;
            }

            if (Uri.TryCreate(rawValue, UriKind.Absolute, out var uri))
            {
                var lastSegment = uri.Segments.LastOrDefault()?.Trim('/');
                if (!string.IsNullOrWhiteSpace(lastSegment) && lastSegment.StartsWith("urn:lsid:", StringComparison.OrdinalIgnoreCase))
                {
                    return lastSegment;
                }
            }

            return string.Empty;
        }

        private static string GetDownloadFileName(HttpResponseMessage response, Uri sourceUri)
        {
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? sourceUri.Segments.LastOrDefault()
                ?? "wfo_snapshot.csv";

            fileName = fileName.Trim('"');
            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                fileName += ".csv";
            }

            return fileName;
        }

        private static string BuildCachedWfoSnapshotToken(string fileName)
        {
            return $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        }

        private static string GetCachedWfoSnapshotPath(string token)
        {
            var safeToken = Path.GetFileName(token);
            return Path.Combine(Path.GetTempPath(), "UPlant", "WfoNomenclatureImports", safeToken);
        }

        private static string GetCachedWfoCheckpointPath(string token)
        {
            var safeToken = Path.GetFileNameWithoutExtension(Path.GetFileName(token));
            return Path.Combine(Path.GetTempPath(), "UPlant", "WfoNomenclatureImports", $"{safeToken}.checkpoint.json");
        }

        private static async Task SaveWfoImportCheckpointAsync(string token, WfoNomenclatureImportCheckpoint checkpoint, CancellationToken cancellationToken)
        {
            var checkpointPath = GetCachedWfoCheckpointPath(token);
            Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
            await using var stream = System.IO.File.Create(checkpointPath);
            await JsonSerializer.SerializeAsync(stream, checkpoint, cancellationToken: cancellationToken);
        }

        private static async Task<WfoNomenclatureImportCheckpoint> LoadWfoImportCheckpointAsync(string token, CancellationToken cancellationToken)
        {
            var checkpointPath = GetCachedWfoCheckpointPath(token);
            if (!System.IO.File.Exists(checkpointPath))
            {
                return null;
            }

            await using var stream = System.IO.File.OpenRead(checkpointPath);
            return await JsonSerializer.DeserializeAsync<WfoNomenclatureImportCheckpoint>(stream, cancellationToken: cancellationToken);
        }

        private static Task DeleteWfoImportCheckpointAsync(string token)
        {
            var checkpointPath = GetCachedWfoCheckpointPath(token);
            if (System.IO.File.Exists(checkpointPath))
            {
                System.IO.File.Delete(checkpointPath);
            }

            return Task.CompletedTask;
        }

        private async Task<InterruptedWfoImportInfo> TryLoadInterruptedWfoImportAsync(CancellationToken cancellationToken)
        {
            var importDirectory = Path.Combine(Path.GetTempPath(), "UPlant", "WfoNomenclatureImports");
            if (!Directory.Exists(importDirectory))
            {
                return null;
            }

            var checkpointFiles = Directory
                .EnumerateFiles(importDirectory, "*.checkpoint.json", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            foreach (var checkpointFile in checkpointFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tokenBase = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(checkpointFile.Name));
                var matchedZip = Directory
                    .EnumerateFiles(importDirectory, $"{tokenBase}.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(path =>
                    {
                        var fileName = Path.GetFileName(path);
                        return !fileName.EndsWith(".checkpoint.json", StringComparison.OrdinalIgnoreCase);
                    });

                if (string.IsNullOrWhiteSpace(matchedZip) || !System.IO.File.Exists(matchedZip))
                {
                    continue;
                }

                var checkpoint = await LoadWfoImportCheckpointAsync(Path.GetFileName(matchedZip), cancellationToken);
                if (checkpoint == null)
                {
                    continue;
                }

                return new InterruptedWfoImportInfo
                {
                    FileToken = Path.GetFileName(matchedZip),
                    FileName = Path.GetFileName(matchedZip),
                    FileUrl = Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token = Path.GetFileName(matchedZip) }) ?? string.Empty,
                    UpdatedAtUtc = checkpoint.UpdatedAtUtc,
                    Message = string.IsNullOrWhiteSpace(checkpoint.LastScientificName)
                        ? "Ho trovato un import WFO interrotto pronto da riprendere."
                        : $"Ho trovato un import WFO interrotto. Ultima specie processata: {checkpoint.LastScientificName}."
                };
            }

            return null;
        }

        private async Task<WfoSnapshotCacheInfo> TryLoadLatestCachedWfoSnapshotAsync(CancellationToken cancellationToken)
        {
            var importDirectory = Path.Combine(Path.GetTempPath(), "UPlant", "WfoNomenclatureImports");
            if (!Directory.Exists(importDirectory))
            {
                return null;
            }

            var metadataFiles = Directory
                .EnumerateFiles(importDirectory, "*.snapshot.json", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            foreach (var metadataFile in metadataFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var metadataStream = System.IO.File.OpenRead(metadataFile.FullName);
                var metadata = await JsonSerializer.DeserializeAsync<WfoSnapshotCacheInfo>(metadataStream, cancellationToken: cancellationToken);
                if (metadata == null || string.IsNullOrWhiteSpace(metadata.FileToken))
                {
                    continue;
                }

                var cachedFilePath = GetCachedWfoSnapshotPath(metadata.FileToken);
                if (!System.IO.File.Exists(cachedFilePath))
                {
                    continue;
                }

                metadata.FileUrl = Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token = metadata.FileToken }) ?? string.Empty;
                metadata.Message = IsCurrentOfficialWfoDataset(metadata.FileName)
                    ? $"Ho trovato un dataset WFO gia scaricato e allineato all'ultima versione ({DefaultWfoDatasetLabel})."
                    : $"Ho trovato un dataset WFO gia scaricato, ma l'ultima versione ufficiale disponibile e {DefaultWfoDatasetLabel}: ti conviene riscaricare.";
                return metadata;
            }

            var fallbackZip = Directory
                .EnumerateFiles(importDirectory, "*.zip", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault();

            if (fallbackZip == null)
            {
                return null;
            }

            return new WfoSnapshotCacheInfo
            {
                FileToken = fallbackZip.Name,
                FileName = DefaultWfoDatasetFileName,
                FileUrl = Url.Action(nameof(PreviewWfoNomenclatureSnapshot), new { token = fallbackZip.Name }) ?? string.Empty,
                SourceUrl = DefaultWfoDatasetDownloadUrl,
                DownloadedAtUtc = fallbackZip.LastWriteTimeUtc,
                Message = IsCurrentOfficialWfoDataset(DefaultWfoDatasetFileName)
                    ? "Ho trovato l'ultimo zip WFO gia scaricato in cache."
                    : $"Ho trovato uno zip WFO in cache, ma l'ultima versione ufficiale disponibile e {DefaultWfoDatasetLabel}: ti conviene riscaricare."
            };
        }

        private static bool IsCurrentOfficialWfoDataset(string fileName)
        {
            return string.Equals(Path.GetFileName(fileName), DefaultWfoDatasetFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDatasetLabelFromFileName(string fileName)
        {
            var normalized = Path.GetFileNameWithoutExtension(fileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "dataset non identificato";
            }

            var match = System.Text.RegularExpressions.Regex.Match(normalized, @"(?<year>20\d{2})-(?<month>\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success &&
                int.TryParse(match.Groups["month"].Value, out var month) &&
                int.TryParse(match.Groups["year"].Value, out var year) &&
                month is >= 1 and <= 12)
            {
                var monthName = CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetMonthName(month);
                return $"{monthName} {year}";
            }

            return normalized;
        }

        private static string GetCachedWfoSnapshotMetadataPath(string token)
        {
            var safeToken = Path.GetFileNameWithoutExtension(Path.GetFileName(token));
            return Path.Combine(Path.GetTempPath(), "UPlant", "WfoNomenclatureImports", $"{safeToken}.snapshot.json");
        }

        private static async Task SaveWfoSnapshotMetadataAsync(WfoSnapshotCacheInfo metadata, CancellationToken cancellationToken)
        {
            var metadataPath = GetCachedWfoSnapshotMetadataPath(metadata.FileToken);
            Directory.CreateDirectory(Path.GetDirectoryName(metadataPath)!);
            await using var stream = System.IO.File.Create(metadataPath);
            await JsonSerializer.SerializeAsync(stream, metadata, cancellationToken: cancellationToken);
        }

        private static string GetWfoAuditCachePath()
        {
            return Path.Combine(Path.GetTempPath(), "UPlant", "WfoAudit", "latest-audit.json");
        }

        private static int GetWfoDatabaseAuditSnapshotCount(WfoDatabaseAuditJobSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            return snapshot.PerfectAccepted.Count + snapshot.PerfectSynonym.Count + snapshot.Ambiguous.Count + snapshot.NoMatch.Count;
        }

        private static async Task<int> CountPendingWfoAuditSpeciesAsync(Entities context, CancellationToken cancellationToken)
        {
            return await context.Specie
                .AsNoTracking()
                .CountAsync(x => x.validazione_tassonomicaNavigation != null && x.validazione_tassonomicaNavigation.descrizione == "N.D.", cancellationToken);
        }

        private static async Task SaveWfoDatabaseAuditCacheAsync(WfoDatabaseAuditJobSnapshot snapshot, CancellationToken cancellationToken)
        {
            var cachePath = GetWfoAuditCachePath();
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            await using var stream = System.IO.File.Create(cachePath);
            await JsonSerializer.SerializeAsync(stream, snapshot, cancellationToken: cancellationToken);
        }

        private static async Task<WfoDatabaseAuditJobSnapshot> LoadWfoDatabaseAuditCacheAsync(CancellationToken cancellationToken)
        {
            var cachePath = GetWfoAuditCachePath();
            if (!System.IO.File.Exists(cachePath))
            {
                return null;
            }

            await using var stream = System.IO.File.OpenRead(cachePath);
            return await JsonSerializer.DeserializeAsync<WfoDatabaseAuditJobSnapshot>(stream, cancellationToken: cancellationToken);
        }

        private static async Task<WfoDatabaseAuditJobSnapshot> FilterWfoDatabaseAuditSnapshotAsync(
            Entities context,
            WfoDatabaseAuditJobSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            if (snapshot == null)
            {
                return null;
            }

            var snapshotIds = snapshot.PerfectAccepted
                .Select(x => x.SpecieId)
                .Concat(snapshot.PerfectSynonym.Select(x => x.SpecieId))
                .Concat(snapshot.Ambiguous.Select(x => x.SpecieId))
                .Concat(snapshot.NoMatch.Select(x => x.SpecieId))
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (snapshotIds.Count == 0)
            {
                snapshot.TotalSpecies = 0;
                snapshot.CheckedSpecies = 0;
                snapshot.Percent = snapshot.Status == "completed" ? 100 : 0;
                return snapshot;
            }

            var pendingIds = await context.Specie
                .AsNoTracking()
                .Where(x => snapshotIds.Contains(x.id) && x.validazione_tassonomicaNavigation != null && x.validazione_tassonomicaNavigation.descrizione == "N.D.")
                .Select(x => x.id)
                .ToListAsync(cancellationToken);

            var pendingSet = pendingIds.ToHashSet();

            snapshot.PerfectAccepted = snapshot.PerfectAccepted.Where(x => pendingSet.Contains(x.SpecieId)).ToList();
            snapshot.PerfectSynonym = snapshot.PerfectSynonym.Where(x => pendingSet.Contains(x.SpecieId)).ToList();
            snapshot.Ambiguous = snapshot.Ambiguous.Where(x => pendingSet.Contains(x.SpecieId)).ToList();
            snapshot.NoMatch = snapshot.NoMatch.Where(x => pendingSet.Contains(x.SpecieId)).ToList();
            snapshot.TotalSpecies = pendingSet.Count;
            snapshot.CheckedSpecies = Math.Min(GetWfoDatabaseAuditSnapshotCount(snapshot), snapshot.TotalSpecies);
            snapshot.Percent = snapshot.TotalSpecies <= 0
                ? 100
                : Math.Min(100, (int)Math.Round(GetWfoDatabaseAuditSnapshotCount(snapshot) * 100d / snapshot.TotalSpecies));

            return snapshot;
        }

        private sealed class WfoNomenclatureImportRow
        {
            public string FamilyName { get; set; } = string.Empty;

            public string GenusName { get; set; } = string.Empty;

            public string ScientificName { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Autori { get; set; } = string.Empty;

            public string Subspecie { get; set; } = string.Empty;

            public string AutoriSub { get; set; } = string.Empty;

            public string Varieta { get; set; } = string.Empty;

            public string AutoriVar { get; set; } = string.Empty;

            public string Cult { get; set; } = string.Empty;

            public string AutoriCult { get; set; } = string.Empty;

            public string Lsid { get; set; } = string.Empty;
        }

        private sealed class WfoArchiveNameRow
        {
            public string Id { get; set; } = string.Empty;

            public string ScientificName { get; set; } = string.Empty;

            public string Authorship { get; set; } = string.Empty;

            public string Rank { get; set; } = string.Empty;

            public string Uninomial { get; set; } = string.Empty;

            public string Genus { get; set; } = string.Empty;

            public string SpecificEpithet { get; set; } = string.Empty;

            public string InfraspecificEpithet { get; set; } = string.Empty;

            public string AlternativeId { get; set; } = string.Empty;
        }

        private sealed class WfoArchiveTaxonRow
        {
            public string Id { get; set; } = string.Empty;

            public string NameId { get; set; } = string.Empty;

            public string ParentId { get; set; } = string.Empty;
        }

        private bool SpecieExists(Guid id)
        {
            return _context.Specie.Any(e => e.id == id);
        }

        private async Task PopulateSelectionsAsync(Specie specie = null)
        {
            var organizationId = await GetCurrentUserOrganizationAsync();
            var validazioneQuery = _context.ValidazioneTassonomica
                .Include(x => x.organizzazioneNavigation)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                validazioneQuery = validazioneQuery.Where(x => x.organizzazione == organizationId.Value);
            }

            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(x => x.descrizione), "id", "descrizione", specie?.areale);
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(x => x.ordinamento), "id", "codice", specie?.cites);
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(x => x.descrizione), "id", "descrizione", specie?.genere);
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice", specie?.iucn_globale);
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice", specie?.iucn_italia);
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(x => x.ordinamento), "id", "descrizione", specie?.regno);
            ViewData["validazione_tassonomica"] = new SelectList(await validazioneQuery.OrderBy(x => x.ordinamento).ToListAsync(), "id", "descrizione", specie?.validazione_tassonomica);
        }

        private async Task<string> ComposeScientificNameAsync(Guid genereId, Specie specie)
        {
            var genus = await _context.Generi
                .Where(g => g.id == genereId)
                .Select(g => g.descrizione)
                .FirstOrDefaultAsync();

            return SpecieScientificNameHelper.Compose(genus ?? string.Empty, specie.nome, specie.autori, specie.subspecie, specie.autorisub, specie.varieta, specie.autorivar, specie.cult, specie.autoricult);
        }

        private async Task<Guid?> GetCurrentUserOrganizationAsync()
        {
            var username = User.Identities.FirstOrDefault()?.Claims?.FirstOrDefault(c => c.Type == "UnipiUserID")?.Value;
            if (string.IsNullOrWhiteSpace(username) || !username.Contains("@"))
            {
                return null;
            }

            var usernameKey = username[..username.IndexOf("@", StringComparison.Ordinal)];
            return await _context.Users
                .Where(a => a.UnipiUserName == usernameKey)
                .Select(a => (Guid?)a.Organizzazione)
                .FirstOrDefaultAsync();
        }

        private async Task<Guid> EnsureDefaultValidazioneTassonomicaAsync(string statusName)
        {
            var organizationId = await GetCurrentUserOrganizationAsync();
            if (!organizationId.HasValue)
            {
                return Guid.Empty;
            }

            return await EnsureDefaultValidazioneTassonomicaAsync(_context, organizationId.Value, statusName, CancellationToken.None);
        }

        private static async Task<Guid> EnsureDefaultValidazioneTassonomicaAsync(
            Entities context,
            Guid organizationId,
            string statusName,
            CancellationToken cancellationToken)
        {
            var existingId = await context.ValidazioneTassonomica
                .Where(x => x.organizzazione == organizationId &&
                            (x.descrizione == statusName || x.descrizione_en == statusName))
                .Select(x => x.id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingId != Guid.Empty)
            {
                return existingId;
            }

            var nextOrder = (await context.ValidazioneTassonomica
                .Where(x => x.organizzazione == organizationId)
                .MaxAsync(x => (int?)x.ordinamento, cancellationToken) ?? 0) + 1;

            var status = new ValidazioneTassonomica
            {
                id = Guid.NewGuid(),
                descrizione = statusName,
                descrizione_en = statusName,
                ordinamento = nextOrder,
                organizzazione = organizationId
            };

            context.ValidazioneTassonomica.Add(status);
            await context.SaveChangesAsync(cancellationToken);
            return status.id;
        }

        private static async Task<(int removedGenera, int removedFamilies)> CleanupOrphanTaxonomyAsync(
            Entities context,
            CancellationToken cancellationToken)
        {
            var orphanGenera = await context.Generi
                .Where(g => !g.Specie.Any())
                .ToListAsync(cancellationToken);

            var removedGenera = orphanGenera.Count;
            if (removedGenera > 0)
            {
                context.Generi.RemoveRange(orphanGenera);
                await context.SaveChangesAsync(cancellationToken);
            }

            var orphanFamilies = await context.Famiglie
                .Where(f => !f.Generi.Any())
                .ToListAsync(cancellationToken);

            var removedFamilies = orphanFamilies.Count;
            if (removedFamilies > 0)
            {
                context.Famiglie.RemoveRange(orphanFamilies);
                await context.SaveChangesAsync(cancellationToken);
            }

            return (removedGenera, removedFamilies);
        }

        private async Task<bool> ApplyAcceptedNameAsync(Specie specie, string acceptedFullName, string lsid)
        {
            var parsed = SpecieScientificNameHelper.ParseWfoName(acceptedFullName);
            if (string.IsNullOrWhiteSpace(parsed.Genus) || string.IsNullOrWhiteSpace(parsed.Nome))
            {
                return false;
            }

            var genusId = await _context.Generi
                .Where(g => g.descrizione == parsed.Genus)
                .Select(g => (Guid?)g.id)
                .FirstOrDefaultAsync();

            if (!genusId.HasValue)
            {
                return false;
            }

            specie.genere = genusId.Value;
            specie.nome = parsed.Nome;
            specie.autori = parsed.Autori;
            specie.subspecie = parsed.Subspecie;
            specie.autorisub = parsed.AutoriSub;
            specie.varieta = parsed.Varieta;
            specie.autorivar = parsed.AutoriVar;
            specie.cult = parsed.Cult;
            specie.autoricult = parsed.AutoriCult;
            specie.lsid = string.IsNullOrWhiteSpace(lsid) ? specie.lsid : lsid.Trim();
            specie.nome_scientifico = SpecieScientificNameHelper.Compose(parsed.Genus, parsed.Nome, parsed.Autori, parsed.Subspecie, parsed.AutoriSub, parsed.Varieta, parsed.AutoriVar, parsed.Cult, parsed.AutoriCult);
            return true;
        }

        private async Task<bool> EnsureGenusForAcceptedNameAsync(string acceptedFullName, string familyName)
        {
            var parsed = SpecieScientificNameHelper.ParseWfoName(acceptedFullName);
            var genusName = SpecieScientificNameHelper.NormalizeSpacing(parsed.Genus);
            var normalizedFamilyName = SpecieScientificNameHelper.NormalizeSpacing(familyName);

            if (string.IsNullOrWhiteSpace(genusName) || string.IsNullOrWhiteSpace(normalizedFamilyName))
            {
                return false;
            }

            var existingGenus = await _context.Generi.AnyAsync(g => g.descrizione == genusName);
            if (existingGenus)
            {
                return true;
            }

            var familyId = await _context.Famiglie
                .Where(f => f.descrizione == normalizedFamilyName)
                .Select(f => (Guid?)f.id)
                .FirstOrDefaultAsync();

            if (!familyId.HasValue)
            {
                return false;
            }

            _context.Generi.Add(new Generi
            {
                id = Guid.NewGuid(),
                descrizione = genusName,
                famiglia = familyId.Value
            });

            await _context.SaveChangesAsync();
            return true;
        }

        private ApplyWfoDecisionInput BuildFormInput(Specie specie, string genusName, string currentValidationStatusName)
        {
            return new ApplyWfoDecisionInput
            {
                SpecieId = specie.id,
                ActionType = "save_review",
                ValidationStatusName = currentValidationStatusName ?? string.Empty,
                AcceptedFullName = specie.nome_scientifico ?? string.Empty,
                Lsid = specie.lsid ?? string.Empty,
                GenusName = genusName,
                Nome = specie.nome ?? string.Empty,
                Autori = specie.autori ?? string.Empty,
                Subspecie = specie.subspecie ?? string.Empty,
                AutoriSub = specie.autorisub ?? string.Empty,
                Varieta = specie.varieta ?? string.Empty,
                AutoriVar = specie.autorivar ?? string.Empty,
                Cult = specie.cult ?? string.Empty,
                AutoriCult = specie.autoricult ?? string.Empty,
                SuggestedIucnGlobalCode = specie.iucn_globaleNavigation?.codice ?? string.Empty,
                SuggestedIucnGlobalLabel = specie.iucn_globaleNavigation?.descrizione ?? string.Empty
            };
        }

        private static WfoApplicationOption BuildOption(string buttonLabel, string description, string validationStatusName, string wfoId, string fullName, string lsid, bool isAccepted = false, string acceptedName = "", string rank = "", string familyName = "", string iucnGlobalCode = "", string iucnGlobalLabel = "")
        {
            var parsed = SpecieScientificNameHelper.ParseWfoName(fullName);
            return new WfoApplicationOption
            {
                ButtonLabel = buttonLabel,
                Description = description,
                ValidationStatusName = validationStatusName ?? string.Empty,
                IsAccepted = isAccepted,
                AcceptedName = acceptedName ?? string.Empty,
                Rank = rank ?? string.Empty,
                FamilyName = familyName ?? string.Empty,
                WfoId = wfoId ?? string.Empty,
                FullName = fullName ?? string.Empty,
                Lsid = lsid ?? string.Empty,
                GenusName = parsed.Genus ?? string.Empty,
                Nome = parsed.Nome ?? string.Empty,
                Autori = parsed.Autori ?? string.Empty,
                Subspecie = parsed.Subspecie ?? string.Empty,
                AutoriSub = parsed.AutoriSub ?? string.Empty,
                Varieta = parsed.Varieta ?? string.Empty,
                AutoriVar = parsed.AutoriVar ?? string.Empty,
                Cult = parsed.Cult ?? string.Empty,
                AutoriCult = parsed.AutoriCult ?? string.Empty,
                IucnGlobalCode = iucnGlobalCode ?? string.Empty,
                IucnGlobalLabel = iucnGlobalLabel ?? string.Empty
            };
        }

        private async Task<bool> ApplyReviewFormAsync(Specie specie, ApplyWfoDecisionInput input)
        {
            var genusName = SpecieScientificNameHelper.NormalizeSpacing(input.GenusName);
            if (string.IsNullOrWhiteSpace(genusName))
            {
                return false;
            }

            var genusId = await _context.Generi
                .Where(g => g.descrizione == genusName)
                .Select(g => (Guid?)g.id)
                .FirstOrDefaultAsync();

            if (!genusId.HasValue)
            {
                return false;
            }

            specie.genere = genusId.Value;
            specie.nome = SpecieScientificNameHelper.NormalizeSpacing(input.Nome);
            specie.autori = SpecieScientificNameHelper.NormalizeSpacing(input.Autori);
            specie.subspecie = SpecieScientificNameHelper.NormalizeSpacing(input.Subspecie);
            specie.autorisub = SpecieScientificNameHelper.NormalizeSpacing(input.AutoriSub);
            specie.varieta = SpecieScientificNameHelper.NormalizeSpacing(input.Varieta);
            specie.autorivar = SpecieScientificNameHelper.NormalizeSpacing(input.AutoriVar);
            specie.cult = SpecieScientificNameHelper.NormalizeSpacing(input.Cult);
            specie.autoricult = SpecieScientificNameHelper.NormalizeSpacing(input.AutoriCult);
            specie.lsid = SpecieScientificNameHelper.NormalizeSpacing(input.Lsid);

            if (input.ApplySuggestedIucnGlobal)
            {
                specie.iucn_globale = await ResolveIucnIdByCodeAsync(input.SuggestedIucnGlobalCode);
            }

            specie.nome_scientifico = SpecieScientificNameHelper.Compose(
                genusName,
                specie.nome,
                specie.autori,
                specie.subspecie,
                specie.autorisub,
                specie.varieta,
                specie.autorivar,
                specie.cult,
                specie.autoricult);

            return true;
        }

        private async Task<Guid?> ResolveIucnIdByCodeAsync(string iucnCode)
        {
            var normalizedCode = SpecieScientificNameHelper.NormalizeSpacing(iucnCode);
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                return null;
            }

            return await _context.Iucn
                .Where(x => x.codice == normalizedCode)
                .Select(x => (Guid?)x.id)
                .FirstOrDefaultAsync();
        }

        private static string ResolveValidationStatusName(ApplyWfoDecisionInput input)
        {
            var requestedStatus = SpecieScientificNameHelper.NormalizeSpacing(input.ValidationStatusName);
            if (!string.IsNullOrWhiteSpace(requestedStatus))
            {
                return requestedStatus;
            }

            return string.Equals(input.ActionType, "accept_suggested", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(input.ActionType, "keep_current", StringComparison.OrdinalIgnoreCase)
                ? "WFO"
                : "A.A.";
        }

        private async Task PopulateMissingGenusSuggestionAsync(SpecieWfoReviewViewModel model, CancellationToken cancellationToken)
        {
            var genusName = SpecieScientificNameHelper.NormalizeSpacing(model.Form.GenusName);
            if (string.IsNullOrWhiteSpace(genusName))
            {
                return;
            }

            var genusExists = await _context.Generi.AnyAsync(g => g.descrizione == genusName, cancellationToken);
            if (genusExists)
            {
                return;
            }

            var familyName = model.SynonymCandidateOptions
                .Where(x => string.Equals(x.GenusName, genusName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FamilyName)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (string.IsNullOrWhiteSpace(familyName))
            {
                familyName = model.AcceptedCandidateOptions
                    .Where(x => string.Equals(x.GenusName, genusName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.FamilyName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            }

            if (string.IsNullOrWhiteSpace(familyName))
            {
                familyName = model.AcceptedCandidateOptions
                    .Select(x => x.FamilyName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            }

            model.SuggestedMissingGenusName = genusName;
            model.SuggestedMissingFamilyName = familyName ?? string.Empty;
            model.CanCreateSuggestedGenus = !string.IsNullOrWhiteSpace(model.SuggestedMissingFamilyName);
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static List<WfoAcceptedCandidateGroup> BuildAcceptedCandidateGroups(
            Specie specie,
            IEnumerable<WfoApplicationOption> acceptedOptions,
            IEnumerable<WfoApplicationOption> synonymOptions)
        {
            var currentName = SpecieScientificNameHelper.Compose(
                specie.genereNavigation?.descrizione ?? string.Empty,
                specie.nome,
                specie.autori,
                specie.subspecie,
                specie.autorisub,
                specie.varieta,
                specie.autorivar,
                specie.cult,
                specie.autoricult);

            var acceptedByName = acceptedOptions
                .Where(x => !string.IsNullOrWhiteSpace(x.FullName))
                .GroupBy(x => SpecieScientificNameHelper.NormalizeSpacing(x.FullName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var groups = new Dictionary<string, WfoAcceptedCandidateGroup>(StringComparer.OrdinalIgnoreCase);

            foreach (var acceptedOption in acceptedByName.Values)
            {
                var normalizedAcceptedName = SpecieScientificNameHelper.NormalizeSpacing(acceptedOption.FullName);
                groups[normalizedAcceptedName] = new WfoAcceptedCandidateGroup
                {
                    AcceptedOption = acceptedOption,
                    MatchScore = CalculateWfoOptionMatchScore(specie, acceptedOption),
                    HasExactAcceptedMatch = string.Equals(
                        normalizedAcceptedName,
                        SpecieScientificNameHelper.NormalizeSpacing(currentName),
                        StringComparison.OrdinalIgnoreCase)
                };
            }

            foreach (var synonymOption in synonymOptions.Where(x => !string.IsNullOrWhiteSpace(x.AcceptedName)))
            {
                var normalizedAcceptedName = SpecieScientificNameHelper.NormalizeSpacing(synonymOption.AcceptedName);
                if (!groups.TryGetValue(normalizedAcceptedName, out var group))
                {
                    group = new WfoAcceptedCandidateGroup
                    {
                        AcceptedOption = BuildOption(
                            "Carica nel form",
                            "Nome accettato derivato dai sinonimi trovati in WFO.",
                            "WFO",
                            synonymOption.WfoId,
                            synonymOption.AcceptedName,
                            synonymOption.Lsid,
                            true,
                            synonymOption.AcceptedName,
                            synonymOption.Rank,
                            synonymOption.FamilyName,
                            synonymOption.IucnGlobalCode,
                            synonymOption.IucnGlobalLabel)
                    };
                    groups[normalizedAcceptedName] = group;
                }

                group.SupportingSynonyms.Add(synonymOption);
                group.MatchScore = Math.Max(group.MatchScore, CalculateWfoOptionMatchScore(specie, synonymOption));
            }

            foreach (var group in groups.Values)
            {
                group.SupportingSynonyms = group.SupportingSynonyms
                    .OrderByDescending(x => CalculateWfoOptionMatchScore(specie, x))
                    .ThenByDescending(x => !string.IsNullOrWhiteSpace(x.Subspecie))
                    .ThenBy(x => x.FullName)
                    .ToList();
            }

            return groups.Values
                .OrderByDescending(x => x.HasExactAcceptedMatch)
                .ThenByDescending(x => x.MatchScore)
                .ThenByDescending(x => x.SupportingSynonyms.Count)
                .ThenBy(x => x.AcceptedOption?.FullName)
                .ToList();
        }

        private static int CalculateWfoOptionMatchScore(Specie specie, WfoApplicationOption option)
        {
            var score = 0;

            score += ScoreToken(specie.genereNavigation?.descrizione, option.GenusName, 4);
            score += ScoreToken(specie.nome, option.Nome, 6);
            score += ScoreToken(specie.autori, option.Autori, 2);
            score += ScoreToken(specie.subspecie, option.Subspecie, 5);
            score += ScoreToken(specie.autorisub, option.AutoriSub, 3);
            score += ScoreToken(specie.varieta, option.Varieta, 4);
            score += ScoreToken(specie.autorivar, option.AutoriVar, 2);
            score += ScoreToken(specie.cult, option.Cult, 4);
            score += ScoreToken(specie.autoricult, option.AutoriCult, 2);

            return score;
        }

        private static int ScoreToken(string currentValue, string candidateValue, int weight)
        {
            var normalizedCurrent = SpecieScientificNameHelper.NormalizeSpacing(currentValue);
            var normalizedCandidate = SpecieScientificNameHelper.NormalizeSpacing(candidateValue);

            if (string.IsNullOrWhiteSpace(normalizedCurrent) || string.IsNullOrWhiteSpace(normalizedCandidate))
            {
                return 0;
            }

            return string.Equals(normalizedCurrent, normalizedCandidate, StringComparison.OrdinalIgnoreCase)
                ? weight
                : 0;
        }

        private static string BuildWfoStableUrl(string wfoId)
        {
            return $"https://list.worldfloraonline.org/{Uri.EscapeDataString(wfoId.Trim())}";
        }

        private static string BuildWfoSearchUrl(string scientificName)
        {
            return $"https://www.worldfloraonline.org/search?query={Uri.EscapeDataString(scientificName)}";
        }

        private static string TruncateNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return string.Empty;
            }

            return note.Length > 100
                ? $"{note.Substring(0, 100)} [...]"
                : note;
        }

        private static IQueryable<SpecieIndexRow> ApplyOrdering(IQueryable<SpecieIndexRow> query, int columnIndex, bool descending)
        {
            return columnIndex switch
            {
                2 => descending ? query.OrderByDescending(x => x.ValidationStatus) : query.OrderBy(x => x.ValidationStatus),
                3 => descending ? query.OrderByDescending(x => x.InsertedAt) : query.OrderBy(x => x.InsertedAt),
                4 => descending ? query.OrderByDescending(x => x.Lsid) : query.OrderBy(x => x.Lsid),
                5 => descending ? query.OrderByDescending(x => x.CommonName) : query.OrderBy(x => x.CommonName),
                6 => descending ? query.OrderByDescending(x => x.EnglishCommonName) : query.OrderBy(x => x.EnglishCommonName),
                7 => descending ? query.OrderByDescending(x => x.Family) : query.OrderBy(x => x.Family),
                8 => descending ? query.OrderByDescending(x => x.Genus) : query.OrderBy(x => x.Genus),
                9 => descending ? query.OrderByDescending(x => x.Kingdom) : query.OrderBy(x => x.Kingdom),
                10 => descending ? query.OrderByDescending(x => x.Range) : query.OrderBy(x => x.Range),
                11 => descending ? query.OrderByDescending(x => x.Cites) : query.OrderBy(x => x.Cites),
                12 => descending ? query.OrderByDescending(x => x.IucnGlobal) : query.OrderBy(x => x.IucnGlobal),
                13 => descending ? query.OrderByDescending(x => x.IucnLocal) : query.OrderBy(x => x.IucnLocal),
                14 => descending ? query.OrderByDescending(x => x.Note) : query.OrderBy(x => x.Note),
                _ => descending ? query.OrderByDescending(x => x.ScientificName) : query.OrderBy(x => x.ScientificName)
            };
        }

        private sealed class SpecieIndexRow
        {
            public Guid Id { get; set; }
            public string ScientificName { get; set; }
            public string ValidationStatus { get; set; }
            public DateTime InsertedAt { get; set; }
            public string Lsid { get; set; }
            public string CommonName { get; set; }
            public string EnglishCommonName { get; set; }
            public string Family { get; set; }
            public string Genus { get; set; }
            public string Kingdom { get; set; }
            public string Range { get; set; }
            public string Cites { get; set; }
            public string IucnGlobal { get; set; }
            public string IucnLocal { get; set; }
            public string Note { get; set; }
            public bool HasAccessioni { get; set; }
        }

        private sealed class WfoNomenclatureImportJobState
        {
            private readonly object _sync = new();
            private readonly Queue<string> _recentMessages = new();
            private readonly Queue<string> _failedItems = new();
            private readonly CancellationTokenSource _cancellationTokenSource = new();

            public WfoNomenclatureImportJobState(string jobId, string fileName, string fileToken)
            {
                JobId = jobId;
                FileName = fileName ?? string.Empty;
                FileToken = fileToken ?? string.Empty;
                Stage = "In attesa";
                Status = "pending";
                UpdatedAtUtc = DateTime.UtcNow;
            }

            public string JobId { get; }
            public string FileName { get; }
            public string FileToken { get; }
            public string Status { get; private set; }
            public string Stage { get; private set; }
            public int TotalFamilies { get; private set; }
            public int TotalGenera { get; private set; }
            public int TotalSpecies { get; private set; }
            public int CompletedSteps { get; private set; }
            public int TotalSteps { get; private set; }
            public int ImportedFamilies { get; private set; }
            public int ImportedGenera { get; private set; }
            public int ImportedSpecies { get; private set; }
            public int SkippedRows { get; private set; }
            public string CurrentItem { get; private set; } = string.Empty;
            public string ErrorMessage { get; private set; } = string.Empty;
            public DateTime UpdatedAtUtc { get; private set; }
            public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

            public void MarkRunning(string stage)
            {
                lock (_sync)
                {
                    Status = "running";
                    Stage = stage ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void SetPlannedWork(int totalFamilies, int totalGenera, int totalSpecies)
            {
                lock (_sync)
                {
                    TotalFamilies = totalFamilies;
                    TotalGenera = totalGenera;
                    TotalSpecies = totalSpecies;
                    TotalSteps = totalFamilies + totalGenera + totalSpecies;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void AddMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                lock (_sync)
                {
                    if (_recentMessages.Count >= 8)
                    {
                        _recentMessages.Dequeue();
                    }

                    _recentMessages.Enqueue(message);
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void ReportFamilyImported(string familyName, int importedFamilies)
            {
                lock (_sync)
                {
                    ImportedFamilies = importedFamilies;
                    CompletedSteps++;
                    CurrentItem = familyName ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                AddMessage($"Famiglia importata: {familyName}");
            }

            public void ReportGenusImported(string genusName, string familyName, int importedGenera)
            {
                lock (_sync)
                {
                    ImportedGenera = importedGenera;
                    CompletedSteps++;
                    CurrentItem = genusName ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                AddMessage($"Genere importato: {genusName} ({familyName})");
            }

            public void ReportSpeciesProcessed(string scientificName, int importedSpecies, int skippedRows, bool imported)
            {
                lock (_sync)
                {
                    ImportedSpecies = importedSpecies;
                    SkippedRows = skippedRows;
                    CompletedSteps++;
                    CurrentItem = scientificName ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                if (importedSpecies <= 10 || importedSpecies % 200 == 0 || !imported)
                {
                    AddMessage(imported
                        ? $"Specie importata: {scientificName}"
                        : $"Riga saltata: {scientificName}");
                }
            }

            public void Complete(int importedFamilies, int importedGenera, int importedSpecies, int skippedRows, string fileName)
            {
                lock (_sync)
                {
                    Status = "completed";
                    Stage = "Import completato";
                    ImportedFamilies = importedFamilies;
                    ImportedGenera = importedGenera;
                    ImportedSpecies = importedSpecies;
                    SkippedRows = skippedRows;
                    CompletedSteps = Math.Max(CompletedSteps, TotalSteps);
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                AddMessage($"Import completato dal file '{fileName}'.");
            }

            public void Fail(string errorMessage)
            {
                lock (_sync)
                {
                    Status = "failed";
                    Stage = "Import interrotto";
                    ErrorMessage = errorMessage ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                AddMessage(ErrorMessage);
            }

            public void RequestCancellation()
            {
                lock (_sync)
                {
                    if (Status == "completed" || Status == "failed" || Status == "paused")
                    {
                        return;
                    }

                    Status = "cancellationRequested";
                    Stage = "Interruzione richiesta";
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                _cancellationTokenSource.Cancel();
                AddMessage("Interruzione richiesta. Salvo il checkpoint.");
            }

            public void MarkPaused(string message)
            {
                lock (_sync)
                {
                    Status = "paused";
                    Stage = "Import in pausa";
                    UpdatedAtUtc = DateTime.UtcNow;
                }

                AddMessage(message);
            }

            public void RestoreFromCheckpoint(WfoNomenclatureImportCheckpoint checkpoint)
            {
                if (checkpoint == null)
                {
                    return;
                }

                lock (_sync)
                {
                    ImportedFamilies = checkpoint.ImportedFamilies;
                    ImportedGenera = checkpoint.ImportedGenera;
                    ImportedSpecies = checkpoint.ImportedSpecies;
                    SkippedRows = checkpoint.SkippedRows;
                    CompletedSteps = checkpoint.ImportedFamilies + checkpoint.ImportedGenera + checkpoint.NextSpeciesIndex;
                    CurrentItem = checkpoint.LastScientificName ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void SetImportedCounts(int importedFamilies, int importedGenera, int importedSpecies, int skippedRows, int nextSpeciesIndex)
            {
                lock (_sync)
                {
                    ImportedFamilies = importedFamilies;
                    ImportedGenera = importedGenera;
                    ImportedSpecies = importedSpecies;
                    SkippedRows = skippedRows;
                    CompletedSteps = importedFamilies + importedGenera + nextSpeciesIndex;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void ReportFailedItem(string scientificName, string reason)
            {
                var item = string.IsNullOrWhiteSpace(reason)
                    ? scientificName
                    : $"{scientificName}: {reason}";

                lock (_sync)
                {
                    if (_failedItems.Count >= 20)
                    {
                        _failedItems.Dequeue();
                    }

                    _failedItems.Enqueue(item);
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public WfoNomenclatureImportJobSnapshot CreateSnapshot()
            {
                lock (_sync)
                {
                    var percent = TotalSteps <= 0
                        ? (Status == "completed" ? 100 : 0)
                        : Math.Min(100, (int)Math.Round(CompletedSteps * 100d / TotalSteps));

                    return new WfoNomenclatureImportJobSnapshot
                    {
                        JobId = JobId,
                        FileName = FileName,
                        FileToken = FileToken,
                        Status = Status,
                        Stage = Stage,
                        CurrentItem = CurrentItem,
                        ErrorMessage = ErrorMessage,
                        ImportedFamilies = ImportedFamilies,
                        ImportedGenera = ImportedGenera,
                        ImportedSpecies = ImportedSpecies,
                        SkippedRows = SkippedRows,
                        TotalFamilies = TotalFamilies,
                        TotalGenera = TotalGenera,
                        TotalSpecies = TotalSpecies,
                        Percent = percent,
                        UpdatedAtUtc = UpdatedAtUtc,
                        RecentMessages = _recentMessages.ToList(),
                        FailedItems = _failedItems.ToList()
                    };
                }
            }
        }

        private sealed class WfoNomenclatureImportJobSnapshot
        {
            public string JobId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string FileToken { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Stage { get; set; } = string.Empty;
            public string CurrentItem { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public int ImportedFamilies { get; set; }
            public int ImportedGenera { get; set; }
            public int ImportedSpecies { get; set; }
            public int SkippedRows { get; set; }
            public int TotalFamilies { get; set; }
            public int TotalGenera { get; set; }
            public int TotalSpecies { get; set; }
            public int Percent { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public List<string> RecentMessages { get; set; } = new();
            public List<string> FailedItems { get; set; } = new();
        }

        private sealed class WfoNomenclatureImportCheckpoint
        {
            public int NextSpeciesIndex { get; set; }
            public int ImportedFamilies { get; set; }
            public int ImportedGenera { get; set; }
            public int ImportedSpecies { get; set; }
            public int SkippedRows { get; set; }
            public string LastScientificName { get; set; } = string.Empty;
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        }

        private sealed class InterruptedWfoImportInfo
        {
            public string FileToken { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string FileUrl { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime UpdatedAtUtc { get; set; }
        }

        private sealed class WfoSnapshotCacheInfo
        {
            public string FileToken { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string FileUrl { get; set; } = string.Empty;
            public string SourceUrl { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime DownloadedAtUtc { get; set; }
        }

        private sealed class WfoDatabaseAuditSourceRow
        {
            public Guid Id { get; set; }
            public string ScientificName { get; set; } = string.Empty;
            public string Nome { get; set; } = string.Empty;
            public string Autori { get; set; } = string.Empty;
            public string Subspecie { get; set; } = string.Empty;
            public string AutoriSub { get; set; } = string.Empty;
            public string Varieta { get; set; } = string.Empty;
            public string AutoriVar { get; set; } = string.Empty;
            public string Cult { get; set; } = string.Empty;
            public string AutoriCult { get; set; } = string.Empty;
            public string GenusName { get; set; } = string.Empty;
            public string CurrentValidationStatus { get; set; } = string.Empty;
        }

        private sealed class WfoDatabaseAuditItem
        {
            public Guid SpecieId { get; set; }
            public string ScientificName { get; set; } = string.Empty;
            public string CurrentValidationStatus { get; set; } = string.Empty;
            public string Section { get; set; } = string.Empty;
            public string AcceptedName { get; set; } = string.Empty;
            public string Lsid { get; set; } = string.Empty;
            public string FamilyName { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
        }

        private sealed class WfoDatabaseAuditJobState
        {
            private readonly object _sync = new();
            private readonly Queue<string> _recentMessages = new();
            private readonly List<WfoDatabaseAuditItem> _perfectAccepted = new();
            private readonly List<WfoDatabaseAuditItem> _perfectSynonym = new();
            private readonly List<WfoDatabaseAuditItem> _ambiguous = new();
            private readonly List<WfoDatabaseAuditItem> _noMatch = new();

            public WfoDatabaseAuditJobState(string jobId, int totalSpecies, StartWfoDatabaseAuditInput options)
            {
                JobId = jobId;
                TotalSpecies = totalSpecies;
                Options = new StartWfoDatabaseAuditInput
                {
                    MaxSpeciesToProcess = options.MaxSpeciesToProcess,
                    IncludePerfectAccepted = options.IncludePerfectAccepted,
                    IncludePerfectSynonym = options.IncludePerfectSynonym,
                    IncludeAmbiguous = options.IncludeAmbiguous,
                    IncludeNoMatch = options.IncludeNoMatch
                };
                Status = "pending";
                Stage = "In attesa";
                UpdatedAtUtc = DateTime.UtcNow;
            }

            public string JobId { get; }
            public int TotalSpecies { get; }
            public StartWfoDatabaseAuditInput Options { get; }
            public int CheckedSpecies { get; private set; }
            public string Status { get; private set; }
            public string Stage { get; private set; }
            public string CurrentItem { get; private set; } = string.Empty;
            public string ErrorMessage { get; private set; } = string.Empty;
            public DateTime UpdatedAtUtc { get; private set; }

            public void MarkRunning(string stage)
            {
                lock (_sync)
                {
                    Status = "running";
                    Stage = stage ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void AddMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                lock (_sync)
                {
                    if (_recentMessages.Count >= 8)
                    {
                        _recentMessages.Dequeue();
                    }

                    _recentMessages.Enqueue(message);
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void ReportItem(WfoDatabaseAuditItem item, bool includeInResults)
            {
                lock (_sync)
                {
                    CheckedSpecies++;
                    CurrentItem = item.ScientificName ?? string.Empty;

                    if (includeInResults)
                    {
                        switch (item.Section)
                        {
                            case "perfectAccepted":
                                _perfectAccepted.Add(item);
                                break;
                            case "perfectSynonym":
                                _perfectSynonym.Add(item);
                                break;
                            case "ambiguous":
                                _ambiguous.Add(item);
                                break;
                            case "error":
                                _noMatch.Add(item);
                                break;
                            default:
                                _noMatch.Add(item);
                                break;
                        }
                    }

                    UpdatedAtUtc = DateTime.UtcNow;
                }

                if (CheckedSpecies <= 10 || CheckedSpecies % 100 == 0)
                {
                    var outcome = item.Section == "error"
                        ? $"errore ({item.Notes})"
                        : item.Section;
                    AddMessage($"{item.ScientificName} -> {outcome}");
                }
            }

            public void Complete()
            {
                lock (_sync)
                {
                    Status = "completed";
                    Stage = "Audit completato";
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public void Fail(string message)
            {
                lock (_sync)
                {
                    Status = "failed";
                    Stage = "Audit interrotto";
                    ErrorMessage = message ?? string.Empty;
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            public WfoDatabaseAuditJobSnapshot CreateSnapshot()
            {
                lock (_sync)
                {
                    var percent = TotalSpecies <= 0
                        ? (Status == "completed" ? 100 : 0)
                        : Math.Min(100, (int)Math.Round(CheckedSpecies * 100d / TotalSpecies));

                    return new WfoDatabaseAuditJobSnapshot
                    {
                        JobId = JobId,
                        TotalSpecies = TotalSpecies,
                        CheckedSpecies = CheckedSpecies,
                        MaxSpeciesToProcess = Options.MaxSpeciesToProcess,
                        IncludePerfectAccepted = Options.IncludePerfectAccepted,
                        IncludePerfectSynonym = Options.IncludePerfectSynonym,
                        IncludeAmbiguous = Options.IncludeAmbiguous,
                        IncludeNoMatch = Options.IncludeNoMatch,
                        Status = Status,
                        Stage = Stage,
                        CurrentItem = CurrentItem,
                        ErrorMessage = ErrorMessage,
                        Percent = percent,
                        UpdatedAtUtc = UpdatedAtUtc,
                        RecentMessages = _recentMessages.ToList(),
                        PerfectAccepted = _perfectAccepted.ToList(),
                        PerfectSynonym = _perfectSynonym.ToList(),
                        Ambiguous = _ambiguous.ToList(),
                        NoMatch = _noMatch.ToList()
                    };
                }
            }
        }

        private sealed class WfoDatabaseAuditJobSnapshot
        {
            public string JobId { get; set; } = string.Empty;
            public int TotalSpecies { get; set; }
            public int CheckedSpecies { get; set; }
            public int MaxSpeciesToProcess { get; set; }
            public bool IncludePerfectAccepted { get; set; }
            public bool IncludePerfectSynonym { get; set; }
            public bool IncludeAmbiguous { get; set; }
            public bool IncludeNoMatch { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Stage { get; set; } = string.Empty;
            public string CurrentItem { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public int Percent { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public List<string> RecentMessages { get; set; } = new();
            public List<WfoDatabaseAuditItem> PerfectAccepted { get; set; } = new();
            public List<WfoDatabaseAuditItem> PerfectSynonym { get; set; } = new();
            public List<WfoDatabaseAuditItem> Ambiguous { get; set; } = new();
            public List<WfoDatabaseAuditItem> NoMatch { get; set; } = new();
        }
    }
}
