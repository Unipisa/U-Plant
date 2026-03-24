using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class StatusNomenclaturaleController : BaseController
    {
        private readonly Entities _context;

        public StatusNomenclaturaleController(Entities context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entities = _context.StatusNomenclaturale
                .Include(s => s.organizzazioneNavigation)
                .Include(s => s.Specie)
                .OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.StatusNomenclaturale == null)
            {
                return NotFound();
            }

            var status = await _context.StatusNomenclaturale
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.FirstOrDefault(c => c.Type == "UnipiUserID")?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == username.Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.StatusNomenclaturale.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] StatusNomenclaturale statusNomenclaturale)
        {
            if (ModelState.IsValid)
            {
                statusNomenclaturale.id = Guid.NewGuid();
                _context.Add(statusNomenclaturale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statusNomenclaturale.organizzazione);
            return View(statusNomenclaturale);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.StatusNomenclaturale == null)
            {
                return NotFound();
            }

            var status = await _context.StatusNomenclaturale.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", status.organizzazione);
            return View(status);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione,descrizione_en,ordinamento")] StatusNomenclaturale statusNomenclaturale)
        {
            if (id != statusNomenclaturale.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statusNomenclaturale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusNomenclaturaleExists(statusNomenclaturale.id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", statusNomenclaturale.organizzazione);
            return View(statusNomenclaturale);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.StatusNomenclaturale == null)
            {
                return NotFound();
            }

            var status = await _context.StatusNomenclaturale
                .Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.StatusNomenclaturale == null)
            {
                return Problem("Entity set 'Entities.StatusNomenclaturale' is null.");
            }

            var status = await _context.StatusNomenclaturale.FindAsync(id);
            if (status != null)
            {
                _context.StatusNomenclaturale.Remove(status);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatusNomenclaturaleExists(Guid id)
        {
            return _context.StatusNomenclaturale.Any(e => e.id == id);
        }
    }
}
