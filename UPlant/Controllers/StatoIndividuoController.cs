using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class StatoIndividuoController : BaseController
    {
        private readonly Entities _context;

        public StatoIndividuoController(Entities context)
        {
            _context = context;
        }

        // GET: StatoIndividuo
        public async Task<IActionResult> Index()
        {
            var entities = _context.StatoIndividuo.Include(s => s.organizzazioneNavigation).Include(a => a.StoricoIndividuo).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: StatoIndividuo/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.StatoIndividuo == null)
            {
                return NotFound();
            }

            var statoIndividuo = await _context.StatoIndividuo
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (statoIndividuo == null)
            {
                return NotFound();
            }

            return View(statoIndividuo);
        }

        // GET: StatoIndividuo/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.StatoIndividuo.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
            return View();
        }

        // POST: StatoIndividuo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,stato,descrizione_en,ordinamento,visualizzazioneweb,organizzazione")] StatoIndividuo statoIndividuo)
        {
            if (ModelState.IsValid)
            {
                statoIndividuo.id = Guid.NewGuid();
                _context.Add(statoIndividuo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoIndividuo.organizzazione);
            return View(statoIndividuo);
        }

        // GET: StatoIndividuo/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.StatoIndividuo == null)
            {
                return NotFound();
            }

            var statoIndividuo = await _context.StatoIndividuo.FindAsync(id);
            if (statoIndividuo == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoIndividuo.organizzazione);
            return View(statoIndividuo);
        }

        // POST: StatoIndividuo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,stato,descrizione_en,ordinamento,visualizzazioneweb,organizzazione")] StatoIndividuo statoIndividuo)
        {
            if (id != statoIndividuo.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statoIndividuo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatoIndividuoExists(statoIndividuo.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoIndividuo.organizzazione);
            return View(statoIndividuo);
        }

        // GET: StatoIndividuo/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.StatoIndividuo == null)
            {
                return NotFound();
            }

            var statoIndividuo = await _context.StatoIndividuo
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (statoIndividuo == null)
            {
                return NotFound();
            }

            return View(statoIndividuo);
        }

        // POST: StatoIndividuo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.StatoIndividuo == null)
            {
                return Problem("Entity set 'Entities.StatoIndividuo'  is null.");
            }
            var statoIndividuo = await _context.StatoIndividuo.FindAsync(id);
            if (statoIndividuo != null)
            {
                _context.StatoIndividuo.Remove(statoIndividuo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatoIndividuoExists(Guid id)
        {
          return _context.StatoIndividuo.Any(e => e.id == id);
        }
    }
}
