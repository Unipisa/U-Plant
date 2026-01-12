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
    public class AlberiController : BaseController
    {
        private readonly Entities _context;
        private readonly LanguageService _languageService;
        public AlberiController(Entities context, LanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        // GET: Alberi
        public async Task<IActionResult> Index()
        {
            var entities = _context.Alberi.Include(a => a.fornitoreNavigation).Include(a => a.individuoNavigation).Include(a => a.interventoNavigation).Include(a => a.prioritaNavigation).Include(a => a.utenteaperturaNavigation).Include(a => a.utenteultimamodificaNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: Alberi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi
                .Include(a => a.fornitoreNavigation)
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
        public IActionResult Create()
        {
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione");
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo");
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione");
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione");
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF");
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF");
            return View();
        }

        // POST: Alberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,datachiusura,utenteapertura,utenteultimamodifica")] Alberi alberi)
        {
            if (ModelState.IsValid)
            {
                alberi.id = Guid.NewGuid();
                _context.Add(alberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // GET: Alberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi.FindAsync(id);
            if (alberi == null)
            {
                return NotFound();
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // POST: Alberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,datachiusura,utenteapertura,utenteultimamodifica")] Alberi alberi)
        {
            if (id != alberi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alberi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlberiExists(alberi.id))
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
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // GET: Alberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi
                .Include(a => a.fornitoreNavigation)
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

        // POST: Alberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var alberi = await _context.Alberi.FindAsync(id);
            if (alberi != null)
            {
                _context.Alberi.Remove(alberi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlberiExists(Guid id)
        {
            return _context.Alberi.Any(e => e.id == id);
        }
        public JsonResult AutoComplete()
        {
            //  var allowedStatus = new[] { "30e70f7c13774994ac9215b3543ebd7b", "3d91514fecb3473783eda3d3f8a63457", "429773f8ba564e2b87a0b775935c3ff7" }; //Vivo e incerto, malato
            var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e" }; //Nursery e Banca Semi
            string term = HttpContext.Request.Query["term"].ToString();

            IEnumerable<StoricoIndividuo> prog =
                _context.StoricoIndividuo
                .Include(x => x.individuoNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                .Include(x => x.statoIndividuoNavigation)
                .AsEnumerable()
                .OrderByDescending(c => c.individuoNavigation.propagatoData)
                .GroupBy(c => c.individuo)
                        .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                        .Where(x => x.individuoNavigation.progressivo.StartsWith(term))
           .ToList();

            var result = prog.Take(10).Select(x => x.individuoNavigation.progressivo);



          
            return Json(result, new System.Text.Json.JsonSerializerOptions());
        }


        public ActionResult InserisciIndividuoAlberi(Guid individuo)
        {


            var indicerca = _context.Alberi.Where(x => x.individuo == individuo);
            if (indicerca.Count() == 0)
            {
                Alberi indiper = new Alberi();

                
                indiper.individuo = individuo;

                if (ModelState.IsValid)
                {


                    _context.Alberi.Add(indiper);
                    _context.SaveChanges();
                    AddPageAlerts(PageAlertType.Success, _languageService.Getkey("Message_5").ToString());
                    TempData["message"] = _languageService.Getkey("Message_5").ToString();

                    return RedirectToAction("Index", "Percorsi");

                }
            }
            AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_2").ToString());
            TempData["message"] = _languageService.Getkey("Message_2").ToString();


            return RedirectToAction("Index", "Percorsi");
        }
        public JsonResult Ricerca(string progressivo)
        {

            return Json(_context.Individui.Where(a => a.progressivo.Contains(progressivo)).OrderByDescending(c => c.progressivo).Select(x => new
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






    }
}
