using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class ValidazioneTassonomicaController : BaseController
    {
        private readonly Entities _context;

        public ValidazioneTassonomicaController(Entities context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entities = _context.ValidazioneTassonomica
                .Include(s => s.organizzazioneNavigation)
                .Include(s => s.Specie)
                .OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.ValidazioneTassonomica == null)
            {
                return NotFound();
            }

            var validazioneTassonomica = await _context.ValidazioneTassonomica
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (validazioneTassonomica == null)
            {
                return NotFound();
            }

            return View(validazioneTassonomica);
        }

        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.FirstOrDefault(c => c.Type == "UnipiUserID")?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == username.Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.ValidazioneTassonomica.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] ValidazioneTassonomica validazioneTassonomica)
        {
            if (ModelState.IsValid)
            {
                validazioneTassonomica.id = Guid.NewGuid();
                _context.Add(validazioneTassonomica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", validazioneTassonomica.organizzazione);
            return View(validazioneTassonomica);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ValidazioneTassonomica == null)
            {
                return NotFound();
            }

            var validazioneTassonomica = await _context.ValidazioneTassonomica.FindAsync(id);
            if (validazioneTassonomica == null)
            {
                return NotFound();
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", validazioneTassonomica.organizzazione);
            return View(validazioneTassonomica);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] ValidazioneTassonomica validazioneTassonomica)
        {
            if (id != validazioneTassonomica.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(validazioneTassonomica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ValidazioneTassonomicaExists(validazioneTassonomica.id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", validazioneTassonomica.organizzazione);
            return View(validazioneTassonomica);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ValidazioneTassonomica == null)
            {
                return NotFound();
            }

            var validazioneTassonomica = await _context.ValidazioneTassonomica
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (validazioneTassonomica == null)
            {
                return NotFound();
            }

            return View(validazioneTassonomica);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ValidazioneTassonomica == null)
            {
                return Problem("Entity set 'Entities.ValidazioneTassonomica' is null.");
            }

            var validazioneTassonomica = await _context.ValidazioneTassonomica.FindAsync(id);
            if (validazioneTassonomica != null)
            {
                _context.ValidazioneTassonomica.Remove(validazioneTassonomica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ValidazioneTassonomicaExists(Guid id)
        {
            return _context.ValidazioneTassonomica.Any(e => e.id == id);
        }
    }
}

