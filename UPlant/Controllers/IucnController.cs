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
    public class IucnController : BaseController
    {
        private readonly Entities _context;

        public IucnController(Entities context)
        {
            _context = context;
        }

        // GET: Iucn
        public async Task<IActionResult> Index()
        {
              return View(await _context.Iucn.Include(a => a.Specieiucn_globaleNavigation).OrderBy(x => x.ordinamento).ToListAsync());
        }

        // GET: Iucn/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Iucn == null)
            {
                return NotFound();
            }

            var iucn = await _context.Iucn
                .FirstOrDefaultAsync(m => m.id == id);
            if (iucn == null)
            {
                return NotFound();
            }

            return View(iucn);
        }

        // GET: Iucn/Create
        public IActionResult Create()
        {
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Iucn.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto
            return View();
        }

        // POST: Iucn/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,codice,ordinamento,descrizione")] Iucn iucn)
        {
            if (ModelState.IsValid)
            {
                iucn.id = Guid.NewGuid();
                _context.Add(iucn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iucn);
        }

        // GET: Iucn/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Iucn == null)
            {
                return NotFound();
            }

            var iucn = await _context.Iucn.FindAsync(id);
            if (iucn == null)
            {
                return NotFound();
            }
            return View(iucn);
        }

        // POST: Iucn/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,codice,ordinamento,descrizione")] Iucn iucn)
        {
            if (id != iucn.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iucn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IucnExists(iucn.id))
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
            return View(iucn);
        }

        // GET: Iucn/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Iucn == null)
            {
                return NotFound();
            }

            var iucn = await _context.Iucn
                .FirstOrDefaultAsync(m => m.id == id);
            if (iucn == null)
            {
                return NotFound();
            }

            return View(iucn);
        }

        // POST: Iucn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Iucn == null)
            {
                return Problem("Entity set 'Entities.Iucn'  is null.");
            }
            var iucn = await _context.Iucn.FindAsync(id);
            if (iucn != null)
            {
                _context.Iucn.Remove(iucn);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IucnExists(Guid id)
        {
          return _context.Iucn.Any(e => e.id == id);
        }
    }
}
