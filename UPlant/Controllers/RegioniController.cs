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
    public class RegioniController : BaseController
    {
        private readonly Entities _context;

        public RegioniController(Entities context)
        {
            _context = context;
        }

        // GET: Regioni
        public async Task<IActionResult> Index()
        {
              return View(await _context.Regioni.ToListAsync());
        }

        // GET: Regioni/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Regioni == null)
            {
                return NotFound();
            }

            var regioni = await _context.Regioni
                .FirstOrDefaultAsync(m => m.codice == id);
            if (regioni == null)
            {
                return NotFound();
            }

            return View(regioni);
        }

        // GET: Regioni/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Regioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("codice,descrizione,descrizione_en")] Regioni regioni)
        {
            if (ModelState.IsValid)
            {
                _context.Add(regioni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(regioni);
        }

        // GET: Regioni/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Regioni == null)
            {
                return NotFound();
            }

            var regioni = await _context.Regioni.FindAsync(id);
            if (regioni == null)
            {
                return NotFound();
            }
            return View(regioni);
        }

        // POST: Regioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("codice,descrizione,descrizione_en")] Regioni regioni)
        {
            if (id != regioni.codice)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(regioni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegioniExists(regioni.codice))
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
            return View(regioni);
        }

        // GET: Regioni/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Regioni == null)
            {
                return NotFound();
            }

            var regioni = await _context.Regioni
                .FirstOrDefaultAsync(m => m.codice == id);
            if (regioni == null)
            {
                return NotFound();
            }

            return View(regioni);
        }

        // POST: Regioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Regioni == null)
            {
                return Problem("Entity set 'Entities.Regioni'  is null.");
            }
            var regioni = await _context.Regioni.FindAsync(id);
            if (regioni != null)
            {
                _context.Regioni.Remove(regioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegioniExists(string id)
        {
          return _context.Regioni.Any(e => e.codice == id);
        }
    }
}
