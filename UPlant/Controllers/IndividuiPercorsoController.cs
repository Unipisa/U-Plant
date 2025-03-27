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
    public class IndividuiPercorsoController : BaseController
    {
        private readonly Entities _context;

        public IndividuiPercorsoController(Entities context)
        {
            _context = context;
        }

        // GET: IndividuiPercorso
        public async Task<IActionResult> Index()
        {
            var entities = _context.IndividuiPercorso.Include(i => i.individuoNavigation).Include(i => i.percorsoNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: IndividuiPercorso/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.IndividuiPercorso == null)
            {
                return NotFound();
            }

            var individuiPercorso = await _context.IndividuiPercorso
                .Include(i => i.individuoNavigation)
                .Include(i => i.percorsoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (individuiPercorso == null)
            {
                return NotFound();
            }

            return View(individuiPercorso);
        }

        // GET: IndividuiPercorso/Create
        public IActionResult Create()
        {
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo");
            ViewData["percorso"] = new SelectList(_context.Percorsi, "id", "autore");
            return View();
        }

        // POST: IndividuiPercorso/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,percorso,individuo")] IndividuiPercorso individuiPercorso)
        {
            if (ModelState.IsValid)
            {
                individuiPercorso.id = Guid.NewGuid();
                _context.Add(individuiPercorso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", individuiPercorso.individuo);
            ViewData["percorso"] = new SelectList(_context.Percorsi, "id", "autore", individuiPercorso.percorso);
            return View(individuiPercorso);
        }

        // GET: IndividuiPercorso/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.IndividuiPercorso == null)
            {
                return NotFound();
            }

            var individuiPercorso = await _context.IndividuiPercorso.FindAsync(id);
            if (individuiPercorso == null)
            {
                return NotFound();
            }
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", individuiPercorso.individuo);
            ViewData["percorso"] = new SelectList(_context.Percorsi, "id", "autore", individuiPercorso.percorso);
            return View(individuiPercorso);
        }

        // POST: IndividuiPercorso/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,percorso,individuo")] IndividuiPercorso individuiPercorso)
        {
            if (id != individuiPercorso.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(individuiPercorso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndividuiPercorsoExists(individuiPercorso.id))
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
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", individuiPercorso.individuo);
            ViewData["percorso"] = new SelectList(_context.Percorsi, "id", "autore", individuiPercorso.percorso);
            return View(individuiPercorso);
        }

        // GET: IndividuiPercorso/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.IndividuiPercorso == null)
            {
                return NotFound();
            }

            var individuiPercorso = await _context.IndividuiPercorso
                .Include(i => i.individuoNavigation)
                .Include(i => i.percorsoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (individuiPercorso == null)
            {
                return NotFound();
            }

            return View(individuiPercorso);
        }

        // POST: IndividuiPercorso/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.IndividuiPercorso == null)
            {
                return Problem("Entity set 'Entities.IndividuiPercorso'  is null.");
            }
            var individuiPercorso = await _context.IndividuiPercorso.FindAsync(id);
            if (individuiPercorso != null)
            {
                _context.IndividuiPercorso.Remove(individuiPercorso);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IndividuiPercorsoExists(Guid id)
        {
          return _context.IndividuiPercorso.Any(e => e.id == id);
        }
    }
}
