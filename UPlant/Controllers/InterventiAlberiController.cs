using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UPlant.Models;
using UPlant.Models.DB;


namespace UPlant.Controllers
{
    [Authorize(Roles = "Administrator,Operator")]
    public class InterventiAlberiController : BaseController
    {
        private readonly Entities _context;
        private readonly LanguageService _languageService;
        public InterventiAlberiController(Entities context, LanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        private bool CanManageTreeInterventions()
        {
            return User.IsInRole("Administrator") || User.IsInRole("TreeManager");
        }

        private bool CanManageClosedTreeInterventions()
        {
            return User.IsInRole("Administrator");
        }

        // GET: Alberi
        public async Task<IActionResult> Index()
        {
            var flat = await _context.InterventiAlberi
                .Select(a => new
                {
                    a.id,
                    a.individuo,
                    a.statoIntervento, // bool -> false = aperto
                    a.dataultimamodifica,
                    livello = (int?)a.prioritaNavigation.livello ?? 0
                })
                .ToListAsync();

            // Seleziono la riga migliore per ogni individuo
            var bestPerIndividuo = flat
                .GroupBy(x => x.individuo)
                .Select(g =>
                {
                    var best = g
                        .OrderByDescending(x => !x.statoIntervento) // ✅ aperti prima
                        .ThenByDescending(x => x.livello)           // ✅ priorità 4->0
                        .ThenByDescending(x => x.dataultimamodifica)// ✅ data più recente
                        .First();

                    return new
                    {
                        Individuo = g.Key,
                        BestId = best.id,
                        BestIsOpen = !best.statoIntervento,
                        BestLivello = best.livello,
                        BestData = best.dataultimamodifica
                    };
                })
                // Ordine finale elenco individui
                .OrderByDescending(x => x.BestIsOpen)   // ✅ prima tutti gli aperti
                .ThenByDescending(x => x.BestLivello)   // ✅ poi priorità
                .ThenByDescending(x => x.BestData)      // ✅ poi data
                .ToList();

            var bestIds = bestPerIndividuo.Select(x => x.BestId).ToList();

            // Carico solo le entità selezionate
            var entities = await _context.InterventiAlberi
                .Where(a => bestIds.Contains(a.id))
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.StoricoIndividuo)
                        .ThenInclude(s => s.statoIndividuoNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.StoricoIndividuo)
                        .ThenInclude(s => s.condizioneNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.accessioneNavigation)
                        .ThenInclude(s => s.specieNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.collezioneNavigation)
                        .ThenInclude(c => c.settoreNavigation)
                .ToListAsync();

            // Riordino entities secondo l’ordine calcolato sopra
            var order = bestPerIndividuo
                .Select((x, idx) => new { x.BestId, idx })
                .ToDictionary(x => x.BestId, x => x.idx);

            entities = entities.OrderBy(e => order[e.id]).ToList();

            return View(entities);
        }

