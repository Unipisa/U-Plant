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
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Settori.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto
            return View();
        }

        // POST: Settori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,settore,organizzazione,visualizzazioneweb,ordinamento,settore_en")] Settori settori)
        {
            if (ModelState.IsValid)
            {
                settori.id = Guid.NewGuid();
                _context.Add(settori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", settori.organizzazione);
            return View(settori);
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
        public async Task<IActionResult> Edit(Guid id, [Bind("id,settore,organizzazione,visualizzazioneweb,ordinamento,settore_en")] Settori settori)
        {
            if (id != settori.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(settori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SettoriExists(settori.id))
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
