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
    public class ImmaginiIndividuoController : BaseController
    {
        private readonly Entities _context;

        public ImmaginiIndividuoController(Entities context)
        {
            _context = context;
        }

        // GET: Immagini
        public async Task<IActionResult> Index()
        {
            var entities = _context.ImmaginiIndividuo.Include(i => i.individuoNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: Immagini/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.ImmaginiIndividuo == null)
            {
                return NotFound();
            }

            var immagini = await _context.ImmaginiIndividuo
                .Include(i => i.individuoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (immagini == null)
            {
                return NotFound();
            }

            return View(immagini);
        }

        // GET: Immagini/Create
        public IActionResult Create()
        {
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo");
            return View();
        }

        // POST: Immagini/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,individuo,descrizione,dataInserimento,autore,nomefile,visibile,predefinita,credits")] ImmaginiIndividuo immagini)
        {
            if (ModelState.IsValid)
            {
                immagini.id = Guid.NewGuid();
                _context.Add(immagini);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", immagini.individuo);
            return View(immagini);
        }

        // GET: Immagini/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ImmaginiIndividuo == null)
            {
                return NotFound();
            }

            var immagini = await _context.ImmaginiIndividuo.FindAsync(id);
            if (immagini == null)
            {
                return NotFound();
            }
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", immagini.individuo);
            return View(immagini);
        }

        // POST: Immagini/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,individuo,descrizione,dataInserimento,autore,nomefile,visibile,predefinita,credits")] ImmaginiIndividuo immagini)
        {
            if (id != immagini.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(immagini);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImmaginiExists(immagini.id))
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
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", immagini.individuo);
            return View(immagini);
        }

        // GET: Immagini/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ImmaginiIndividuo == null)
            {
                return NotFound();
            }

            var immagini = await _context.ImmaginiIndividuo
                .Include(i => i.individuoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (immagini == null)
            {
                return NotFound();
            }

            return View(immagini);
        }

        // POST: Immagini/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ImmaginiIndividuo == null)
            {
                return Problem("Entity set 'Entities.Immagini'  is null.");
            }
            var immagini = await _context.ImmaginiIndividuo.FindAsync(id);
            if (immagini != null)
            {
                _context.ImmaginiIndividuo.Remove(immagini);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImmaginiExists(Guid id)
        {
          return _context.ImmaginiIndividuo.Any(e => e.id == id);
        }
    }
}
