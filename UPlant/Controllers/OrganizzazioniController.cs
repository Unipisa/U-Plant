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
    public class OrganizzazioniController : BaseController
    {
        private readonly Entities _context;

        public OrganizzazioniController(Entities context)
        {
            _context = context;
        }

        // GET: Organizzazioni
        public async Task<IActionResult> Index()
        {
              return View(await _context.Organizzazioni.Include(u => u.Accessioni).OrderBy(x => x.descrizione).ToListAsync());
        }

        // GET: Organizzazioni/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Organizzazioni == null)
            {
                return NotFound();
            }

            var organizzazioni = await _context.Organizzazioni
                .FirstOrDefaultAsync(m => m.id == id);
            if (organizzazioni == null)
            {
                return NotFound();
            }

            return View(organizzazioni);
        }

        // GET: Organizzazioni/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizzazioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,attivo,prefissoIpen")] Organizzazioni organizzazioni)
        {
            if (ModelState.IsValid)
            {

                organizzazioni.id = Guid.NewGuid();
                organizzazioni.prefissoIpen = organizzazioni.prefissoIpen.ToUpper();
                _context.Add(organizzazioni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(organizzazioni);
        }

        // GET: Organizzazioni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Organizzazioni == null)
            {
                return NotFound();
            }

            var organizzazioni = await _context.Organizzazioni.FindAsync(id);
            if (organizzazioni == null)
            {
                return NotFound();
            }
            return View(organizzazioni);
        }

        // POST: Organizzazioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,attivo,prefissoIpen")] Organizzazioni organizzazioni)
        {
            if (id != organizzazioni.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizzazioni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizzazioniExists(organizzazioni.id))
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
            return View(organizzazioni);
        }

        // GET: Organizzazioni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Organizzazioni == null)
            {
                return NotFound();
            }

            var organizzazioni = await _context.Organizzazioni
                .FirstOrDefaultAsync(m => m.id == id);
            if (organizzazioni == null)
            {
                return NotFound();
            }

            return View(organizzazioni);
        }

        // POST: Organizzazioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Organizzazioni == null)
            {
                return Problem("Entity set 'Entities.Organizzazioni'  is null.");
            }
            var organizzazioni = await _context.Organizzazioni.FindAsync(id);
            if (organizzazioni != null)
            {
                _context.Organizzazioni.Remove(organizzazioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizzazioniExists(Guid id)
        {
          return _context.Organizzazioni.Any(e => e.id == id);
        }
    }
}
