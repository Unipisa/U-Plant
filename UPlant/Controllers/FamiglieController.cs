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
    public class FamiglieController : BaseController
    {
        private readonly Entities _context;

        public FamiglieController(Entities context)
        {
            _context = context;
        }

        // GET: Famiglie
        public async Task<IActionResult> Index()
        {
              return View(await _context.Famiglie.Include(a => a.Generi).OrderBy(x => x.descrizione).ToListAsync());
        }

        // GET: Famiglie/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Famiglie == null)
            {
                return NotFound();
            }

            var famiglie = await _context.Famiglie
                .FirstOrDefaultAsync(m => m.id == id);
            if (famiglie == null)
            {
                return NotFound();
            }

            return View(famiglie);
        }

        // GET: Famiglie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Famiglie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en")] Famiglie famiglie)
        {
            if (ModelState.IsValid)
            {
                famiglie.id = Guid.NewGuid();
                _context.Add(famiglie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(famiglie);
        }

        // GET: Famiglie/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Famiglie == null)
            {
                return NotFound();
            }

            var famiglie = await _context.Famiglie.FindAsync(id);
            if (famiglie == null)
            {
                return NotFound();
            }
            return View(famiglie);
        }

        // POST: Famiglie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en")] Famiglie famiglie)
        {
            if (id != famiglie.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(famiglie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamiglieExists(famiglie.id))
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
            return View(famiglie);
        }

        // GET: Famiglie/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Famiglie == null)
            {
                return NotFound();
            }

            var famiglie = await _context.Famiglie
                .FirstOrDefaultAsync(m => m.id == id);
            if (famiglie == null)
            {
                return NotFound();
            }

            return View(famiglie);
        }

        // POST: Famiglie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Famiglie == null)
            {
                return Problem("Entity set 'Entities.Famiglie'  is null.");
            }
            var famiglie = await _context.Famiglie.FindAsync(id);
            if (famiglie != null)
            {
                _context.Famiglie.Remove(famiglie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FamiglieExists(Guid id)
        {
          return _context.Famiglie.Any(e => e.id == id);
        }
    }
}
