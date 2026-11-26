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
    public class SettoriController : Controller
    {
        private readonly Entities _context;

        public SettoriController(Entities context)
        {
            _context = context;
        }

        // GET: Settori
        public async Task<IActionResult> Index()
        {
            var entities = _context.Settori.Include(s => s.organizzazioneNavigation).Include(a => a.Individui).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: Settori/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Settori == null)
            {
                return NotFound();
            }

            var settori = await _context.Settori
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (settori == null)
            {
                return NotFound();
            }

            return View(settori);
        }

        // GET: Settori/Create
        public async Task<IActionResult> Create()
        {
            var organizationId = await GetCreateOrganizationAsync();
            if (!organizationId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Impossibile individuare l'organizzazione dell'utente. Verifica la configurazione dell'account.");
            }
            var ultimo = await _context.Settori.MaxAsync(x => (int?)x.ordinamento);
            return View(new Settori
            {
                organizzazione = organizationId ?? Guid.Empty,
                ordinamento = StaticUtils.GeneraSuccessivo(ultimo),
                colore = "#6c757d"
            });
        }

        // POST: Settori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("settore,visualizzazioneweb,ordinamento,settore_en,colore")] Settori settori)
        {
            var organizationId = await GetCreateOrganizationAsync();
            if (!organizationId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Impossibile individuare l'organizzazione dell'utente. Verifica la configurazione dell'account.");
            }
            else
            {
                settori.organizzazione = organizationId.Value;
            }
            if (string.IsNullOrWhiteSpace(settori.settore))
            {
                ModelState.AddModelError(nameof(settori.settore), "Inserisci il nome del settore.");
            }
            if (ModelState.IsValid)
            {
                settori.id = Guid.NewGuid();
                _context.Add(settori);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<SettoriController>>();
                    logger.LogError(ex, "Errore durante la creazione del settore {SectorId}", settori.id);
                    ModelState.AddModelError(string.Empty, "Impossibile salvare il settore. Verifica i dati inseriti e riprova.");
                }
            }
            return View(settori);
        }

        private async Task<Guid?> GetCreateOrganizationAsync()
        {
            var username = UPlant.Services.UserIdentityHelper.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            var organizations = await _context.Users.AsNoTracking()
                .Where(x => x.IsEnabled && x.UnipiUserName == username)
                .Select(x => x.Organizzazione).Distinct().Take(2).ToListAsync();
            if (organizations.Count != 1 || organizations[0] == Guid.Empty)
            {
                return null;
            }
            var organizationId = organizations[0];
            return await _context.Organizzazioni.AnyAsync(x => x.id == organizationId)
                ? organizationId : (Guid?)null;
        }

        // GET: Settori/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Settori == null)
            {
                return NotFound();
            }

            var settori = await _context.Settori.FindAsync(id);
            if (settori == null)
            {
                return NotFound();
            }


            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", settori.organizzazione);
            return View(settori);
        }

        // POST: Settori/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,settore,organizzazione,visualizzazioneweb,ordinamento,settore_en,colore")] Settori settori)
        {
            if (id != settori.id)
            {
                return NotFound();
            }

            if (!await _context.Organizzazioni.AnyAsync(x => x.id == settori.organizzazione))
            {
                ModelState.AddModelError(nameof(settori.organizzazione), "Seleziona un'organizzazione valida.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(settori);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SettoriExists(settori.id))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError(string.Empty, "I dati sono stati modificati da un altro utente. Ricarica la pagina e riprova.");
                }
                catch (DbUpdateException ex)
                {
                    HttpContext.RequestServices.GetRequiredService<ILogger<SettoriController>>()
                        .LogError(ex, "Errore durante l'aggiornamento di Settori {Id}", id);
                    ModelState.AddModelError(string.Empty, "Impossibile aggiornare i dati. Verifica i riferimenti selezionati e riprova.");
                }
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", settori.organizzazione);
            return View(settori);
        }

        // GET: Settori/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Settori == null)
            {
                return NotFound();
            }

            var settori = await _context.Settori
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (settori == null)
            {
                return NotFound();
            }

            return View(settori);
        }

        // POST: Settori/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Settori == null)
            {
                return Problem("Entity set 'Entities.Settori'  is null.");
            }
            var settori = await _context.Settori.FindAsync(id);
            if (settori != null)
            {
                _context.Settori.Remove(settori);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SettoriExists(Guid id)
        {
          return _context.Settori.Any(e => e.id == id);
        }
    }
}
