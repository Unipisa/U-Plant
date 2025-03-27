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
    public class CartelliniController : Controller
    {
        private readonly Entities _context;

        public CartelliniController(Entities context)
        {
            _context = context;
        }

        // GET: Cartellini
        public async Task<IActionResult> Index()
        {
            var entities = _context.Cartellini.Include(c => c.organizzazioneNavigation).Include(a => a.Individui).OrderBy(x => x.ordinamento);
            return View(await entities.ToListAsync());
        }

        // GET: Cartellini/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Cartellini == null)
            {
                return NotFound();
            }

            var cartellini = await _context.Cartellini
                .Include(c => c.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (cartellini == null)
            {
                return NotFound();
            }

            return View(cartellini);
        }

        // GET: Cartellini/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x => x.Organizzazione).FirstOrDefault());
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Cartellini.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto


            return View();
        }

        // POST: Cartellini/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,ordinamento,organizzazione")] Cartellini cartellini)
        {
            if (ModelState.IsValid)
            {
                cartellini.id = Guid.NewGuid();
                _context.Add(cartellini);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", cartellini.organizzazione);
            return View(cartellini);
        }

        // GET: Cartellini/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Cartellini == null)
            {
                return NotFound();
            }

            var cartellini = await _context.Cartellini.FindAsync(id);
            if (cartellini == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", cartellini.organizzazione);
            return View(cartellini);
        }

        // POST: Cartellini/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,ordinamento,organizzazione")] Cartellini cartellini)
        {
            if (id != cartellini.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartellini);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartelliniExists(cartellini.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", cartellini.organizzazione);
            return View(cartellini);
        }

        // GET: Cartellini/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Cartellini == null)
            {
                return NotFound();
            }

            var cartellini = await _context.Cartellini
                .Include(c => c.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (cartellini == null)
            {
                return NotFound();
            }

            return View(cartellini);
        }

        // POST: Cartellini/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Cartellini == null)
            {
                return Problem("Entity set 'Entities.Cartellini'  is null.");
            }
            var cartellini = await _context.Cartellini.FindAsync(id);
            if (cartellini != null)
            {
                _context.Cartellini.Remove(cartellini);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartelliniExists(Guid id)
        {
          return _context.Cartellini.Any(e => e.id == id);
        }
    }
}
