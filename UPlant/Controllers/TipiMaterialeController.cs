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
    public class TipiMaterialeController : BaseController
    {
        private readonly Entities _context;

        public TipiMaterialeController(Entities context)
        {
            _context = context;
        }

        // GET: TipiMateriale
        public async Task<IActionResult> Index()
        {
            var entities = _context.TipiMateriale.Include(t => t.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: TipiMateriale/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TipiMateriale == null)
            {
                return NotFound();
            }

            var tipiMateriale = await _context.TipiMateriale
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipiMateriale == null)
            {
                return NotFound();
            }

            return View(tipiMateriale);
        }

        // GET: TipiMateriale/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.TipiMateriale.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);
            return View();
        }

        // POST: TipiMateriale/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] TipiMateriale tipiMateriale)
        {
            if (ModelState.IsValid)
            {
                tipiMateriale.id = Guid.NewGuid();
                _context.Add(tipiMateriale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipiMateriale.organizzazione);
            return View(tipiMateriale);
        }

        // GET: TipiMateriale/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TipiMateriale == null)
            {
                return NotFound();
            }

            var tipiMateriale = await _context.TipiMateriale.FindAsync(id);
            if (tipiMateriale == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipiMateriale.organizzazione);
            return View(tipiMateriale);
        }

        // POST: TipiMateriale/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] TipiMateriale tipiMateriale)
        {
            if (id != tipiMateriale.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipiMateriale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipiMaterialeExists(tipiMateriale.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipiMateriale.organizzazione);
            return View(tipiMateriale);
        }

        // GET: TipiMateriale/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TipiMateriale == null)
            {
                return NotFound();
            }

            var tipiMateriale = await _context.TipiMateriale
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipiMateriale == null)
            {
                return NotFound();
            }

            return View(tipiMateriale);
        }

        // POST: TipiMateriale/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TipiMateriale == null)
            {
                return Problem("Entity set 'Entities.TipiMateriale'  is null.");
            }
            var tipiMateriale = await _context.TipiMateriale.FindAsync(id);
            if (tipiMateriale != null)
            {
                _context.TipiMateriale.Remove(tipiMateriale);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipiMaterialeExists(Guid id)
        {
          return _context.TipiMateriale.Any(e => e.id == id);
        }
    }
}
