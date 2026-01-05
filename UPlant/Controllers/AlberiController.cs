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
    public class AlberiController : Controller
    {
        private readonly Entities _context;

        public AlberiController(Entities context)
        {
            _context = context;
        }

        // GET: Alberi
        public async Task<IActionResult> Index()
        {
            var entities = _context.Alberi.Include(a => a.fornitoreNavigation).Include(a => a.individuoNavigation).Include(a => a.interventoNavigation).Include(a => a.prioritaNavigation).Include(a => a.utenteaperturaNavigation).Include(a => a.utenteultimamodificaNavigation);
            return View(await entities.ToListAsync());
        }

        // GET: Alberi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (alberi == null)
            {
                return NotFound();
            }

            return View(alberi);
        }

        // GET: Alberi/Create
        public IActionResult Create()
        {
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione");
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo");
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione");
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione");
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF");
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF");
            return View();
        }

        // POST: Alberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,datachiusura,utenteapertura,utenteultimamodifica")] Alberi alberi)
        {
            if (ModelState.IsValid)
            {
                alberi.id = Guid.NewGuid();
                _context.Add(alberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // GET: Alberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi.FindAsync(id);
            if (alberi == null)
            {
                return NotFound();
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // POST: Alberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,individuo,dataapertura,priorita,intervento,fornitore,motivo,esitointervento,stato,datachiusura,utenteapertura,utenteultimamodifica")] Alberi alberi)
        {
            if (id != alberi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alberi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlberiExists(alberi.id))
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
            ViewData["fornitore"] = new SelectList(_context.Fornitori, "id", "descrizione", alberi.fornitore);
            ViewData["individuo"] = new SelectList(_context.Individui, "id", "progressivo", alberi.individuo);
            ViewData["intervento"] = new SelectList(_context.TipoInterventiAlberi, "id", "descrizione", alberi.intervento);
            ViewData["priorita"] = new SelectList(_context.TipoPrioritaAlberi, "id", "descrizione", alberi.priorita);
            ViewData["utenteapertura"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteapertura);
            ViewData["utenteultimamodifica"] = new SelectList(_context.Users, "Id", "CF", alberi.utenteultimamodifica);
            return View(alberi);
        }

        // GET: Alberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alberi = await _context.Alberi
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.individuoNavigation)
                .Include(a => a.interventoNavigation)
                .Include(a => a.prioritaNavigation)
                .Include(a => a.utenteaperturaNavigation)
                .Include(a => a.utenteultimamodificaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (alberi == null)
            {
                return NotFound();
            }

            return View(alberi);
        }

        // POST: Alberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var alberi = await _context.Alberi.FindAsync(id);
            if (alberi != null)
            {
                _context.Alberi.Remove(alberi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlberiExists(Guid id)
        {
            return _context.Alberi.Any(e => e.id == id);
        }
    }
}
