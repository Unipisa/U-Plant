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
    public class StatoMaterialeController : BaseController
    {
        private readonly Entities _context;

        public StatoMaterialeController(Entities context)
        {
            _context = context;
        }

        // GET: StatoMateriale
        public async Task<IActionResult> Index()
        {
            var entities = _context.StatoMateriale.Include(s => s.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.ordinamento); 
            return View(await entities.ToListAsync());
        }

        // GET: StatoMateriale/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.StatoMateriale == null)
            {
                return NotFound();
            }

            var statoMateriale = await _context.StatoMateriale
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (statoMateriale == null)
            {
                return NotFound();
            }

            return View(statoMateriale);
        }

        // GET: StatoMateriale/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.StatoMateriale.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
           return View();
        }

        // POST: StatoMateriale/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] StatoMateriale statoMateriale)
        {
            if (ModelState.IsValid)
            {
                statoMateriale.id = Guid.NewGuid();
                _context.Add(statoMateriale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoMateriale.organizzazione);
            return View(statoMateriale);
        }

        // GET: StatoMateriale/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.StatoMateriale == null)
            {
                return NotFound();
            }

            var statoMateriale = await _context.StatoMateriale.FindAsync(id);
            if (statoMateriale == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoMateriale.organizzazione);
            return View(statoMateriale);
        }

        // POST: StatoMateriale/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] StatoMateriale statoMateriale)
        {
            if (id != statoMateriale.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statoMateriale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatoMaterialeExists(statoMateriale.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statoMateriale.organizzazione);
            return View(statoMateriale);
        }

        // GET: StatoMateriale/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.StatoMateriale == null)
            {
                return NotFound();
            }

            var statoMateriale = await _context.StatoMateriale
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (statoMateriale == null)
            {
                return NotFound();
            }

            return View(statoMateriale);
        }

        // POST: StatoMateriale/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.StatoMateriale == null)
            {
                return Problem("Entity set 'Entities.StatoMateriale'  is null.");
            }
            var statoMateriale = await _context.StatoMateriale.FindAsync(id);
            if (statoMateriale != null)
            {
                _context.StatoMateriale.Remove(statoMateriale);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatoMaterialeExists(Guid id)
        {
          return _context.StatoMateriale.Any(e => e.id == id);
        }
    }
}
