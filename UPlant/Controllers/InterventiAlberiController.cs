using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPlant.Models;
using UPlant.Models.DB;


namespace UPlant.Controllers
{
    [Authorize(Roles = "Administrator,Tree")]
    public class InterventiAlberiController : BaseController
    {
        private readonly Entities _context;
        private readonly LanguageService _languageService;
        public InterventiAlberiController(Entities context, LanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        // GET: Alberi
        public async Task<IActionResult> Index()
        {
            var flat = await _context.InterventiAlberi
     .Select(a => new
     {
         a.id,                      // <-- PK Alberi (cambia se diverso)
         a.individuo,
         a.stato,                   // false = aperto
         a.dataultimamodifica,
         livello = a.prioritaNavigation.livello  // 0..5 dalla tabella TipoPrioritaAlberi
     })
     .ToListAsync();

            // Raggruppo e scelgo la riga migliore per ogni individuo
            var bestPerIndividuo = flat
                .GroupBy(x => x.individuo)
                .Select(g =>
                {
                    var hasOpen = g.Any(x => x.stato == false);

                    var best = g
                        .OrderByDescending(x => x.stato == false) // aperti prima
                        .ThenByDescending(x => x.livello)                   // 0 più alta
                        .ThenByDescending(x => x.dataultimamodifica)
                        .First();

                    return new
                    {
                        Individuo = g.Key,
                        HasOpen = hasOpen,
                        BestId = best.id,
                        BestLivello = best.livello,
                        BestData = best.dataultimamodifica
                    };
                })
                .OrderByDescending(x => x.HasOpen)     // individui con aperti prima
                .ThenByDescending(x => x.BestLivello)            // livello 0 prima
                .ThenByDescending(x => x.BestData)     // ultima modifica più recente
                .ToList();

            var bestIds = bestPerIndividuo.Select(x => x.BestId).ToList();


            // PASSO 2: carico SOLO le righe "best" con tutte le Include che servono
            var entities = await _context.InterventiAlberi
                .Where(a => bestIds.Contains(a.id))    // <-- PK Alberi (cambia se diverso)
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.statoIndividuoNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.condizioneNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.accessioneNavigation).ThenInclude(s => s.specieNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.collezioneNavigation).ThenInclude(c => c.settoreNavigation)
                .ToListAsync();

            // Contains() non preserva l'ordine: riordino come bestPerIndividuo
            var order = bestPerIndividuo
                .Select((x, idx) => new { x.BestId, idx })
                .ToDictionary(x => x.BestId, x => x.idx);

            entities = entities.OrderBy(e => order[e.id]).ToList(); // <-- PK Alberi (cambia se diverso)

            return View(entities);
        }

        // GET: Alberi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.statoIndividuoNavigation)
                .Include(a => a.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(s => s.condizioneNavigation)
                .Include(a => a.individuoNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (alberi == null)
            {
                return NotFound();
            }

            return View(alberi);
        }

        // GET: Alberi/Create
        public IActionResult Create(Guid idindividuo)
        {
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
            //var individuo = _context.Individui.Where(x => x.id == idindividuo).FirstOrDefault();
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione");
            ViewData["individuo"] = individuo.id;

            ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", selectedStato);
            ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", selectedCondizione);
            ViewData["progressivo"] = individuo.progressivo;
            ViewData["nomescientifico"] = individuo.accessioneNavigation.specieNavigation.nome_scientifico;
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
            ViewData["utenteapertura"] = utente;
            ViewData["utenteultimamodifica"] = utente;
            return View();
        }

        // POST: Alberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,dataultimamodifica,utenteapertura,utenteultimamodifica,statoIndividuo,condizione")] InterventiAlberi interventiAlberi)
        {
            if (ModelState.IsValid)
            {
                interventiAlberi.id = Guid.NewGuid();
                interventiAlberi.dataultimamodifica = DateTime.Now;
                interventiAlberi.stato = false; // aperto di default
                _context.Add(interventiAlberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var selectedStato = interventiAlberi.statoIndividuo;
            var selectedCondizione = interventiAlberi.condizione;
            ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.stato : x.descrizione_en }), "id", "Desc", selectedStato);
            ViewData["condizione"] = new SelectList(_context.Condizioni.Select(x => new { x.id, Desc = string.IsNullOrEmpty(x.descrizione_en) ? x.condizione : x.descrizione_en }), "id", "Desc", selectedCondizione);

            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", interventiAlberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", interventiAlberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", interventiAlberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", interventiAlberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteultimamodifica);
            return View(interventiAlberi);
        }

        // GET: Alberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interventiAlberi = await _context.InterventiAlberi.FindAsync(id);
            if (interventiAlberi == null)
            {
                return NotFound();
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", interventiAlberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", interventiAlberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", interventiAlberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", interventiAlberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteultimamodifica);
            return View(interventiAlberi);
        }

        // POST: Alberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,dataultimamodifica,utenteapertura,utenteultimamodifica")] InterventiAlberi interventiAlberi)
        {
            if (id != interventiAlberi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    interventiAlberi.dataultimamodifica = DateTime.Now;
                    _context.Update(interventiAlberi);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", interventiAlberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", interventiAlberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", interventiAlberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", interventiAlberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", interventiAlberi.utenteultimamodifica);
            return View(interventiAlberi);
        }

        // GET: Alberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interventiAlberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation)
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

        // POST: Alberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var interventiAlberi = await _context.InterventiAlberi.FindAsync(id);
            if (interventiAlberi != null)
            {
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
            
            return Json(_context.Individui.Include(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation).Where(x => x.accessioneNavigation.specieNavigation.nome_scientifico.ToLower().Contains(nome_scientifico.ToLower())).Select(x => new
            {
                idindividuo = x.id,
                progressivo = x.progressivo,
                vecchioprogressivo = x.vecchioprogressivo,
                nomescientifico = x.accessioneNavigation.specieNavigation.nome_scientifico,
                settore = x.settoreNavigation.settore,
                collezione = x.collezioneNavigation.collezione,
                cartellino = x.cartellinoNavigation.descrizione,
                immagini = x.ImmaginiIndividuo.Count
            }).ToList(), new System.Text.Json.JsonSerializerOptions());


        }

        public async Task<IActionResult> ElencoInterventi(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interventiAlberi = await _context.InterventiAlberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation).Where(m => m.individuo == id)
                .ToListAsync();
            if (interventiAlberi == null)
            {
                return NotFound();
            }

            return View(interventiAlberi);
        }

    }
}
