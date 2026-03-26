using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using UPlant.Models.DB;
using UPlant.Models.ViewModels;
using UPlant.Services;

namespace UPlant.Controllers
{
    public class SpecieController : BaseController
    {
        private const int MaxPageSize = 100;
        private readonly Entities _context;
        private readonly IWorldFloraOnlineService _worldFloraOnlineService;

        public SpecieController(Entities context, IWorldFloraOnlineService worldFloraOnlineService)
        {
            _context = context;
            _worldFloraOnlineService = worldFloraOnlineService;
        }

        public IActionResult Index()
        {
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

            if (!familyId.HasValue)
            {
                TempData["WfoError"] = $"La famiglia {familyName} non esiste in archivio, quindi non posso creare automaticamente il genere {genusName}.";
                TempData["PendingWfoForm"] = input.PendingFormJson ?? string.Empty;
                return RedirectToAction(nameof(ReviewWfo), new { id = input.SpecieId });
            }

            _context.Generi.Add(new Generi
            {
                id = Guid.NewGuid(),
                descrizione = genusName,
                famiglia = familyId.Value
            });

            await _context.SaveChangesAsync(cancellationToken);
            TempData["PendingWfoForm"] = input.PendingFormJson ?? string.Empty;
            TempData["WfoSuccess"] = $"Ho inserito il genere {genusName} collegato alla famiglia {familyName}.";
            return RedirectToAction(nameof(ReviewWfo), new { id = input.SpecieId });
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

            var existingId = await _context.ValidazioneTassonomica
                .Where(x => x.organizzazione == organizationId.Value &&
                            (x.descrizione == statusName || x.descrizione_en == statusName))
                .Select(x => x.id)
                .FirstOrDefaultAsync();

            if (existingId != Guid.Empty)
            {
                return existingId;
            }

            var nextOrder = (await _context.ValidazioneTassonomica
                .Where(x => x.organizzazione == organizationId.Value)
                .MaxAsync(x => (int?)x.ordinamento) ?? 0) + 1;

            var status = new ValidazioneTassonomica
            {
                id = Guid.NewGuid(),
                descrizione = statusName,
                descrizione_en = statusName,
                ordinamento = nextOrder,
                organizzazione = organizationId.Value
            };

            _context.ValidazioneTassonomica.Add(status);
            await _context.SaveChangesAsync();
            return status.id;
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
    }
}


