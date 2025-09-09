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
    public class CondizioniController : BaseController
    {
        private readonly Entities _context;

        public CondizioniController(Entities context)
        {
            _context = context;
        }

        // GET: Condizioni
        public async Task<IActionResult> Index()
        {
            var entities = _context.Condizioni.Include(c => c.organizzazioneNavigation).Include(a => a.StoricoIndividuo).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: Condizioni/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Condizioni == null)
            {
                return NotFound();
            }

            var condizioni = await _context.Condizioni
                .Include(c => c.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (condizioni == null)
            {
                return NotFound();
            }

            return View(condizioni);
        }

        // GET: Condizioni/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Condizioni.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto

            return View();
        }

        // POST: Condizioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,condizione,descrizione_en,ordinamento,organizzazione")] Condizioni condizioni)
        {
            if (ModelState.IsValid)
            {
                condizioni.id = Guid.NewGuid();
                _context.Add(condizioni);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", condizioni.organizzazione);
            return View(condizioni);
        }

        // GET: Condizioni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Condizioni == null)
            {
                return NotFound();
            }

            var condizioni = await _context.Condizioni.FindAsync(id);
            if (condizioni == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", condizioni.organizzazione);
            return View(condizioni);
        }

        // POST: Condizioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,condizione,descrizione_en,ordinamento,organizzazione")] Condizioni condizioni)
        {
            if (id != condizioni.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(condizioni);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CondizioniExists(condizioni.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", condizioni.organizzazione);
            return View(condizioni);
        }

        // GET: Condizioni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Condizioni == null)
            {
                return NotFound();
            }

            var condizioni = await _context.Condizioni
                .Include(c => c.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (condizioni == null)
            {
                return NotFound();
            }

            return View(condizioni);
        }

        // POST: Condizioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Condizioni == null)
            {
                return Problem("Entity set 'Entities.Condizioni'  is null.");
            }
            var condizioni = await _context.Condizioni.FindAsync(id);
            if (condizioni != null)
            {
                _context.Condizioni.Remove(condizioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CondizioniExists(Guid id)
        {
          return _context.Condizioni.Any(e => e.id == id);
        }
    }
}
