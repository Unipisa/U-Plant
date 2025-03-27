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
    public class GeneriController : BaseController
    {
        private readonly Entities _context;

        public GeneriController(Entities context)
        {
            _context = context;
        }

        // GET: Generi
        public async Task<IActionResult> Index()
        {
            var entities = _context.Generi.Include(g => g.famigliaNavigation).Include(a => a.Specie).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
        }

        // GET: Generi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Generi == null)
            {
                return NotFound();
            }

            var generi = await _context.Generi
                .Include(g => g.famigliaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (generi == null)
            {
                return NotFound();
            }

            return View(generi);
        }

        // GET: Generi/Create
        public IActionResult Create()
        {
            ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione");
            return View();
        }

        // POST: Generi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,famiglia")] Generi generi)
        {
            if (ModelState.IsValid)
            {
                generi.id = Guid.NewGuid();
                _context.Add(generi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione", generi.famiglia);
            return View(generi);
        }

        // GET: Generi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Generi == null)
            {
                return NotFound();
            }

            var generi = await _context.Generi.FindAsync(id);
            if (generi == null)
            {
                return NotFound();
            }
            ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione", generi.famiglia);
            return View(generi);
        }

        // POST: Generi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,famiglia")] Generi generi)
        {
            if (id != generi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(generi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneriExists(generi.id))
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
            ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione", generi.famiglia);
            return View(generi);
        }

        // GET: Generi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Generi == null)
            {
                return NotFound();
            }

            var generi = await _context.Generi
                .Include(g => g.famigliaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (generi == null)
            {
                return NotFound();
            }

            return View(generi);
        }

        // POST: Generi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Generi == null)
            {
                return Problem("Entity set 'Entities.Generi'  is null.");
            }
            var generi = await _context.Generi.FindAsync(id);
            if (generi != null)
            {
                _context.Generi.Remove(generi);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeneriExists(Guid id)
        {
          return _context.Generi.Any(e => e.id == id);
        }
    }
}