        [HttpGet]
        public IActionResult RicercaTabella()
        {
            PopulateRicercaTabellaViewData();
            return View(new List<InterventiAlberi>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RicercaTabella(string nomeScientifico, string progressivo, string destinazione, Guid? settore,
            Guid? collezione, DateTime? dataUltimaModificaDa, DateTime? dataUltimaModificaA, string statoIntervento,
            Guid? priorita, Guid? tipoIntervento, Guid? fornitore)
        {
            var query = BuildRicercaTabellaQuery(nomeScientifico, progressivo, destinazione, settore,
                collezione, dataUltimaModificaDa, dataUltimaModificaA, statoIntervento, priorita, tipoIntervento, fornitore);

            var risultati = await query
                .OrderByDescending(a => a.dataultimamodifica ?? a.dataapertura)
                .ToListAsync();

            PopulateRicercaTabellaViewData();
            ViewBag.nomeScientifico = nomeScientifico;
            ViewBag.progressivo = progressivo;
            ViewBag.destinazione = destinazione;
            ViewBag.settore = settore;
            ViewBag.collezione = collezione;
            ViewBag.dataUltimaModificaDa = dataUltimaModificaDa;
            ViewBag.dataUltimaModificaA = dataUltimaModificaA;
            ViewBag.statoIntervento = statoIntervento;
            ViewBag.priorita = priorita;
            ViewBag.tipoIntervento = tipoIntervento;
            ViewBag.fornitore = fornitore;

            return View(risultati);
        }

        [HttpGet]
        public async Task<IActionResult> ExportRicercaTabella(string nomeScientifico, string progressivo, string destinazione, Guid? settore,
            Guid? collezione, DateTime? dataUltimaModificaDa, DateTime? dataUltimaModificaA, string statoIntervento,
            Guid? priorita, Guid? tipoIntervento, Guid? fornitore)
        {
            var linguaCorrente = _languageService.GetCurrentCulture();
            var interventi = await BuildRicercaTabellaQuery(nomeScientifico, progressivo, destinazione, settore,
                collezione, dataUltimaModificaDa, dataUltimaModificaA, statoIntervento, priorita, tipoIntervento, fornitore)
                .OrderByDescending(a => a.dataultimamodifica ?? a.dataapertura)
                .ToListAsync();

            var headers = new[]
            {
                "Progressivo",
                "Nome scientifico",
                "Destinazione",
                "Settore",
                "Collezione",
                "Stato",
                "Priorità",
                "Tipo intervento",
                "Fornitore",
                "Data ultima modifica"
            };

            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Ricerca Interventi"
                });

                sheetData.Append(CreateTextRow(headers));

                foreach (var intervento in interventi)
                {
                    var prioritaDescrizione = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.prioritaNavigation?.descrizione_en)
                            ? intervento.prioritaNavigation?.descrizione
                            : intervento.prioritaNavigation?.descrizione_en)
                        : intervento.prioritaNavigation?.descrizione;

