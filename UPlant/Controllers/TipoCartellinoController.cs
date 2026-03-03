using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class TipoCartellinoController : Controller
    {
        private readonly Entities _context;

        public TipoCartellinoController(Entities context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoCartellino.Include(a => a.Individui).OrderBy(x => x.ordinamento).ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TipoCartellino == null)
            {
                return NotFound();
            }

            var tipoCartellino = await _context.TipoCartellino.FirstOrDefaultAsync(m => m.id == id);
            if (tipoCartellino == null)
            {
                return NotFound();
            }

            return View(tipoCartellino);
        }

        public IActionResult Create()
        {
            var ultimo = _context.TipoCartellino.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en,ordinamento")] TipoCartellino tipoCartellino)
        {
            if (ModelState.IsValid)
            {
                tipoCartellino.id = Guid.NewGuid();
                _context.Add(tipoCartellino);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoCartellino);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TipoCartellino == null)
            {
                return NotFound();
            }

            var tipoCartellino = await _context.TipoCartellino.FindAsync(id);
            if (tipoCartellino == null)
            {
                return NotFound();
            }
            return View(tipoCartellino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en,ordinamento")] TipoCartellino tipoCartellino)
        {
            if (id != tipoCartellino.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoCartellino);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoCartellinoExists(tipoCartellino.id))
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
            return View(tipoCartellino);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TipoCartellino == null)
            {
                return NotFound();
            }

            var tipoCartellino = await _context.TipoCartellino.FirstOrDefaultAsync(m => m.id == id);
            if (tipoCartellino == null)
            {
                return NotFound();
            }

            return View(tipoCartellino);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TipoCartellino == null)
            {
                return Problem("Entity set 'Entities.TipoCartellino' is null.");
            }
            var tipoCartellino = await _context.TipoCartellino.FindAsync(id);
            if (tipoCartellino != null)
            {
                _context.TipoCartellino.Remove(tipoCartellino);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoCartellinoExists(Guid id)
        {
            return _context.TipoCartellino.Any(e => e.id == id);
        }
    }
}
