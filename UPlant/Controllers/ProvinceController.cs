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
    public class ProvinceController : BaseController
    {
        private readonly Entities _context;

        public ProvinceController(Entities context)
        {
            _context = context;
        }

        // GET: Province
        public async Task<IActionResult> Index()
        {
            var entities = _context.Province.Include(p => p.regioneNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: Province/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Province == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.regioneNavigation)
                .FirstOrDefaultAsync(m => m.codice == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // GET: Province/Create
        public IActionResult Create()
        {
            ViewData["regione"] = new SelectList(_context.Regioni, "codice", "codice");
            return View();
        }

        // POST: Province/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("codice,regione,descrizione,descrizione_en")] Province province)
        {
            if (ModelState.IsValid)
            {
                _context.Add(province);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["regione"] = new SelectList(_context.Regioni, "codice", "codice", province.regione);
            return View(province);
        }

        // GET: Province/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Province == null)
            {
                return NotFound();
            }

            var province = await _context.Province.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            ViewData["regione"] = new SelectList(_context.Regioni, "codice", "codice", province.regione);
            return View(province);
        }

        // POST: Province/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("codice,regione,descrizione,descrizione_en")] Province province)
        {
            if (id != province.codice)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(province);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvinceExists(province.codice))
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
            ViewData["regione"] = new SelectList(_context.Regioni, "codice", "codice", province.regione);
            return View(province);
        }

        // GET: Province/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Province == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.regioneNavigation)
                .FirstOrDefaultAsync(m => m.codice == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // POST: Province/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Province == null)
            {
                return Problem("Entity set 'Entities.Province'  is null.");
            }
            var province = await _context.Province.FindAsync(id);
            if (province != null)
            {
                _context.Province.Remove(province);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvinceExists(string id)
        {
          return _context.Province.Any(e => e.codice == id);
        }
    }
}
