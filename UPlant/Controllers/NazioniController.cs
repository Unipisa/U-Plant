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
    public class NazioniController : BaseController
    {
        private readonly Entities _context;

        public NazioniController(Entities context)
        {
            _context = context;
        }

        // GET: Nazioni
        public async Task<IActionResult> Index()
        {
              return View(await _context.Nazioni.OrderBy(x => x.descrizione).Include(t => t.Accessioni).ToListAsync());
        }

        // GET: Nazioni/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Nazioni == null)
            {
                return NotFound();
            }

            var nazioni = await _context.Nazioni
                .FirstOrDefaultAsync(m => m.codice == id);
            if (nazioni == null)
            {
                return NotFound();
            }

            return View(nazioni);
        }

        // GET: Nazioni/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Nazioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("codice,descrizione,descrizione_en")] Nazioni nazioni)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nazioni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nazioni);
        }

        // GET: Nazioni/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Nazioni == null)
            {
                return NotFound();
            }

            var nazioni = await _context.Nazioni.FindAsync(id);
            if (nazioni == null)
            {
                return NotFound();
            }
            return View(nazioni);
        }

        // POST: Nazioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("codice,descrizione,descrizione_en")] Nazioni nazioni)
        {
            if (id != nazioni.codice)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nazioni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NazioniExists(nazioni.codice))
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
            return View(nazioni);
        }

        // GET: Nazioni/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Nazioni == null)
            {
                return NotFound();
            }

            var nazioni = await _context.Nazioni
                .FirstOrDefaultAsync(m => m.codice == id);
            if (nazioni == null)
            {
                return NotFound();
            }

            return View(nazioni);
        }

        // POST: Nazioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Nazioni == null)
            {
                return Problem("Entity set 'Entities.Nazioni'  is null.");
            }
            var nazioni = await _context.Nazioni.FindAsync(id);
            if (nazioni != null)
            {
                _context.Nazioni.Remove(nazioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NazioniExists(string id)
        {
          return _context.Nazioni.Any(e => e.codice == id);
        }
    }
}
