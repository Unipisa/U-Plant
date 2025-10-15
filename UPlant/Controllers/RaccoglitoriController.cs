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
    public class RaccoglitoriController : BaseController
    {
        private readonly Entities _context;

        public RaccoglitoriController(Entities context)
        {
            _context = context;
        }

        // GET: Raccoglitori
        public async Task<IActionResult> Index()
        {
            var entities = _context.Raccoglitori.Include(r => r.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.nominativo);
            return View(await entities.ToListAsync());
        }

        // GET: Raccoglitori/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Raccoglitori == null)
            {
                return NotFound();
            }

            var raccoglitori = await _context.Raccoglitori
                .Include(r => r.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (raccoglitori == null)
            {
                return NotFound();
            }

            return View(raccoglitori);
        }

        // GET: Raccoglitori/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            return View();
        }

        // POST: Raccoglitori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,nominativo,nominativo_en,attivo")] Raccoglitori raccoglitori)
        {
            if (ModelState.IsValid)
            {
                raccoglitori.id = Guid.NewGuid();
                _context.Add(raccoglitori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", raccoglitori.organizzazione);
            return View(raccoglitori);
        }

        // GET: Raccoglitori/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Raccoglitori == null)
            {
                return NotFound();
            }

            var raccoglitori = await _context.Raccoglitori.FindAsync(id);
            if (raccoglitori == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", raccoglitori.organizzazione);
            return View(raccoglitori);
        }

        // POST: Raccoglitori/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,nominativo,nominativo_en,attivo")] Raccoglitori raccoglitori)
        {
            if (id != raccoglitori.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(raccoglitori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RaccoglitoriExists(raccoglitori.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", raccoglitori.organizzazione);
            return View(raccoglitori);
        }

        // GET: Raccoglitori/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Raccoglitori == null)
            {
                return NotFound();
            }

            var raccoglitori = await _context.Raccoglitori
                .Include(r => r.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (raccoglitori == null)
            {
                return NotFound();
            }

            return View(raccoglitori);
        }

        // POST: Raccoglitori/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Raccoglitori == null)
            {
                return Problem("Entity set 'Entities.Raccoglitori'  is null.");
            }
            var raccoglitori = await _context.Raccoglitori.FindAsync(id);
            if (raccoglitori != null)
            {
                _context.Raccoglitori.Remove(raccoglitori);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RaccoglitoriExists(Guid id)
        {
          return _context.Raccoglitori.Any(e => e.id == id);
        }
    }
}
