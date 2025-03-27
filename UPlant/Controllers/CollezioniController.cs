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
    public class CollezioniController : BaseController
    {
        private readonly Entities _context;

        public CollezioniController(Entities context)
        {
            _context = context;
        }

        // GET: Collezioni
        public async Task<IActionResult> Index()
        {
            var entities = _context.Collezioni.Include(c => c.organizzazioneNavigation).Include(c => c.settoreNavigation).Include(a => a.Individui).OrderBy(x => x.collezione);
            return View(await entities.ToListAsync());
        }

        // GET: Collezioni/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni
                .Include(c => c.organizzazioneNavigation)
                .Include(c => c.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (collezioni == null)
            {
                return NotFound();
            }

            return View(collezioni);
        }

        // GET: Collezioni/Create
        public IActionResult Create()
        {
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione");
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore");
            return View();
        }

        // POST: Collezioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,collezione,organizzazione,settore,collezione_en")] Collezioni collezioni)
        {
            if (ModelState.IsValid)
            {
                collezioni.id = Guid.NewGuid();
                _context.Add(collezioni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // GET: Collezioni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni.FindAsync(id);
            if (collezioni == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", collezioni.organizzazione);
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // POST: Collezioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,collezione,organizzazione,settore,collezione_en")] Collezioni collezioni)
        {
            if (id != collezioni.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collezioni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollezioniExists(collezioni.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", collezioni.organizzazione);
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // GET: Collezioni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni
                .Include(c => c.organizzazioneNavigation)
                .Include(c => c.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (collezioni == null)
            {
                return NotFound();
            }

            return View(collezioni);
        }

        // POST: Collezioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Collezioni == null)
            {
                return Problem("Entity set 'Entities.Collezioni'  is null.");
            }
            var collezioni = await _context.Collezioni.FindAsync(id);
            if (collezioni != null)
            {
                _context.Collezioni.Remove(collezioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollezioniExists(Guid id)
        {
          return _context.Collezioni.Any(e => e.id == id);
        }
    }
}
