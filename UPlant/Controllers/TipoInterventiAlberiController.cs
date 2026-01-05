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
    public class TipoInterventiAlberiController : Controller
    {
        private readonly Entities _context;

        public TipoInterventiAlberiController(Entities context)
        {
            _context = context;
        }

        // GET: TipoInterventiAlberi
        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoInterventiAlberi.ToListAsync());
        }

        // GET: TipoInterventiAlberi/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoInterventiAlberi = await _context.TipoInterventiAlberi
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoInterventiAlberi == null)
            {
                return NotFound();
            }

            return View(tipoInterventiAlberi);
        }

        // GET: TipoInterventiAlberi/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoInterventiAlberi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,descrizione,descrizione_en,organizzazione,ordinamento")] TipoInterventiAlberi tipoInterventiAlberi)
        {
            if (ModelState.IsValid)
            {
                tipoInterventiAlberi.id = Guid.NewGuid();
                _context.Add(tipoInterventiAlberi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoInterventiAlberi);
        }

        // GET: TipoInterventiAlberi/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoInterventiAlberi = await _context.TipoInterventiAlberi.FindAsync(id);
            if (tipoInterventiAlberi == null)
            {
                return NotFound();
            }
            return View(tipoInterventiAlberi);
        }

        // POST: TipoInterventiAlberi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("id,descrizione,descrizione_en,organizzazione,ordinamento")] TipoInterventiAlberi tipoInterventiAlberi)
        {
            if (id != tipoInterventiAlberi.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoInterventiAlberi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoInterventiAlberiExists(tipoInterventiAlberi.id))
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
            return View(tipoInterventiAlberi);
        }

        // GET: TipoInterventiAlberi/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoInterventiAlberi = await _context.TipoInterventiAlberi
                .FirstOrDefaultAsync(m => m.id == id);
            if (tipoInterventiAlberi == null)
            {
                return NotFound();
            }

            return View(tipoInterventiAlberi);
        }

        // POST: TipoInterventiAlberi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tipoInterventiAlberi = await _context.TipoInterventiAlberi.FindAsync(id);
            if (tipoInterventiAlberi != null)
            {
                _context.TipoInterventiAlberi.Remove(tipoInterventiAlberi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoInterventiAlberiExists(Guid id)
        {
            return _context.TipoInterventiAlberi.Any(e => e.id == id);
        }
    }
}
