using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class ModalitaPropagazioneController : BaseController
    {
        private readonly Entities _context;

        public ModalitaPropagazioneController(Entities context)
        {
            _context = context;
        }

        // GET: ModalitaPropagazione
        public async Task<IActionResult> Index()
        {
              return View(await _context.ModalitaPropagazione.Include(a => a.organizzazioneNavigation).Include(a => a.Individui).OrderBy(x => x.ordinamento).ToListAsync());
        }

        // GET: ModalitaPropagazione/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.ModalitaPropagazione == null)
            {
                return NotFound();
            }

            var modalitaPropagazione = await _context.ModalitaPropagazione
                .FirstOrDefaultAsync(m => m.id == id);
            if (modalitaPropagazione == null)
            {
                return NotFound();
            }

            return View(modalitaPropagazione);
        }

        // GET: ModalitaPropagazione/Create
        public IActionResult Create()
        {

            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", oggettoutente.Select(x =>x.Organizzazione).FirstOrDefault());
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(_context.Cartellini.OrderBy(x => x.ordinamento).LastOrDefault().ordinamento);//da il numero successivo anche se stringa se il valore è 1 ,2 se viene espresso in alfabetico per ora da vuoto
            return View();
        }

        // POST: ModalitaPropagazione/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,propagatoModalita,ordinamento,organizzazione")] ModalitaPropagazione modalitaPropagazione)
        {
            
            if (ModelState.IsValid)
            {
                modalitaPropagazione.id = Guid.NewGuid();
                _context.Add(modalitaPropagazione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", modalitaPropagazione.organizzazione);
            return View(modalitaPropagazione);
        }

        // GET: ModalitaPropagazione/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ModalitaPropagazione == null)
            {
                return NotFound();
            }
            
            var modalitaPropagazione = await _context.ModalitaPropagazione.FindAsync(id);
            
            if (modalitaPropagazione == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", modalitaPropagazione.organizzazione);
            return View(modalitaPropagazione);
        }

        // POST: ModalitaPropagazione/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,propagatoModalita,ordinamento,organizzazione")] ModalitaPropagazione modalitaPropagazione)
        {
            if (id != modalitaPropagazione.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modalitaPropagazione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModalitaPropagazioneExists(modalitaPropagazione.id))
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
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", modalitaPropagazione.organizzazione);
            return View(modalitaPropagazione);
        }

        // GET: ModalitaPropagazione/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ModalitaPropagazione == null)
            {
                return NotFound();
            }

            var modalitaPropagazione = await _context.ModalitaPropagazione.Include(a => a.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (modalitaPropagazione == null)
            {
                return NotFound();
            }

            return View(modalitaPropagazione);
        }

        // POST: ModalitaPropagazione/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ModalitaPropagazione == null)
            {
                return Problem("Entity set 'Entities.ModalitaPropagazione'  is null.");
            }
            var modalitaPropagazione = await _context.ModalitaPropagazione.FindAsync(id);
            if (modalitaPropagazione != null)
            {
                _context.ModalitaPropagazione.Remove(modalitaPropagazione);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModalitaPropagazioneExists(Guid id)
        {
          return _context.ModalitaPropagazione.Any(e => e.id == id);
        }
    }
}
