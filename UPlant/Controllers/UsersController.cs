using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;
using UPlant.Models;


namespace UPlant.Controllers
{
    public class UsersController : BaseController
    {
        private readonly Entities _context;
        private readonly LanguageService _languageService;
        public UsersController(Entities context, LanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var entities = _context.UserRole.Include(u => u.UserFKNavigation).ThenInclude(u => u.AccessioniutenteUltimaModificaNavigation).Include(u => u.UserFKNavigation).ThenInclude(u => u.OrganizzazioneNavigation).Include(u => u.UserFKNavigation).ThenInclude(u => u.TipologiaUtenteNavigation).Include(u => u.RoleFKNavigation).OrderBy(u => u.UserFKNavigation.LastName);
            return View(await entities.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .Include(u => u.OrganizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["Organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione");
            ViewData["Ruolo"] = new SelectList(_context.Roles.OrderBy(x =>x.Descr), "Id", "Descr");
            ViewData["TipologiaUtente"] = new SelectList(_context.TipologiaUtente.OrderBy(x => x.descrizione), "id", "descrizione");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid ruolo,[Bind("Id,Name,LastName,Email,UnipiUserName,CF,IsEnabled,CreatedAt,CreatedBy,CreatedFrom,TipologiaUtente,Organizzazione")] Users users)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            string cognomecreatore = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "family_name").FirstOrDefault()?.Value;
            string nomecreatore = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "given_name").FirstOrDefault()?.Value;
          



            
            
            string hostName = Dns.GetHostName();
            string Ip = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            


                var utenteesistente = _context.Users.Where(a => a.UnipiUserName == users.UnipiUserName);
            if (utenteesistente.Count() >0)
            {
                ///avverti che stai inserendo un nominativo già presente
                ///
              

                var msg = _languageService.Getkey("UsersController_1").Value;
                AddPageAlerts(PageAlertType.Warning, msg); 
                TempData["message"] = msg;
                
               
               
                // return RedirectToAction(nameof(Index));
                ViewData["Organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", users.Organizzazione);
                ViewData["Ruolo"] = new SelectList(_context.Roles.OrderBy(x => x.Descr), "Id", "Descr");
                ViewData["TipologiaUtente"] = new SelectList(_context.TipologiaUtente.OrderBy(x => x.descrizione), "id", "descrizione", users.TipologiaUtente);
                return View(users);
            }
            if (ModelState.IsValid)
            {


                users.Id = Guid.NewGuid();
                users.CreatedBy = nomecreatore + " " + cognomecreatore;
                users.CreatedFrom = Ip;

                users.UserRole = new List<UserRole>
                {
                    new UserRole
                    {
                    Id= Guid.NewGuid(),
                    UserFK = users.Id,
                    RoleFK = ruolo // devo mettere i valori giusti passati dal form
                    }
                };
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", users.Organizzazione);
            ViewData["Ruolo"] = new SelectList(_context.Roles.OrderBy(x => x.Descr), "Id", "Descr");
            ViewData["TipologiaUtente"] = new SelectList(_context.TipologiaUtente.OrderBy(x => x.descrizione), "id", "descrizione", users.TipologiaUtente);
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users.FindAsync(id);
            var userrole = _context.UserRole.Where(c => c.UserFK == users.Id).FirstOrDefault();
            if (users == null)
            {
                return NotFound();
            }
            ViewData["Organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", users.Organizzazione);
            ViewData["Ruolo"] = new SelectList(_context.Roles.OrderBy(x => x.Descr), "Id", "Descr", userrole.RoleFK);
            ViewData["TipologiaUtente"] = new SelectList(_context.TipologiaUtente.OrderBy(x => x.descrizione), "id", "descrizione", users.TipologiaUtente);
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Guid ruolo, [Bind("Id,Name,LastName,Email,UnipiUserName,CF,IsEnabled,CreatedAt,CreatedBy,CreatedFrom,TipologiaUtente,Organizzazione")] Users users)
        {
            
            if (id != users.Id)
            {
                return NotFound();
            }
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            string cognomecreatore = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "family_name").FirstOrDefault()?.Value;
            string nomecreatore = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "given_name").FirstOrDefault()?.Value;
            string hostName = Dns.GetHostName();
            string Ip = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            UserRole userrole = _context.UserRole.AsNoTracking().Where(c => c.UserFK == id).FirstOrDefault();
            if (ModelState.IsValid)
            {
                try
                {
                    users.CreatedBy = nomecreatore + " " + cognomecreatore;
                    users.CreatedFrom = Ip;
                    
                    users.CreatedAt = DateTime.Now;
                    _context.Update(users);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
               
                try
                {
                    userrole.RoleFK = ruolo;
                    _context.Update(userrole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.Id))
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
            ViewData["Organizzazione"] = new SelectList(_context.Organizzazioni.OrderBy(x => x.descrizione), "id", "descrizione", users.Organizzazione);
            ViewData["Ruolo"] = new SelectList(_context.Roles.OrderBy(x => x.Descr), "Id", "Descr", userrole.RoleFK);
            ViewData["TipologiaUtente"] = new SelectList(_context.TipologiaUtente.OrderBy(x => x.descrizione), "id", "descrizione", users.TipologiaUtente);
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .Include(u => u.OrganizzazioneNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'Entities.Users'  is null.");
            }

            var users = await _context.Users.FindAsync(id);
            var userrole =  _context.UserRole.Where(c => c.UserFK == users.Id).FirstOrDefault();

            if (users != null)
            {
                _context.UserRole.Remove(userrole);
                _context.Users.Remove(users);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(Guid id)
        {
          return _context.Users.Any(e => e.Id == id);
        }
    }
}
