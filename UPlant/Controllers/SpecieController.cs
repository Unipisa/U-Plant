using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;
using UPlant.Models.ViewModels;
using UPlant.Services;

namespace UPlant.Controllers
{
    public class SpecieController : BaseController
    {
        private readonly Entities _context;
        private readonly IWorldFloraOnlineService _worldFloraOnlineService;

        public SpecieController(Entities context, IWorldFloraOnlineService worldFloraOnlineService)
        {
            _context = context;
            _worldFloraOnlineService = worldFloraOnlineService;
        }

        public async Task<IActionResult> Index()
        {
            var entities = _context.Specie
                .Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .Include(s => s.genereNavigation.famigliaNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation)
                .Include(s => s.status_nomenclaturaleNavigation)
                .Include(s => s.Accessioni)
                .OrderBy(x => x.nome_scientifico);

            return View(await entities.ToListAsync());
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
                .Include(s => s.status_nomenclaturaleNavigation)
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
        public async Task<IActionResult> Create([Bind("id,genere,status_nomenclaturale,nome,nome_scientifico,data,lsid,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
        {
            if (ModelState.IsValid)
            {
                specie.id = Guid.NewGuid();
                specie.status_nomenclaturale = await EnsureDefaultStatusAsync("Non definito");
                specie.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                specie.data = DateTime.Now;
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
        public async Task<IActionResult> Edit(Guid id, [Bind("id,genere,status_nomenclaturale,nome,nome_scientifico,data,lsid,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
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
                existing.status_nomenclaturale = specie.status_nomenclaturale;
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
                existing.status_nomenclaturale = await EnsureDefaultStatusAsync("Modificato");
                existing.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                existing.data = DateTime.Now;

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
                .Include(s => s.status_nomenclaturaleNavigation)
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

            var result = await _worldFloraOnlineService.CheckAsync(specie, specie.genereNavigation?.descrizione ?? string.Empty, cancellationToken);
            var reviewUrl = Url.Action(nameof(ReviewWfo), new { id });

            return Json(new
            {
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

        [HttpGet]
        public async Task<IActionResult> ReviewWfo(Guid id, CancellationToken cancellationToken)
        {
            var specie = await _context.Specie
                .Include(s => s.genereNavigation)
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
                CheckResult = checkResult
            };

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

            var wfoStatusId = await EnsureDefaultStatusAsync("WFO");

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
                    data = DateTime.Now,
                    status_nomenclaturale = wfoStatusId
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

        [HttpPost]
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

            specie.status_nomenclaturale = await EnsureDefaultStatusAsync("WFO");
            specie.data = DateTime.Now;

            if (string.Equals(input.ActionType, "keep_current", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(input.Lsid))
                {
                    specie.lsid = input.Lsid.Trim();
                }

                specie.nome_scientifico = await ComposeScientificNameAsync(specie.genere, specie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (!await ApplyAcceptedNameAsync(specie, input.AcceptedFullName, input.Lsid))
            {
                TempData["WfoError"] = "Il nome proposto da WFO usa un genere non presente in archivio. Inserisci prima il genere oppure correggi manualmente la specie.";
                return RedirectToAction(nameof(ReviewWfo), new { id = specie.id });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpecieExists(Guid id)
        {
            return _context.Specie.Any(e => e.id == id);
        }

        private async Task PopulateSelectionsAsync(Specie specie = null)
        {
            var organizationId = await GetCurrentUserOrganizationAsync();
            var statusQuery = _context.StatusNomenclaturale
                .Include(x => x.organizzazioneNavigation)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                statusQuery = statusQuery.Where(x => x.organizzazione == organizationId.Value);
            }

            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(x => x.descrizione), "id", "descrizione", specie?.areale);
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(x => x.ordinamento), "id", "codice", specie?.cites);
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(x => x.descrizione), "id", "descrizione", specie?.genere);
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice", specie?.iucn_globale);
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice", specie?.iucn_italia);
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(x => x.ordinamento), "id", "descrizione", specie?.regno);
            ViewData["status_nomenclaturale"] = new SelectList(await statusQuery.OrderBy(x => x.ordinamento).ToListAsync(), "id", "descrizione", specie?.status_nomenclaturale);
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

        private async Task<Guid> EnsureDefaultStatusAsync(string statusName)
        {
            var organizationId = await GetCurrentUserOrganizationAsync();
            if (!organizationId.HasValue)
            {
                return Guid.Empty;
            }

            var existingId = await _context.StatusNomenclaturale
                .Where(x => x.organizzazione == organizationId.Value &&
                            (x.descrizione == statusName || x.descrizione_en == statusName))
                .Select(x => x.id)
                .FirstOrDefaultAsync();

            if (existingId != Guid.Empty)
            {
                return existingId;
            }

            var nextOrder = (await _context.StatusNomenclaturale
                .Where(x => x.organizzazione == organizationId.Value)
                .MaxAsync(x => (int?)x.ordinamento) ?? 0) + 1;

            var status = new StatusNomenclaturale
            {
                id = Guid.NewGuid(),
                descrizione = statusName,
                descrizione_en = statusName,
                ordinamento = nextOrder,
                organizzazione = organizationId.Value
            };

            _context.StatusNomenclaturale.Add(status);
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
    }
}
