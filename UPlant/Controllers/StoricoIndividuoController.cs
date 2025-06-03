using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UPlant.Models;
using UPlant.Models.DB;


namespace UPlant.Controllers
{
    public class StoricoIndividuoController : BaseController
    {
        private readonly Entities _context;
        

  



        public StoricoIndividuoController(Entities context)
        {
            _context = context;


        }

        // GET: StoricoIndividuo
        public async Task<IActionResult> Index()
        {
            var entities = _context.StoricoIndividuo.Include(s => s.condizioneNavigation).Include(s => s.individuoNavigation).Include(s => s.statoIndividuoNavigation).Include(s => s.utenteNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: StoricoIndividuo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.StoricoIndividuo == null)
            {
                return NotFound();
            }

            var storicoIndividuo = await _context.StoricoIndividuo
                .Include(s => s.condizioneNavigation)
                .Include(s => s.individuoNavigation)
                .Include(s => s.statoIndividuoNavigation)
                .Include(s => s.utenteNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (storicoIndividuo == null)
            {
                return NotFound();
            }

            return View(storicoIndividuo);
        }

        // GET: StoricoIndividuo/Create
        public IActionResult Create(Guid idindividuo,string tipo,string damodifica)
        {
        if (damodifica== "ok") { 
            var messaggio = "Hai appena fatto una modifica all'individuo vuoi aggiornare il suo stato?Altrimenti torna alla pagina dell'individuo";
            AddPageAlerts(PageAlertType.Success, messaggio);
            }
            var storicoindividuo = _context.StoricoIndividuo.Where(x => x.individuo == idindividuo).ToList().OrderByDescending(x => x.dataInserimento).FirstOrDefault();
            ViewData["individuo"] = idindividuo;
            ViewData["tipo"] = tipo;
            if (storicoindividuo == null) {
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderBy(x => x.condizione), "id", "condizione");
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderBy(x => x.stato), "id", "stato");
            }
            else
            {
                ViewData["condizione"] = new SelectList(_context.Condizioni.OrderBy(x => x.condizione), "id", "condizione", storicoindividuo.condizione);
                ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderBy(x => x.stato), "id", "stato", storicoindividuo.statoIndividuo);
             
            }
            return View();

        }

        // POST: StoricoIndividuo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("id,statoIndividuo,condizione,operazioniColturali,dataInserimento,individuo,utente")] StoricoIndividuo storicoIndividuo)
        public async Task<IActionResult> Create(Guid statoIndividuo,Guid condizione,string operazioniColturali, Guid individuo, string tipo)


        {
            StoricoIndividuo storicoIndividuo = new StoricoIndividuo();
            if (ModelState.IsValid)
            {
                string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
                Guid utente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Id).FirstOrDefault();

               
                storicoIndividuo.statoIndividuo=statoIndividuo;
                storicoIndividuo.condizione =condizione;
                storicoIndividuo.operazioniColturali =operazioniColturali;
                storicoIndividuo.individuo = individuo;
                storicoIndividuo.dataInserimento = DateTime.Now;
                storicoIndividuo.utente = utente;
                _context.Add(storicoIndividuo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details),nameof(Individui), new
                {
                    id = storicoIndividuo.individuo,
                    tipo = tipo
                });
                
           
            }
            ViewBag.tipo = tipo;
            
            ViewData["condizione"] = new SelectList(_context.Condizioni.OrderBy(x => x.condizione), "id", "condizione", storicoIndividuo.condizione);
            ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderBy(x => x.stato), "id", "stato", storicoIndividuo.statoIndividuo);
            return View(storicoIndividuo);


        }

        // GET: StoricoIndividuo/Edit/5
        public async Task<IActionResult> Edit(Guid? id,string tipo,string individuo)
        {
            if (id == null || _context.StoricoIndividuo == null)
            {
                return NotFound();
            }
            
            var storicoIndividuo = await _context.StoricoIndividuo.Where(x => x.id == id).FirstOrDefaultAsync();

            if (storicoIndividuo == null)
            {
                return NotFound();
            }
            ViewBag.tipo = tipo;
           ViewBag.individuo = storicoIndividuo.individuo;
            ViewData["condizione"] = new SelectList(_context.Condizioni.OrderBy(x => x.condizione), "id", "condizione", storicoIndividuo.condizione);
            
            ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderBy(x => x.stato), "id", "stato", storicoIndividuo.statoIndividuo);
            
            return View(storicoIndividuo);
        }

        // POST: StoricoIndividuo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,string tipo, [Bind("id,statoIndividuo,condizione,operazioniColturali,dataInserimento,individuo,utente")] StoricoIndividuo storicoIndividuo)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid utente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Id).FirstOrDefault();
            if (id != storicoIndividuo.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    storicoIndividuo.dataInserimento = DateTime.Now;
                    storicoIndividuo.utente = utente;
                    _context.Update(storicoIndividuo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoricoIndividuoExists(storicoIndividuo.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), nameof(Individui), new { id = storicoIndividuo.individuo ,tipo =tipo});
            }
            ViewBag.tipo = tipo;
            ViewData["condizione"] = new SelectList(_context.Condizioni.OrderBy(x => x.condizione), "id", "condizione", storicoIndividuo.condizione);
           // ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", storicoIndividuo.individuo);
            ViewData["statoIndividuo"] = new SelectList(_context.StatoIndividuo.OrderBy(x => x.stato), "id", "stato", storicoIndividuo.statoIndividuo);
            ViewData["utente"] = new SelectList(_context.Users, "Id", "CreatedBy", storicoIndividuo.utente);
            return View(storicoIndividuo);
        }

        // GET: StoricoIndividuo/Delete/5
        public async Task<IActionResult> Delete(Guid? id, string tipo)
        {
            if (id == null || _context.StoricoIndividuo == null)
            {
                return NotFound();
            }

            var storicoIndividuo = await _context.StoricoIndividuo
                .Include(s => s.condizioneNavigation)
                .Include(s => s.individuoNavigation)
                .Include(s => s.statoIndividuoNavigation)
                .Include(s => s.utenteNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (storicoIndividuo == null)
            {
                return NotFound();
            }
            ViewBag.tipo = tipo;
            return View(storicoIndividuo);
        }

        // POST: StoricoIndividuo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, string tipo)
        {
            if (_context.StoricoIndividuo == null)
            {
                return Problem("Entity set 'Entities.StoricoIndividuo'  is null.");
            }
            var storicoIndividuo = await _context.StoricoIndividuo.FindAsync(id);
            if (storicoIndividuo != null)
            {
                _context.StoricoIndividuo.Remove(storicoIndividuo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), nameof(Individui), new
            {
                id = storicoIndividuo.individuo,
                tipo = tipo
            });
        }

        private bool StoricoIndividuoExists(Guid id)
        {
          return _context.StoricoIndividuo.Any(e => e.id == id);
        }
    }
}
