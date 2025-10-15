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
    public class IdentificatoriController : BaseController
    {
        private readonly Entities _context;

        public IdentificatoriController(Entities context)
        {
            _context = context;
        }

        // GET: Identificatori
        public async Task<IActionResult> Index()
        {
            var entities = _context.Identificatori.Include(v => v.organizzazioneNavigation).Include(v => v.tipoIdentificatoreNavigation).Include(t => t.Accessioni).OrderBy(x => x.nominativo);
            return View(await entities.ToListAsync());
        }

        // GET: Identificatori/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Identificatori == null)
            {
                return NotFound();
            }

            var identificatori = await _context.Identificatori
                .Include(v => v.organizzazioneNavigation)
                .Include(v => v.tipoIdentificatoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (identificatori == null)
            {
                return NotFound();
            }

            return View(identificatori);
        }

        // GET: Identificatori/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["tipoIdentificatore"] = new SelectList(_context.TipoIdentificatore, "id", "descrizione");
            return View();
        }

        // POST: Identificatori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,organizzazione,nominativo,nominativo_en,tipoIdentificatore,attivo")] Identificatori identificatori)
        {
            if (ModelState.IsValid)
            {
                identificatori.id = Guid.NewGuid();
                _context.Add(identificatori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", identificatori.organizzazione);
            ViewData["TipoIdentificatore"] = new SelectList(_context.TipoIdentificatore, "id", "descrizione", identificatori.tipoIdentificatore);
            return View(identificatori);
        }

        // GET: Identificatori/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Identificatori == null)
            {
                return NotFound();
            }

            var identificatori = await _context.Identificatori.FindAsync(id);
            if (identificatori == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", identificatori.organizzazione);
            ViewData["tipoIdentificatore"] = new SelectList(_context.TipoIdentificatore, "id", "descrizione", identificatori.tipoIdentificatore);
            return View(identificatori);
        }

        // POST: Verificatori/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,organizzazione,nominativo,nominativo_en,tipoIdentificatore,attivo")] Identificatori identificatori)
        {
            if (id != identificatori.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(identificatori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IdentificatoriExists(identificatori.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", identificatori.organizzazione);
            ViewData["tipoIdentificatore"] = new SelectList(_context.TipoIdentificatore, "id", "descrizione", identificatori.tipoIdentificatore);
            return View(identificatori);
        }

        // GET: Identificatori/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Identificatori == null)
            {
                return NotFound();
            }

            var identificatori = await _context.Identificatori
                .Include(v => v.organizzazioneNavigation)
                .Include(v => v.tipoIdentificatoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (identificatori == null)
            {
                return NotFound();
            }

            return View(identificatori);
        }

        // POST: Verificatori/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Identificatori == null)
            {
                return Problem("Entity set 'Entities.Identificatori'  is null.");
            }
            var identificatori = await _context.Identificatori.FindAsync(id);
            if (identificatori != null)
            {
                _context.Identificatori.Remove(identificatori);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IdentificatoriExists(Guid id)
        {
          return _context.Identificatori.Any(e => e.id == id);
        }
    }
}
