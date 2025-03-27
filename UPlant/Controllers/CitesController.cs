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
    public class CitesController : Controller
    {
        private readonly Entities _context;

        public CitesController(Entities context)
        {
            _context = context;
        }

        // GET: Cites
        public async Task<IActionResult> Index()
        {
              return View(await _context.Cites.Include(a => a.Specie).OrderBy(x => x.descrizione).ToListAsync());
        }

        // GET: Cites/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Cites == null)
            {
                return NotFound();
            }

            var cites = await _context.Cites
                .FirstOrDefaultAsync(m => m.id == id);
            if (cites == null)
            {
                return NotFound();
            }

            return View(cites);
        }

        // GET: Cites/Create
        public IActionResult Create()
        {
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Cites.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto

            return View();
        }

        // POST: Cites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,codice,ordinamento,descrizione")] Cites cites)
        {
            if (ModelState.IsValid)
            {
                cites.id = Guid.NewGuid();
                _context.Add(cites);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cites);
        }

        // GET: Cites/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Cites == null)
            {
                return NotFound();
            }

            var cites = await _context.Cites.FindAsync(id);
            if (cites == null)
            {
                return NotFound();
            }
            return View(cites);
        }

        // POST: Cites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,codice,ordinamento,descrizione")] Cites cites)
        {
            if (id != cites.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cites);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitesExists(cites.id))
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
            return View(cites);
        }

        // GET: Cites/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Cites == null)
            {
                return NotFound();
            }

            var cites = await _context.Cites
                .FirstOrDefaultAsync(m => m.id == id);
            if (cites == null)
            {
                return NotFound();
            }

            return View(cites);
        }

        // POST: Cites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Cites == null)
            {
                return Problem("Entity set 'Entities.Cites'  is null.");
            }
            var cites = await _context.Cites.FindAsync(id);
            if (cites != null)
            {
                _context.Cites.Remove(cites);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CitesExists(Guid id)
        {
          return _context.Cites.Any(e => e.id == id);
        }
    }
}
