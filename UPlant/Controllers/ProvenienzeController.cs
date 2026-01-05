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
    public class ProvenienzeController : BaseController
    {
        private readonly Entities _context;

        public ProvenienzeController(Entities context)
        {
            _context = context;
        }

        // GET: Provenienze
        public async Task<IActionResult> Index()
        {
            var entities = _context.Provenienze.Include(p => p.organizzazioneNavigation).Include(t => t.Accessioni).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: Provenienze/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Provenienze == null)
            {
                return NotFound();
            }

            var provenienze = await _context.Provenienze
                .Include(p => p.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (provenienze == null)
            {
                return NotFound();
            }

            return View(provenienze);
        }

        // GET: Provenienze/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            var ultimo = _context.Provenienze.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimo);
           return View();
        }

        // POST: Provenienze/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en,ordinamento,organizzazione")] Provenienze provenienze)
        {
            if (ModelState.IsValid)
            {
                provenienze.id = Guid.NewGuid();
                _context.Add(provenienze);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", provenienze.organizzazione);
            return View(provenienze);
        }

        // GET: Provenienze/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Provenienze == null)
            {
                return NotFound();
            }

            var provenienze = await _context.Provenienze.FindAsync(id);
            if (provenienze == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", provenienze.organizzazione);
            return View(provenienze);
        }

        // POST: Provenienze/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en,ordinamento,organizzazione")] Provenienze provenienze)
        {
            if (id != provenienze.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(provenienze);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvenienzeExists(provenienze.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", provenienze.organizzazione);
            return View(provenienze);
        }

        // GET: Provenienze/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Provenienze == null)
            {
                return NotFound();
            }

            var provenienze = await _context.Provenienze
                .Include(p => p.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (provenienze == null)
            {
                return NotFound();
            }

            return View(provenienze);
        }

        // POST: Provenienze/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Provenienze == null)
            {
                return Problem("Entity set 'Entities.Provenienze'  is null.");
            }
            var provenienze = await _context.Provenienze.FindAsync(id);
            if (provenienze != null)
            {
                _context.Provenienze.Remove(provenienze);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvenienzeExists(Guid id)
        {
          return _context.Provenienze.Any(e => e.id == id);
        }
    }
}
