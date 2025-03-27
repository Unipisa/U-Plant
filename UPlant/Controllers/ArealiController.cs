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
    public class ArealiController : Controller
    {
        private readonly Entities _context;

        public ArealiController(Entities context)
        {
            _context = context;
        }

        // GET: Areali
        public async Task<IActionResult> Index()
        {
              return View(await _context.Areali.OrderBy(x => x.descrizione).ToListAsync());
        }

        // GET: Areali/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Areali == null)
            {
                return NotFound();
            }

            var areali = await _context.Areali
                .FirstOrDefaultAsync(m => m.id == id);
            if (areali == null)
            {
                return NotFound();
            }

            return View(areali);
        }

        // GET: Areali/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Areali/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,codiceInterno")] Areali areali)
        {
            if (ModelState.IsValid)
            {
                areali.id = Guid.NewGuid();
                _context.Add(areali);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(areali);
        }

        // GET: Areali/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Areali == null)
            {
                return NotFound();
            }

            var areali = await _context.Areali.FindAsync(id);
            if (areali == null)
            {
                return NotFound();
            }
            return View(areali);
        }

        // POST: Areali/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,codiceInterno")] Areali areali)
        {
            if (id != areali.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(areali);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArealiExists(areali.id))
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
            return View(areali);
        }

        // GET: Areali/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Areali == null)
            {
                return NotFound();
            }

            var areali = await _context.Areali
                .FirstOrDefaultAsync(m => m.id == id);
            if (areali == null)
            {
                return NotFound();
            }

            return View(areali);
        }

        // POST: Areali/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Areali == null)
            {
                return Problem("Entity set 'Entities.Areali'  is null.");
            }
            var areali = await _context.Areali.FindAsync(id);
            if (areali != null)
            {
                _context.Areali.Remove(areali);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArealiExists(Guid id)
        {
          return _context.Areali.Any(e => e.id == id);
        }
    }
}
