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
    public class SessoController : BaseController
    {
        private readonly Entities _context;

        public SessoController(Entities context)
        {
            _context = context;
        }

        // GET: Sesso
        public async Task<IActionResult> Index()
        {
            var entities = _context.Sesso.Include(s => s.organizzazioneNavigation).Include(a => a.Individui).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
        }

        // GET: Sesso/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Sesso == null)
            {
                return NotFound();
            }

            var sesso = await _context.Sesso
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (sesso == null)
            {
                return NotFound();
            }

            return View(sesso);
        }

        // GET: Sesso/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            return View();
        }

        // POST: Sesso/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en,organizzazione")] Sesso sesso)
        {
            if (ModelState.IsValid)
            {
                sesso.id = Guid.NewGuid();
                _context.Add(sesso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", sesso.organizzazione);
            return View(sesso);
        }

        // GET: Sesso/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Sesso == null)
            {
                return NotFound();
            }

            var sesso = await _context.Sesso.FindAsync(id);
            if (sesso == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", sesso.organizzazione);
            return View(sesso);
        }

        // POST: Sesso/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en,organizzazione")] Sesso sesso)
        {
            if (id != sesso.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sesso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessoExists(sesso.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", sesso.organizzazione);
            return View(sesso);
        }

        // GET: Sesso/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Sesso == null)
            {
                return NotFound();
            }

            var sesso = await _context.Sesso
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (sesso == null)
            {
                return NotFound();
            }

            return View(sesso);
        }

        // POST: Sesso/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Sesso == null)
            {
                return Problem("Entity set 'Entities.Sesso'  is null.");
            }
            var sesso = await _context.Sesso.FindAsync(id);
            if (sesso != null)
            {
                _context.Sesso.Remove(sesso);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessoExists(Guid id)
        {
          return _context.Sesso.Any(e => e.id == id);
        }
    }
}
