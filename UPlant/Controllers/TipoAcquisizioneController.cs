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
    public class TipoAcquisizioneController : BaseController
    {
        private readonly Entities _context;

        public TipoAcquisizioneController(Entities context)
        {
            _context = context;
        }

        // GET: TipoAcquisizione
        public async Task<IActionResult> Index()
        {
            var entities = _context.TipoAcquisizione.Include(t => t.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: TipoAcquisizione/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TipoAcquisizione == null)
            {
                return NotFound();
            }

            var tipoAcquisizione = await _context.TipoAcquisizione
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoAcquisizione == null)
            {
                return NotFound();
            }

            return View(tipoAcquisizione);
        }

        // GET: TipoAcquisizione/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.TipoAcquisizione.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
             return View();
        }

        // POST: TipoAcquisizione/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,organizzazione,descrizione_en,ordinamento")] TipoAcquisizione tipoAcquisizione)
        {
            if (ModelState.IsValid)
            {
                tipoAcquisizione.id = Guid.NewGuid();
                _context.Add(tipoAcquisizione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoAcquisizione.organizzazione);
            return View(tipoAcquisizione);
        }

        // GET: TipoAcquisizione/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TipoAcquisizione == null)
            {
                return NotFound();
            }

            var tipoAcquisizione = await _context.TipoAcquisizione.FindAsync(id);
            if (tipoAcquisizione == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoAcquisizione.organizzazione);
            return View(tipoAcquisizione);
        }

        // POST: TipoAcquisizione/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,organizzazione,descrizione_en,ordinamento")] TipoAcquisizione tipoAcquisizione)
        {
            if (id != tipoAcquisizione.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoAcquisizione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoAcquisizioneExists(tipoAcquisizione.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoAcquisizione.organizzazione);
            return View(tipoAcquisizione);
        }

        // GET: TipoAcquisizione/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TipoAcquisizione == null)
            {
                return NotFound();
            }

            var tipoAcquisizione = await _context.TipoAcquisizione
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoAcquisizione == null)
            {
                return NotFound();
            }

            return View(tipoAcquisizione);
        }

        // POST: TipoAcquisizione/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TipoAcquisizione == null)
            {
                return Problem("Entity set 'Entities.TipoAcquisizione'  is null.");
            }
            var tipoAcquisizione = await _context.TipoAcquisizione.FindAsync(id);
            if (tipoAcquisizione != null)
            {
                _context.TipoAcquisizione.Remove(tipoAcquisizione);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoAcquisizioneExists(Guid id)
        {
          return _context.TipoAcquisizione.Any(e => e.id == id);
        }
    }
}