                    var tipoInterventoDescrizione = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.interventoNavigation?.descrizione_en)
                            ? intervento.interventoNavigation?.descrizione
                            : intervento.interventoNavigation?.descrizione_en)
                        : intervento.interventoNavigation?.descrizione;

                    var fornitoreDescrizione = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.fornitoreNavigation?.descrizione_en)
                            ? intervento.fornitoreNavigation?.descrizione
                            : intervento.fornitoreNavigation?.descrizione_en)
                        : intervento.fornitoreNavigation?.descrizione;

                    sheetData.Append(CreateTextRow(new[]
                    {
                        intervento.individuoNavigation?.progressivo,
                        intervento.individuoNavigation?.accessioneNavigation?.specieNavigation?.nome_scientifico,
                        intervento.individuoNavigation?.destinazioni,
                        intervento.individuoNavigation?.collezioneNavigation?.settoreNavigation?.settore,
                        intervento.individuoNavigation?.collezioneNavigation?.collezione,
                        intervento.statoIntervento ? _languageService.Getkey("Global_Close") : _languageService.Getkey("Global_Open"),
                        prioritaDescrizione,
                        tipoInterventoDescrizione,
                        fornitoreDescrizione,
                        (intervento.dataultimamodifica ?? intervento.dataapertura).ToString("dd/MM/yyyy")
                    }));
                }

                workbookPart.Workbook.Save();
            }

            var fileName = $"RicercaInterventiAlberi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private IQueryable<InterventiAlberi> BuildRicercaTabellaQuery(string nomeScientifico, string progressivo, string destinazione, Guid? settore,
            Guid? collezione, DateTime? dataUltimaModificaDa, DateTime? dataUltimaModificaA, string statoIntervento,
            Guid? priorita, Guid? tipoIntervento, Guid? fornitore)
        {
            var query = _context.InterventiAlberi
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.accessioneNavigation)
                        .ThenInclude(a => a.specieNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.collezioneNavigation)
                        .ThenInclude(c => c.settoreNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.fornitoreNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nomeScientifico))
            {
                var filtro = nomeScientifico.Trim().ToLower();
                query = query.Where(a => (a.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico ?? "").ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(progressivo))
            {
                var filtro = progressivo.Trim();
                query = query.Where(a => (a.individuoNavigation.progressivo ?? "").Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(destinazione))
            {
                var filtro = destinazione.Trim().ToLower();
                query = query.Where(a => (a.individuoNavigation.destinazioni ?? "").ToLower().Contains(filtro));
            }

            if (settore.HasValue)
            {
                query = query.Where(a => a.individuoNavigation.settore == settore.Value);
            }

            if (collezione.HasValue)
            {
                query = query.Where(a => a.individuoNavigation.collezione == collezione.Value);
            }

            if (dataUltimaModificaDa.HasValue)
            {
                var da = dataUltimaModificaDa.Value.Date;
                query = query.Where(a => (a.dataultimamodifica ?? a.dataapertura) >= da);
            }

            if (dataUltimaModificaA.HasValue)
            {
                var a = dataUltimaModificaA.Value.Date.AddDays(1);
                query = query.Where(x => (x.dataultimamodifica ?? x.dataapertura) < a);
            }

            if (!string.IsNullOrWhiteSpace(statoIntervento))
            {
                if (statoIntervento == "open")
                {
                    query = query.Where(a => !a.statoIntervento);
                }
                else if (statoIntervento == "close")
                {
                    query = query.Where(a => a.statoIntervento);
                }
            }

            if (priorita.HasValue)
            {
                query = query.Where(a => a.priorita == priorita.Value);
            }

            if (tipoIntervento.HasValue)
            {
                query = query.Where(a => a.intervento == tipoIntervento.Value);
            }

            if (fornitore.HasValue)
            {
                query = query.Where(a => a.fornitore == fornitore.Value);
            }

            return query;
        }

        private void PopulateRicercaTabellaViewData()
        {
            var linguacorrente = _languageService.GetCurrentCulture();

            ViewBag.listaDestinazioni = _context.Individui
                .Where(i => !string.IsNullOrWhiteSpace(i.destinazioni))
                .Select(i => i.destinazioni)
                .Distinct()
                .OrderBy(i => i)
                .Select(i => new SelectListItem { Value = i, Text = i })
                .ToList();

            if (linguacorrente == "en-US")
            {
                ViewBag.listasettori = new SelectList(_context.Settori.OrderBy(a => a.ordinamento)
                    .Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.settore_en) ? a.settore : a.settore_en }), "id", "Desc").ToList();

                ViewBag.listacollezioni = new SelectList(_context.Collezioni.OrderBy(a => a.collezione)
                    .Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.collezione_en) ? a.collezione : a.collezione_en }), "id", "Desc").ToList();

                ViewBag.listapriorita = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento)
                    .Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();

                ViewBag.listatipointervento = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento)
                    .Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();

                ViewBag.listafornitori = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione)
                    .Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
            }
            else
            {
                ViewBag.listasettori = new SelectList(_context.Settori.OrderBy(a => a.settore), "id", "settore").ToList();
                ViewBag.listacollezioni = new SelectList(_context.Collezioni.OrderBy(a => a.collezione), "id", "collezione").ToList();
                ViewBag.listapriorita = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione").ToList();
                ViewBag.listatipointervento = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione").ToList();
                ViewBag.listafornitori = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
            }

            ViewBag.nomeScientifico ??= string.Empty;
            ViewBag.progressivo ??= string.Empty;
            ViewBag.destinazione ??= string.Empty;
            ViewBag.dataUltimaModificaDa ??= DateTime.Today.AddMonths(-1);
            ViewBag.dataUltimaModificaA ??= DateTime.Today;
            ViewBag.statoIntervento ??= string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> ExportTotaleInterventi()
        {
            var linguaCorrente = _languageService.GetCurrentCulture();

            var interventi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.statoIndividuoNavigation)
                .Include(a => a.condizioneNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.accessioneNavigation)
                        .ThenInclude(s => s.specieNavigation)
                .Include(a => a.individuoNavigation)
                    .ThenInclude(i => i.collezioneNavigation)
                        .ThenInclude(c => c.settoreNavigation)
                .OrderByDescending(a => a.dataapertura)
                .ToListAsync();

            var headers = new[]
            {
                "Progressivo",
                "Nome scientifico",
                "Settore",
                "Collezione",
                "Data apertura",
                "Data ultima modifica",
                "Stato intervento",
                "Priorità",
                "Tipo intervento",
                "Fornitore",
                "Motivo",
                "Esito intervento",
                "Stato individuo",
                "Condizione",
                "Utente apertura",
                "Utente ultima modifica"
            };

            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Interventi Alberi"
                });

                sheetData.Append(CreateTextRow(headers));

                foreach (var intervento in interventi)
                {
                    var priorita = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.prioritaNavigation?.descrizione_en)
                            ? intervento.prioritaNavigation?.descrizione
                            : intervento.prioritaNavigation?.descrizione_en)
                        : intervento.prioritaNavigation?.descrizione;

                    var tipoIntervento = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.interventoNavigation?.descrizione_en)
                            ? intervento.interventoNavigation?.descrizione
                            : intervento.interventoNavigation?.descrizione_en)
                        : intervento.interventoNavigation?.descrizione;

                    var fornitore = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.fornitoreNavigation?.descrizione_en)
                            ? intervento.fornitoreNavigation?.descrizione
                            : intervento.fornitoreNavigation?.descrizione_en)
                        : intervento.fornitoreNavigation?.descrizione;

                    var statoIndividuo = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.statoIndividuoNavigation?.descrizione_en)
                            ? intervento.statoIndividuoNavigation?.stato
                            : intervento.statoIndividuoNavigation?.descrizione_en)
                        : intervento.statoIndividuoNavigation?.stato;

                    var condizione = linguaCorrente == "en-US"
                        ? (string.IsNullOrWhiteSpace(intervento.condizioneNavigation?.descrizione_en)
                            ? intervento.condizioneNavigation?.condizione
                            : intervento.condizioneNavigation?.descrizione_en)
                        : intervento.condizioneNavigation?.condizione;

                    var utenteApertura = $"{intervento.utenteaperturaNavigation?.Name} {intervento.utenteaperturaNavigation?.LastName}".Trim();
                    var utenteUltimaModifica = $"{intervento.utenteultimamodificaNavigation?.Name} {intervento.utenteultimamodificaNavigation?.LastName}".Trim();

                    sheetData.Append(CreateTextRow(new[]
                    {
                        intervento.individuoNavigation?.progressivo,
                        intervento.individuoNavigation?.accessioneNavigation?.specieNavigation?.nome_scientifico,
                        intervento.individuoNavigation?.collezioneNavigation?.settoreNavigation?.settore,
                        intervento.individuoNavigation?.collezioneNavigation?.collezione,
                        intervento.dataapertura.ToString("dd/MM/yyyy HH:mm"),
                        intervento.dataultimamodifica?.ToString("dd/MM/yyyy HH:mm"),
                        intervento.statoIntervento ? _languageService.Getkey("Global_Close") : _languageService.Getkey("Global_Open"),
                        priorita,
                        tipoIntervento,
                        fornitore,
                        intervento.motivo,
                        intervento.esitointervento,
                        statoIndividuo,
                        condizione,
                        utenteApertura,
                        utenteUltimaModifica
                    }));
                }

                workbookPart.Workbook.Save();
            }

            var fileName = $"InterventiAlberi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private static Row CreateTextRow(IEnumerable<string> values)
        {
            var row = new Row();

            foreach (var value in values)
            {
                row.Append(new Cell
                {
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(value ?? string.Empty))
                });
            }

            return row;
        }

        // GET: Alberi/Details/5
        public async Task<IActionResult> Details(Guid? id, string source = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.statoIndividuoNavigation)
                .Include(a => a.condizioneNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.statoIndividuoNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.condizioneNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.accessioneNavigation).ThenInclude(a => a.specieNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (alberi == null)
            {
                return NotFound();
            }

            ViewData["progressivo"] = alberi.individuoNavigation?.progressivo;
            ViewData["nomescientifico"] = alberi.individuoNavigation?.accessioneNavigation?.specieNavigation?.nome_scientifico;
            ViewBag.source = source;
            ViewBag.idindividuo = alberi.individuo;
            return View(alberi);
        }
        [Authorize(Roles = "Administrator,Operator")]
        // GET: Alberi/Create
        public IActionResult Create(Guid idindividuo)
        {
            if (!CanManageTreeInterventions())
            {
                return Forbid();
            }

            var linguacorrente = _languageService.GetCurrentCulture();
            var individuo = _context.Individui
                  .Include(i => i.StoricoIndividuo).ThenInclude(s => s.statoIndividuoNavigation)
                .Include(i => i.StoricoIndividuo).ThenInclude(s => s.condizioneNavigation)
               .Include(s => s.accessioneNavigation)
               .ThenInclude(x => x.specieNavigation)
               .FirstOrDefault(x => x.id == idindividuo);


            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));


            Guid organizzazione = oggettoutente.Select(a => a.Organizzazione).FirstOrDefault();
            Guid utente = oggettoutente.Select(a => a.Id).FirstOrDefault();
            var lastStorico = individuo.StoricoIndividuo.OrderByDescending(s => s.dataInserimento).FirstOrDefault();
            var selectedStato = lastStorico?.statoIndividuo;
            var selectedCondizione = lastStorico?.condizione;
            ViewData["progressivo"] = individuo.progressivo;
            ViewData["utenteapertura"] = utente;
            ViewData["utenteultimamodifica"] = utente;
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
           
            ViewData["individuo"] = individuo.id;
            ViewData["nomescientifico"] = individuo.accessioneNavigation.specieNavigation.nome_scientifico;
            if (linguacorrente == "en-US")
            {


                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", selectedStato);
                ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", selectedCondizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
            }

            else
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderByDescending(a => a.ordinamento), "id", "stato", selectedStato);
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderByDescending(a => a.ordinamento), "id", "condizione", selectedCondizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione");
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione");
            }
            return View();
        }

        // POST: Alberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator,Operator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,statoIntervento,dataultimamodifica,utenteapertura,utenteultimamodifica")] InterventiAlberi interventiAlberi)
        {
            if (!CanManageTreeInterventions())
            {
                return Forbid();
            }

            var linguacorrente = _languageService.GetCurrentCulture();
            if (ModelState.IsValid)
            {
                var lastStorico = await _context.StoricoIndividuo
                    .Where(x => x.individuo == interventiAlberi.individuo)
                    .OrderByDescending(x => x.dataInserimento)
                    .FirstOrDefaultAsync();

                if (lastStorico != null)
                {
                    interventiAlberi.statoIndividuo = lastStorico.statoIndividuo;
                    interventiAlberi.condizione = lastStorico.condizione;
                }

                interventiAlberi.dataapertura = DateTime.Now;
                interventiAlberi.id = Guid.NewGuid();
                interventiAlberi.dataultimamodifica = DateTime.Now;
                interventiAlberi.statoIntervento = false; // aperto di default
                _context.Add(interventiAlberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ElencoInterventi), new { id = interventiAlberi.individuo });
            }
            
            var selectedStato = interventiAlberi.statoIndividuo;
            var selectedCondizione = interventiAlberi.condizione;
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.fornitore);
            
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", interventiAlberi.individuo);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteultimamodifica);

            if (linguacorrente == "en-US")
            {


                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", selectedStato);
                ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", selectedCondizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.priorita);
            }

            else
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderByDescending(a => a.ordinamento), "id", "stato", selectedStato);
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderByDescending(a => a.ordinamento), "id", "condizione", selectedCondizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.priorita);
            }


            
            return View(interventiAlberi);
        }
        [Authorize(Roles = "Administrator,Operator")]
        // GET: Alberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id, string source = null)
        {
            if (!CanManageTreeInterventions())
            {
                return Forbid();
            }

            var linguacorrente = _languageService.GetCurrentCulture();
            if (id == null)
            {
                return NotFound();
            }

            var interventiAlberi = await _context.InterventiAlberi
                .Include(x => x.individuoNavigation)
                .ThenInclude(x => x.accessioneNavigation)
                .ThenInclude(x => x.specieNavigation)
                .FirstOrDefaultAsync(x => x.id == id);
            if (interventiAlberi == null)
            {
                return NotFound();
            }

            if (interventiAlberi.statoIntervento && !CanManageClosedTreeInterventions())
            {
                return Forbid();
            }

            ViewData["progressivo"] = interventiAlberi.individuoNavigation?.progressivo;
            ViewData["nomescientifico"] = interventiAlberi.individuoNavigation?.accessioneNavigation?.specieNavigation?.nome_scientifico;
            ViewBag.source = source;
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.fornitore);


            if (linguacorrente == "en-US")
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", interventiAlberi.statoIndividuo);
                ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", interventiAlberi.condizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.priorita);
            }
            else
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderByDescending(a => a.ordinamento), "id", "stato", interventiAlberi.statoIndividuo);
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderByDescending(a => a.ordinamento), "id", "condizione", interventiAlberi.condizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.priorita);
            }

            return View(interventiAlberi);
        }

        // POST: Alberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator,Operator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,statoIntervento,dataultimamodifica,utenteapertura,utenteultimamodifica,statoIndividuo,condizione")] InterventiAlberi interventiAlberi, string source = null)
        {
            if (!CanManageTreeInterventions())
            {
                return Forbid();
            }

            var linguacorrente = _languageService.GetCurrentCulture();
            if (id != interventiAlberi.id)
            {
                return NotFound();
            }

            if (interventiAlberi.statoIntervento && string.IsNullOrWhiteSpace(interventiAlberi.esitointervento))
            {
                ModelState.AddModelError(nameof(interventiAlberi.esitointervento), "L'esito intervento è obbligatorio quando lo stato è Chiuso.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingIntervento = await _context.InterventiAlberi
                        .FirstOrDefaultAsync(x => x.id == id);

                    if (existingIntervento == null)
                    {
                        return NotFound();
                    }

                    var utenteCorrente = GetCurrentUserId();
                    var chiusuraIntervento = existingIntervento.statoIntervento == false && interventiAlberi.statoIntervento == true;
                    var riaperturaIntervento = existingIntervento.statoIntervento == true && interventiAlberi.statoIntervento == false;

                    existingIntervento.priorita = interventiAlberi.priorita;
                    existingIntervento.intervento = interventiAlberi.intervento;
                    existingIntervento.fornitore = interventiAlberi.fornitore;
                    existingIntervento.motivo = interventiAlberi.motivo;
                    existingIntervento.esitointervento = interventiAlberi.esitointervento;
                    existingIntervento.statoIntervento = interventiAlberi.statoIntervento;
                    existingIntervento.dataultimamodifica = DateTime.Now;
                    if (utenteCorrente != Guid.Empty)
                    {
                        existingIntervento.utenteultimamodifica = utenteCorrente;
                    }

                    if (chiusuraIntervento || existingIntervento.statoIntervento)
                    {
                        existingIntervento.statoIndividuo = interventiAlberi.statoIndividuo;
                        existingIntervento.condizione = interventiAlberi.condizione;
                    }

                    var utenteIdStorico = utenteCorrente != Guid.Empty
                        ? utenteCorrente
                        : (existingIntervento.utenteultimamodifica != Guid.Empty
                            ? existingIntervento.utenteultimamodifica
                            : existingIntervento.utenteapertura);

                    if (chiusuraIntervento)
                    {
                        var storico = new StoricoIndividuo
                        {
                            id = Guid.NewGuid(),
                            individuo = existingIntervento.individuo,
                            dataInserimento = DateTime.Now,
                            statoIndividuo = existingIntervento.statoIndividuo,
                            condizione = existingIntervento.condizione,
                            operazioniColturali = existingIntervento.esitointervento,
                            utente = utenteIdStorico
                        };

                        _context.StoricoIndividuo.Add(storico);
                        existingIntervento.storicoIndividuoId = storico.id;
                    }
                    else if (existingIntervento.statoIntervento && existingIntervento.storicoIndividuoId.HasValue)
                    {
                        var storico = await _context.StoricoIndividuo
                            .FirstOrDefaultAsync(x => x.id == existingIntervento.storicoIndividuoId.Value);

                        if (storico != null)
                        {
                            storico.statoIndividuo = existingIntervento.statoIndividuo;
                            storico.condizione = existingIntervento.condizione;
                            storico.operazioniColturali = existingIntervento.esitointervento;
                            storico.utente = utenteIdStorico;
                            storico.dataInserimento = DateTime.Now;
                        }
                    }

                    if (riaperturaIntervento)
                    {
                        if (existingIntervento.storicoIndividuoId.HasValue)
                        {
                            var storico = await _context.StoricoIndividuo
                                .FirstOrDefaultAsync(x => x.id == existingIntervento.storicoIndividuoId.Value);

                            if (storico != null)
                            {
                                _context.StoricoIndividuo.Remove(storico);
                            }
                        }

                        existingIntervento.storicoIndividuoId = null;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlberiExists(interventiAlberi.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (string.Equals(source, "ricerca", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(nameof(RicercaTabella));
                }
                return RedirectToAction(nameof(ElencoInterventi), new { id = interventiAlberi.individuo });
            }
            ViewBag.source = source;
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.fornitore);


            var individuo = await _context.Individui
                .Include(s => s.accessioneNavigation)
                .ThenInclude(x => x.specieNavigation)
                .FirstOrDefaultAsync(x => x.id == interventiAlberi.individuo);

            ViewData["progressivo"] = individuo?.progressivo;
            ViewData["nomescientifico"] = individuo?.accessioneNavigation?.specieNavigation?.nome_scientifico;

            if (linguacorrente == "en-US")
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", interventiAlberi.statoIndividuo);
                ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", interventiAlberi.condizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", interventiAlberi.priorita);
            }
            else
            {
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderByDescending(a => a.ordinamento), "id", "stato", interventiAlberi.statoIndividuo);
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderByDescending(a => a.ordinamento), "id", "condizione", interventiAlberi.condizione);
                ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.intervento);
                ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderByDescending(a => a.ordinamento), "id", "descrizione", interventiAlberi.priorita);
            }

            return View(interventiAlberi);
        }

        private Guid GetCurrentUserId()
        {
            var username = User.Identities.FirstOrDefault()?.Claims?.FirstOrDefault(c => c.Type == "UnipiUserID")?.Value;
            if (string.IsNullOrWhiteSpace(username) || !username.Contains("@"))
            {
                return Guid.Empty;
            }

            var userNameOnly = username.Substring(0, username.IndexOf("@"));
            return _context.Users.Where(u => u.UnipiUserName == userNameOnly).Select(u => u.Id).FirstOrDefault();
        }
        [Authorize(Roles = "Administrator")]
        // GET: Alberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interventiAlberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.accessioneNavigation).ThenInclude(a => a.specieNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (interventiAlberi == null)
            {
                return NotFound();
            }

            return View(interventiAlberi);
        }
        [Authorize(Roles = "Administrator")]
        // POST: Alberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var interventiAlberi = await _context.InterventiAlberi.FindAsync(id);
            if (interventiAlberi != null)
            {
                if (interventiAlberi.storicoIndividuoId.HasValue)
                {
                    var storico = await _context.StoricoIndividuo
                        .FirstOrDefaultAsync(x => x.id == interventiAlberi.storicoIndividuoId.Value);

                    if (storico != null)
                    {
                        _context.StoricoIndividuo.Remove(storico);
                    }
                }

                _context.InterventiAlberi.Remove(interventiAlberi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlberiExists(Guid id)
        {
            return _context.InterventiAlberi.Any(e => e.id == id);
        }
        public JsonResult AutoComplete()
        {
            
            string term = HttpContext.Request.Query["term"].ToString();
            /*
            IEnumerable<StoricoIndividuo> prog =
                _context.StoricoIndividuo
                .Include(x => x.individuoNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                .Include(x => x.statoIndividuoNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation)
                .AsEnumerable()
                .OrderByDescending(c => c.individuoNavigation.propagatoData)
                .GroupBy(c => c.individuo)
                        .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                        .Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico.StartsWith(term))
           .ToList();*/
            var prelist = _context.Accessioni.Where(p => p.specieNavigation.nome_scientifico.ToLower().Contains(term.ToLower())).Select(g => g.specieNavigation.nome_scientifico);
            var names = prelist.Distinct().ToList();
            //var result = prog.Take(10).Select(x => x.individuoNavigation.progressivo);



          
            return Json(names, new System.Text.Json.JsonSerializerOptions());
        }
      
        public JsonResult Ricerca(string nome_scientifico)
        {
            if (string.IsNullOrWhiteSpace(nome_scientifico) || nome_scientifico.Trim().Length < 3)
            {
                return Json(new List<object>(), new System.Text.Json.JsonSerializerOptions());
            }

            var filtro = nome_scientifico.Trim().ToLower();
            var statoMortoId = _context.StatoIndividuo
                .Where(x => (x.stato ?? string.Empty).ToLower() == "morto")
                .Select(x => x.id)
                .FirstOrDefault();

            var risultati = _context.Individui
                .Include(x => x.accessioneNavigation)
                    .ThenInclude(x => x.specieNavigation)
                .Where(x => _context.StoricoIndividuo
                    .Where(s => s.individuo == x.id)
                    .OrderByDescending(s => s.dataInserimento)
                    .Select(s => s.statoIndividuo)
                    .FirstOrDefault() != statoMortoId)
                .Where(x => (x.accessioneNavigation.specieNavigation.nome_scientifico ?? string.Empty)
                    .ToLower()
                    .Contains(filtro))
                .Select(x => new
                {
                    idindividuo = x.id,
                    progressivo = x.progressivo,
                    vecchioprogressivo = x.vecchioprogressivo,
                    nomescientifico = x.accessioneNavigation.specieNavigation.nome_scientifico,
                    settore = x.settoreNavigation.settore,
                    collezione = x.collezioneNavigation.collezione,
                    cartellino = x.cartellinoNavigation.descrizione,
                    statoindividuo = _context.StoricoIndividuo
                        .Where(s => s.individuo == x.id)
                        .OrderByDescending(s => s.dataInserimento)
                        .Select(s => s.statoIndividuoNavigation.stato)
                        .FirstOrDefault()
                })
                .ToList();

            return Json(risultati, new System.Text.Json.JsonSerializerOptions());


        }

        public async Task<IActionResult> ElencoInterventi(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individuo = await _context.Individui
                .Include(i => i.accessioneNavigation)
                .ThenInclude(a => a.specieNavigation)
                .FirstOrDefaultAsync(i => i.id == id);

            if (individuo == null)
            {
                return NotFound();
            }

            ViewBag.idindividuo = id;
            ViewData["progressivo"] = individuo.progressivo;
            ViewData["nomescientifico"] = individuo.accessioneNavigation?.specieNavigation?.nome_scientifico;

            var interventiAlberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.accessioneNavigation).ThenInclude(a => a.specieNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.statoIndividuoNavigation)
                .Include(a => a.condizioneNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .Where(m => m.individuo == id)
                .OrderByDescending(x => x.statoIntervento == false)
                .ThenByDescending(x => x.prioritaNavigation.livello)
                .ThenByDescending(x => x.dataultimamodifica ?? x.dataapertura)
                .ToListAsync();

            return View(interventiAlberi);
        }

    }
}
