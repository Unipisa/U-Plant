using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    public class SpecieController : BaseController
    {
        private readonly Entities _context;
        private const int MaxPageSize = 100;

        public SpecieController(Entities context)
        {
            _context = context;
        }

        // GET: Specie
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> IndexData()
        {
            var draw = ParseInt(Request.Query["draw"], 1);
            var start = Math.Max(ParseInt(Request.Query["start"], 0), 0);
            var length = ParseInt(Request.Query["length"], 25);
            if (length <= 0)
            {
                length = 25;
            }
            length = Math.Min(length, MaxPageSize);

            var search = Request.Query["search[value]"].FirstOrDefault()?.Trim();
            var orderColumn = ParseInt(Request.Query["order[0][column]"], 0);
            var orderDirection = Request.Query["order[0][dir]"].FirstOrDefault();
            var descending = string.Equals(orderDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var isAdministrator = User.IsInRole("Administrator");

            var query = _context.Specie
                .AsNoTracking()
                .Select(s => new SpecieIndexRow
                {
                    Id = s.id,
                    ScientificName = s.nome_scientifico ?? string.Empty,
                    CommonName = s.nome_comune ?? string.Empty,
                    EnglishCommonName = s.nome_comune_en ?? string.Empty,
                    Family = s.genereNavigation != null && s.genereNavigation.famigliaNavigation != null
                        ? s.genereNavigation.famigliaNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Genus = s.genereNavigation != null
                        ? s.genereNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Kingdom = s.regnoNavigation != null
                        ? s.regnoNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Range = s.arealeNavigation != null
                        ? s.arealeNavigation.descrizione ?? string.Empty
                        : string.Empty,
                    Cites = s.citesNavigation != null
                        ? s.citesNavigation.codice ?? string.Empty
                        : string.Empty,
                    IucnGlobal = s.iucn_globaleNavigation != null
                        ? s.iucn_globaleNavigation.codice ?? string.Empty
                        : string.Empty,
                    IucnLocal = s.iucn_italiaNavigation != null
                        ? s.iucn_italiaNavigation.codice ?? string.Empty
                        : string.Empty,
                    Note = s.note ?? string.Empty,
                    HasAccessioni = s.Accessioni.Any()
                });

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.ScientificName.Contains(search) ||
                    s.CommonName.Contains(search) ||
                    s.EnglishCommonName.Contains(search) ||
                    s.Family.Contains(search) ||
                    s.Genus.Contains(search) ||
                    s.Kingdom.Contains(search) ||
                    s.Range.Contains(search) ||
                    s.Cites.Contains(search) ||
                    s.IucnGlobal.Contains(search) ||
                    s.IucnLocal.Contains(search) ||
                    s.Note.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();

            query = ApplyOrdering(query, orderColumn, descending);

            var page = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            var result = new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = page.Select(item => new
                {
                    id = item.Id,
                    scientificName = item.ScientificName,
                    commonName = item.CommonName,
                    englishCommonName = item.EnglishCommonName,
                    family = item.Family,
                    genus = item.Genus,
                    kingdom = item.Kingdom,
                    range = item.Range,
                    cites = item.Cites,
                    iucnGlobal = item.IucnGlobal,
                    iucnLocal = item.IucnLocal,
                    notePreview = TruncateNote(item.Note),
                    canDelete = !item.HasAccessioni,
                    showActions = isAdministrator
                })
            };

            return new JsonResult(result, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
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

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static string TruncateNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return string.Empty;
            }

            return note.Length > 100
                ? $"{note.Substring(0, 100)} [...]"
                : note;
        }

        private static IQueryable<SpecieIndexRow> ApplyOrdering(IQueryable<SpecieIndexRow> query, int columnIndex, bool descending)
        {
            return columnIndex switch
            {
                1 => descending ? query.OrderByDescending(x => x.CommonName) : query.OrderBy(x => x.CommonName),
                2 => descending ? query.OrderByDescending(x => x.EnglishCommonName) : query.OrderBy(x => x.EnglishCommonName),
                3 => descending ? query.OrderByDescending(x => x.Family) : query.OrderBy(x => x.Family),
                4 => descending ? query.OrderByDescending(x => x.Genus) : query.OrderBy(x => x.Genus),
                5 => descending ? query.OrderByDescending(x => x.Kingdom) : query.OrderBy(x => x.Kingdom),
                6 => descending ? query.OrderByDescending(x => x.Range) : query.OrderBy(x => x.Range),
                7 => descending ? query.OrderByDescending(x => x.Cites) : query.OrderBy(x => x.Cites),
                8 => descending ? query.OrderByDescending(x => x.IucnGlobal) : query.OrderBy(x => x.IucnGlobal),
                9 => descending ? query.OrderByDescending(x => x.IucnLocal) : query.OrderBy(x => x.IucnLocal),
                10 => descending ? query.OrderByDescending(x => x.Note) : query.OrderBy(x => x.Note),
                _ => descending ? query.OrderByDescending(x => x.ScientificName) : query.OrderBy(x => x.ScientificName)
            };
        }

        private sealed class SpecieIndexRow
        {
            public Guid Id { get; set; }
            public string ScientificName { get; set; }
            public string CommonName { get; set; }
            public string EnglishCommonName { get; set; }
            public string Family { get; set; }
            public string Genus { get; set; }
            public string Kingdom { get; set; }
            public string Range { get; set; }
            public string Cites { get; set; }
            public string IucnGlobal { get; set; }
            public string IucnLocal { get; set; }
            public string Note { get; set; }
            public bool HasAccessioni { get; set; }
        }
    }
}
