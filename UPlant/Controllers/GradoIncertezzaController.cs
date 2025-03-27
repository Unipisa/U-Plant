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
    public class GradoIncertezzaController : BaseController
    {
        private readonly Entities _context;

        public GradoIncertezzaController(Entities context)
        {
            _context = context;
        }

        // GET: GradoIncertezza
        public async Task<IActionResult> Index()
        {
            var entities = _context.GradoIncertezza.Include(g => g.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
        }

        // GET: GradoIncertezza/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.GradoIncertezza == null)
            {
                return NotFound();
            }

            var gradoIncertezza = await _context.GradoIncertezza
                .Include(g => g.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (gradoIncertezza == null)
            {
                return NotFound();
            }

            return View(gradoIncertezza);
        }

        // GET: GradoIncertezza/Create
        public IActionResult Create()
        {
             string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x =>x.Organizzazione).FirstOrDefault());
            return View();
        }

        // POST: GradoIncertezza/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,organizzazione")] GradoIncertezza gradoIncertezza)
        {
            if (ModelState.IsValid)
            {
                gradoIncertezza.id = Guid.NewGuid();
                _context.Add(gradoIncertezza);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", gradoIncertezza.organizzazione);
            return View(gradoIncertezza);
        }

        // GET: GradoIncertezza/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.GradoIncertezza == null)
            {
                return NotFound();
            }

            var gradoIncertezza = await _context.GradoIncertezza.FindAsync(id);
            if (gradoIncertezza == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", gradoIncertezza.organizzazione);
            return View(gradoIncertezza);
        }

        // POST: GradoIncertezza/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,organizzazione")] GradoIncertezza gradoIncertezza)
        {
            if (id != gradoIncertezza.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gradoIncertezza);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradoIncertezzaExists(gradoIncertezza.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", gradoIncertezza.organizzazione);
            return View(gradoIncertezza);
        }

        // GET: GradoIncertezza/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.GradoIncertezza == null)
            {
                return NotFound();
            }

            var gradoIncertezza = await _context.GradoIncertezza
                .Include(g => g.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (gradoIncertezza == null)
            {
                return NotFound();
            }

            return View(gradoIncertezza);
        }

        // POST: GradoIncertezza/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.GradoIncertezza == null)
            {
                return Problem("Entity set 'Entities.GradoIncertezza'  is null.");
            }
            var gradoIncertezza = await _context.GradoIncertezza.FindAsync(id);
            if (gradoIncertezza != null)
            {
                _context.GradoIncertezza.Remove(gradoIncertezza);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GradoIncertezzaExists(Guid id)
        {
          return _context.GradoIncertezza.Any(e => e.id == id);
        }
    }
}
