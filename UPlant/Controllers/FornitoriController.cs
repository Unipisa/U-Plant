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
    public class FornitoriController : BaseController
    {
        private readonly Entities _context;

        public FornitoriController(Entities context)
        {
            _context = context;
        }

        // GET: Fornitori
        public async Task<IActionResult> Index()
        {
            var entities = _context.Fornitori.Include(f => f.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
        }

        // GET: Fornitori/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Fornitori == null)
            {
                return NotFound();
            }

            var fornitori = await _context.Fornitori
                .Include(f => f.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (fornitori == null)
            {
                return NotFound();
            }

            return View(fornitori);
        }

        // GET: Fornitori/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            return View();
        }

        // POST: Fornitori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione,note,attivo")] Fornitori fornitori)
        {
            if (ModelState.IsValid)
            {
                fornitori.id = Guid.NewGuid();
                _context.Add(fornitori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", fornitori.organizzazione);
            return View(fornitori);
        }

        // GET: Fornitori/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Fornitori == null)
            {
                return NotFound();
            }

            var fornitori = await _context.Fornitori.FindAsync(id);
            if (fornitori == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", fornitori.organizzazione);
            return View(fornitori);
        }

        // POST: Fornitori/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione,note,attivo")] Fornitori fornitori)
        {
            if (id != fornitori.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fornitori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornitoriExists(fornitori.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", fornitori.organizzazione);
            return View(fornitori);
        }

        // GET: Fornitori/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Fornitori == null)
            {
                return NotFound();
            }

            var fornitori = await _context.Fornitori
                .Include(f => f.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (fornitori == null)
            {
                return NotFound();
            }

            return View(fornitori);
        }

        // POST: Fornitori/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Fornitori == null)
            {
                return Problem("Entity set 'Entities.Fornitori'  is null.");
            }
            var fornitori = await _context.Fornitori.FindAsync(id);
            if (fornitori != null)
            {
                _context.Fornitori.Remove(fornitori);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FornitoriExists(Guid id)
        {
          return _context.Fornitori.Any(e => e.id == id);
        }
    }
}
