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
    public class CollezioniController : BaseController
    {
        private readonly Entities _context;

        public CollezioniController(Entities context)
        {
            _context = context;
        }

        // GET: Collezioni
        public async Task<IActionResult> Index()
        {
            var entities = _context.Collezioni.Include(c => c.organizzazioneNavigation).Include(c => c.settoreNavigation).Include(a => a.Individui).OrderBy(x => x.collezione);
            return View(await entities.ToListAsync());
        }

        // GET: Collezioni/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni
                .Include(c => c.organizzazioneNavigation)
                .Include(c => c.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (collezioni == null)
            {
                return NotFound();
            }

            return View(collezioni);
        }

        // GET: Collezioni/Create
        public IActionResult Create()
        {
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione");
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore");
            return View();
        }

        // POST: Collezioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,collezione,organizzazione,settore,collezione_en")] Collezioni collezioni)
        {
            if (!await _context.Organizzazioni.AnyAsync(x => x.id == collezioni.organizzazione))
            {
                ModelState.AddModelError(nameof(collezioni.organizzazione), "Seleziona un'organizzazione valida.");
            }
            if (!await _context.Settori.AnyAsync(x => x.id == collezioni.settore && x.organizzazione == collezioni.organizzazione))
            {
                ModelState.AddModelError(nameof(collezioni.settore), "Seleziona un settore valido per l'organizzazione indicata.");
            }
            if (ModelState.IsValid)
            {
                collezioni.id = Guid.NewGuid();
                _context.Add(collezioni);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    HttpContext.RequestServices.GetRequiredService<ILogger<CollezioniController>>()
                        .LogError(ex, "Errore durante la creazione della collezione");
                    ModelState.AddModelError(string.Empty, "Impossibile salvare la collezione. Verifica i dati e riprova.");
                }
            }
            string username = UPlant.Services.UserIdentityHelper.GetUsername(User);
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == username);
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // GET: Collezioni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni.FindAsync(id);
            if (collezioni == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", collezioni.organizzazione);
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // POST: Collezioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,collezione,organizzazione,settore,collezione_en")] Collezioni collezioni)
        {
            if (id != collezioni.id)
            {
                return NotFound();
            }

            if (!await _context.Organizzazioni.AnyAsync(x => x.id == collezioni.organizzazione))
            {
                ModelState.AddModelError(nameof(collezioni.organizzazione), "Seleziona un'organizzazione valida.");
            }
            if (!await _context.Settori.AnyAsync(x => x.id == collezioni.settore && x.organizzazione == collezioni.organizzazione))
            {
                ModelState.AddModelError(nameof(collezioni.settore), "Seleziona un settore valido per l'organizzazione indicata.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collezioni);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollezioniExists(collezioni.id))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError(string.Empty, "I dati sono stati modificati da un altro utente. Ricarica la pagina e riprova.");
                }
                catch (DbUpdateException ex)
                {
                    HttpContext.RequestServices.GetRequiredService<ILogger<CollezioniController>>()
                        .LogError(ex, "Errore durante l'aggiornamento di Collezioni {Id}", id);
                    ModelState.AddModelError(string.Empty, "Impossibile aggiornare i dati. Verifica i riferimenti selezionati e riprova.");
                }
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", collezioni.organizzazione);
            ViewData["settore"] = new SelectList(_context.Settori, "id", "settore", collezioni.settore);
            return View(collezioni);
        }

        // GET: Collezioni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Collezioni == null)
            {
                return NotFound();
            }

            var collezioni = await _context.Collezioni
                .Include(c => c.organizzazioneNavigation)
                .Include(c => c.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (collezioni == null)
            {
                return NotFound();
            }

            return View(collezioni);
        }

        // POST: Collezioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Collezioni == null)
            {
                return Problem("Entity set 'Entities.Collezioni'  is null.");
            }
            var collezioni = await _context.Collezioni.FindAsync(id);
            if (collezioni != null)
            {
                _context.Collezioni.Remove(collezioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollezioniExists(Guid id)
        {
          return _context.Collezioni.Any(e => e.id == id);
        }
    }
}
