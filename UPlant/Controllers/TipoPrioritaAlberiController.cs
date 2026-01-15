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
    public class TipoPrioritaAlberiController : Controller
    {
        private readonly Entities _context;

        public TipoPrioritaAlberiController(Entities context)
        {
            _context = context;
        }

        // GET: TipoPrioritaAlberi
        public async Task<IActionResult> Index()
        {
            var entities = _context.TipoPrioritaAlberi.Include(s => s.organizzazioneNavigation).Include(a => a.Alberi).OrderBy(x => x.descrizione);
            return View(await entities.ToListAsync());
           
        }

        // GET: TipoPrioritaAlberi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPrioritaAlberi = await _context.TipoPrioritaAlberi.Include(s => s.organizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoPrioritaAlberi == null)
            {
                return NotFound();
            }

            return View(tipoPrioritaAlberi);
        }

        // GET: TipoPrioritaAlberi/Create
        public IActionResult Create()
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            ViewData["organizzazione"] = oggettoutente.Select(x => x.Organizzazione).FirstOrDefault();
            var ultimoord = _context.TipoPrioritaAlberi.Max(x => (int?)x.ordinamento);
            ViewData["ordinesuccessivo"] = StaticUtils.GeneraSuccessivo(ultimoord);
            var ultimoliv = _context.TipoPrioritaAlberi.Max(x => (int?)x.livello);
            ViewData["livellosuccessivo"] = StaticUtils.GeneraSuccessivo(ultimoliv);
            return View();
        }

        // POST: TipoPrioritaAlberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en,organizzazione,ordinamento,livello")] TipoPrioritaAlberi tipoPrioritaAlberi)
        {
            if (ModelState.IsValid)
            {

                tipoPrioritaAlberi.id = Guid.NewGuid();
                _context.Add(tipoPrioritaAlberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["organizzazione"] = tipoPrioritaAlberi.organizzazione;
            return View(tipoPrioritaAlberi);
        }

        // GET: TipoPrioritaAlberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPrioritaAlberi = await _context.TipoPrioritaAlberi.FindAsync(id);
            if (tipoPrioritaAlberi == null)
            {
                return NotFound();
            }
            ViewData["organizzazione"] = tipoPrioritaAlberi.organizzazione;
     
            return View(tipoPrioritaAlberi);
        }

        // POST: TipoPrioritaAlberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en,organizzazione,ordinamento,livello")] TipoPrioritaAlberi tipoPrioritaAlberi)
        {
            if (id != tipoPrioritaAlberi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoPrioritaAlberi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoPrioritaAlberiExists(tipoPrioritaAlberi.id))
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
            ViewData["organizzazione"] = tipoPrioritaAlberi.organizzazione;
            return View(tipoPrioritaAlberi);
        }

        // GET: TipoPrioritaAlberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPrioritaAlberi = await _context.TipoPrioritaAlberi
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoPrioritaAlberi == null)
            {
                return NotFound();
            }

            return View(tipoPrioritaAlberi);
        }

        // POST: TipoPrioritaAlberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tipoPrioritaAlberi = await _context.TipoPrioritaAlberi.FindAsync(id);
            if (tipoPrioritaAlberi != null)
            {
                _context.TipoPrioritaAlberi.Remove(tipoPrioritaAlberi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoPrioritaAlberiExists(Guid id)
        {
            return _context.TipoPrioritaAlberi.Any(e => e.id == id);
        }
    }
}
