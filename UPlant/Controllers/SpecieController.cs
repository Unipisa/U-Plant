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
    public class SpecieController : BaseController
    {
        private readonly Entities _context;

        public SpecieController(Entities context)
        {
            _context = context;
        }

        // GET: Specie
        public async Task<IActionResult> Index()
        {
            var entities = _context.Specie.Include(s => s.arealeNavigation).Include(s => s.citesNavigation).Include(s => s.genereNavigation).Include(s => s.iucn_globaleNavigation).Include(s => s.iucn_italiaNavigation).Include(s => s.regnoNavigation).Include(s => s.genereNavigation.famigliaNavigation).Include(s => s.Accessioni).OrderBy(x => x.nome_scientifico);
            return View(await entities.ToListAsync());
        }

        // GET: Specie/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie
                .Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (specie == null)
            {
                return NotFound();
            }

            return View(specie);
        }

        // GET: Specie/Create
        public IActionResult Create()
        {
            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(x => x.descrizione), "id", "descrizione");
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(x => x.ordinamento), "id", "codice");
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(x => x.descrizione), "id", "descrizione");
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice");
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(x => x.ordinamento), "id", "codice");
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(x => x.ordinamento), "id", "descrizione");
            return View();
        }

        // POST: Specie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,genere,nome,nome_scientifico,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
        {
            if (ModelState.IsValid)
            {
                specie.id = Guid.NewGuid();
                _context.Add(specie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(a => a.descrizione), "id", "descrizione", specie.areale);
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(a => a.ordinamento), "id", "codice", specie.cites);
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(x => x.descrizione), "id", "descrizione", specie.genere);
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_globale);
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_italia);
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(a => a.ordinamento), "id", "descrizione", specie.regno);
            return View(specie);
        }

        // GET: Specie/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie.FindAsync(id);
            if (specie == null)
            {
                return NotFound();
            }
            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(a => a.descrizione), "id", "descrizione", specie.areale);
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(a => a.ordinamento), "id", "codice", specie.cites);
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(a => a.descrizione), "id", "descrizione", specie.genere);
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_globale);
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_italia);
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(a => a.ordinamento), "id", "descrizione", specie.regno);
            return View(specie);
        }

        // POST: Specie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,genere,nome,nome_scientifico,autori,regno,areale,subspecie,autorisub,varieta,autorivar,cult,autoricult,note,nome_comune,nome_comune_en,iucn_globale,iucn_italia,cites")] Specie specie)
        {
            if (id != specie.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(specie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecieExists(specie.id))
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
            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(a => a.descrizione), "id", "descrizione", specie.areale);
            ViewData["cites"] = new SelectList(_context.Cites.OrderBy(a => a.ordinamento), "id", "codice", specie.cites);
            ViewData["genere"] = new SelectList(_context.Generi.OrderBy(a => a.descrizione), "id", "descrizione", specie.genere);
            ViewData["iucn_globale"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_globale);
            ViewData["iucn_italia"] = new SelectList(_context.Iucn.OrderBy(a => a.ordinamento), "id", "codice", specie.iucn_italia);
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(a => a.ordinamento), "id", "descrizione", specie.regno);
            return View(specie);
        }

        // GET: Specie/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Specie == null)
            {
                return NotFound();
            }

            var specie = await _context.Specie
                .Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (specie == null)
            {
                return NotFound();
            }

            return View(specie);
        }

        // POST: Specie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Specie == null)
            {
                return Problem("Entity set 'Entities.Specie'  is null.");
            }
            var specie = await _context.Specie.FindAsync(id);
            if (specie != null)
            {
                _context.Specie.Remove(specie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpecieExists(Guid id)
        {
          return _context.Specie.Any(e => e.id == id);
        }
    }
}
