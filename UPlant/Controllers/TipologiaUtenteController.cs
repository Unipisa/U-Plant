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
    public class TipologiaUtenteController : Controller
    {
        private readonly Entities _context;

        public TipologiaUtenteController(Entities context)
        {
            _context = context;
        }

        // GET: TipologiaUtente
        public async Task<IActionResult> Index()
        {
            var entities = _context.TipologiaUtente.Include(t => t.organizzazioneNavigation).Include(t => t.Users).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: TipologiaUtente/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TipologiaUtente == null)
            {
                return NotFound();
            }

            var tipologiaUtente = await _context.TipologiaUtente
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipologiaUtente == null)
            {
                return NotFound();
            }

            return View(tipologiaUtente);
        }

        // GET: TipologiaUtente/Create
        public IActionResult Create()
        {
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.TipologiaUtente.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione");
            return View();
        }

        // POST: TipologiaUtente/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,organizzazione,ordinamento")] TipologiaUtente tipologiaUtente)
        {
            if (ModelState.IsValid)
            {
                tipologiaUtente.id = Guid.NewGuid();
                _context.Add(tipologiaUtente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipologiaUtente.organizzazione);
            return View(tipologiaUtente);
        }

        // GET: TipologiaUtente/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TipologiaUtente == null)
            {
                return NotFound();
            }

            var tipologiaUtente = await _context.TipologiaUtente.FindAsync(id);
            if (tipologiaUtente == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipologiaUtente.organizzazione);
            return View(tipologiaUtente);
        }

        // POST: TipologiaUtente/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,organizzazione,ordinamento")] TipologiaUtente tipologiaUtente)
        {
            if (id != tipologiaUtente.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipologiaUtente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipologiaUtenteExists(tipologiaUtente.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipologiaUtente.organizzazione);
            return View(tipologiaUtente);
        }

        // GET: TipologiaUtente/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TipologiaUtente == null)
            {
                return NotFound();
            }

            var tipologiaUtente = await _context.TipologiaUtente
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipologiaUtente == null)
            {
                return NotFound();
            }

            return View(tipologiaUtente);
        }

        // POST: TipologiaUtente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TipologiaUtente == null)
            {
                return Problem("Entity set 'Entities.TipologiaUtente'  is null.");
            }
            var tipologiaUtente = await _context.TipologiaUtente.FindAsync(id);
            if (tipologiaUtente != null)
            {
                _context.TipologiaUtente.Remove(tipologiaUtente);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipologiaUtenteExists(Guid id)
        {
          return _context.TipologiaUtente.Any(e => e.id == id);
        }
    }
}
