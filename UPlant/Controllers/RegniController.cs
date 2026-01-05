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
    public class RegniController : BaseController
    {
        private readonly Entities _context;

        public RegniController(Entities context)
        {
            _context = context;
        }

        // GET: Regni
        public async Task<IActionResult> Index()
        {
              return View(await _context.Regni.OrderBy(x => x.ordinamento).ToListAsync());
        }

        // GET: Regni/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Regni == null)
            {
                return NotFound();
            }

            var regni = await _context.Regni
                .FirstOrDefaultAsync(m => m.id == id);
            if (regni == null)
            {
                return NotFound();
            }

            return View(regni);
        }

        // GET: Regni/Create
        public IActionResult Create()
        {
            var ultimo = _context.Regni.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
            return View();
        }

        // POST: Regni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,codiceInterno,descrizione_en,ordinamento")] Regni regni)
        {
            if (ModelState.IsValid)
            {
                regni.id = Guid.NewGuid();
                _context.Add(regni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(regni);
        }

        // GET: Regni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Regni == null)
            {
                return NotFound();
            }

            var regni = await _context.Regni.FindAsync(id);
            if (regni == null)
            {
                return NotFound();
            }
            return View(regni);
        }

        // POST: Regni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,codiceInterno,descrizione_en,ordinamento")] Regni regni)
        {
            if (id != regni.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(regni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegniExists(regni.id))
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
            return View(regni);
        }

        // GET: Regni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Regni == null)
            {
                return NotFound();
            }

            var regni = await _context.Regni
                .FirstOrDefaultAsync(m => m.id == id);
            if (regni == null)
            {
                return NotFound();
            }

            return View(regni);
        }

        // POST: Regni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Regni == null)
            {
                return Problem("Entity set 'Entities.Regni'  is null.");
            }
            var regni = await _context.Regni.FindAsync(id);
            if (regni != null)
            {
                _context.Regni.Remove(regni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegniExists(Guid id)
        {
          return _context.Regni.Any(e => e.id == id);
        }
    }
}
