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
    public class TipoIdentificatoreController : BaseController
    {
        private readonly Entities _context;

        public TipoIdentificatoreController(Entities context)
        {
            _context = context;
        }

        // GET: TipoIdentificatore
        public async Task<IActionResult> Index()
        {
            var entities = _context.TipoIdentificatore.Include(t => t.organizzazioneNavigation).Include(t => t.Identificatori).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
        }

        // GET: TipoIdentificatore/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TipoIdentificatore == null)
            {
                return NotFound();
            }

            var tipoIdentificatore = await _context.TipoIdentificatore
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoIdentificatore == null)
            {
                return NotFound();
            }

            return View(tipoIdentificatore);
        }

        // GET: TipoIdentificatore/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            return View();
        }

        // POST: TipoIdentificatore/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,descrizione")] TipoIdentificatore tipoIdentificatore)
        {
            if (ModelState.IsValid)
            {
                tipoIdentificatore.id = Guid.NewGuid();
                _context.Add(tipoIdentificatore);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoIdentificatore.organizzazione);
            return View(tipoIdentificatore);
        }

        // GET: TipoIdentificatore/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TipoIdentificatore == null)
            {
                return NotFound();
            }

            var tipoIdentificatore = await _context.TipoIdentificatore.FindAsync(id);
            if (tipoIdentificatore == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoIdentificatore.organizzazione);
            return View(tipoIdentificatore);
        }

        // POST: TipoIdentificatore/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,descrizione")] TipoIdentificatore tipoIdentificatore)
        {
            if (id != tipoIdentificatore.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoIdentificatore);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoIdentificatoreExists(tipoIdentificatore.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", tipoIdentificatore.organizzazione);
            return View(tipoIdentificatore);
        }

        // GET: TipoIdentificatore/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TipoIdentificatore == null)
            {
                return NotFound();
            }

            var tipoIdentificatore = await _context.TipoIdentificatore
                .Include(t => t.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoIdentificatore == null)
            {
                return NotFound();
            }

            return View(tipoIdentificatore);
        }

        // POST: TipoIdentificatore/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TipoIdentificatore == null)
            {
                return Problem("Entity set 'Entities.TipoIdentificatore'  is null.");
            }
            var tipoIdentificatore = await _context.TipoIdentificatore.FindAsync(id);
            if (tipoIdentificatore != null)
            {
                _context.TipoIdentificatore.Remove(tipoIdentificatore);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoIdentificatoreExists(Guid id)
        {
          return _context.TipoIdentificatore.Any(e => e.id == id);
        }
    }
}
