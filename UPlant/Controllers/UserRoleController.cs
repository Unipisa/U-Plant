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
    public class UserRoleController : BaseController
    {
        private readonly Entities _context;

        public UserRoleController(Entities context)
        {
            _context = context;
        }

        // GET: UserRole
        public async Task<IActionResult> Index()
        {
            var entities = _context.UserRole.Include(u => u.RoleFKNavigation).Include(u => u.UserFKNavigation).OrderBy(x => x.UserFKNavigation.LastName);
            return View(await entities.ToListAsync());
        }

        // GET: UserRole/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.UserRole == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRole
                .Include(u => u.RoleFKNavigation)
                .Include(u => u.UserFKNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userRole == null)
            {
                return NotFound();
            }

            return View(userRole);
        }

        // GET: UserRole/Create
        public IActionResult Create()
        {
            ViewData["RoleFK"] = new SelectList(_context.Roles, "Id", "Descr");
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Name");
            return View();
        }

        // POST: UserRole/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserFK,RoleFK")] UserRole userRole)
        {
            if (ModelState.IsValid)
            {
                userRole.Id = Guid.NewGuid();
                _context.Add(userRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleFK"] = new SelectList(_context.Roles, "Id", "Descr", userRole.RoleFK);
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Name", userRole.UserFK);
            return View(userRole);
        }

        // GET: UserRole/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.UserRole == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRole.FindAsync(id);
            if (userRole == null)
            {
                return NotFound();
            }
            ViewData["RoleFK"] = new SelectList(_context.Roles, "Id", "Descr", userRole.RoleFK);
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Name", userRole.UserFK);
            return View(userRole);
        }

        // POST: UserRole/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UserFK,RoleFK")] UserRole userRole)
        {
            if (id != userRole.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userRole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRoleExists(userRole.Id))
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
            ViewData["RoleFK"] = new SelectList(_context.Roles, "Id", "Descr", userRole.RoleFK);
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Name", userRole.UserFK);
            return View(userRole);
        }

        // GET: UserRole/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.UserRole == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRole
                .Include(u => u.RoleFKNavigation)
                .Include(u => u.UserFKNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userRole == null)
            {
                return NotFound();
            }

            return View(userRole);
        }

        // POST: UserRole/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.UserRole == null)
            {
                return Problem("Entity set 'Entities.UserRole'  is null.");
            }
            var userRole = await _context.UserRole.FindAsync(id);
            if (userRole != null)
            {
                _context.UserRole.Remove(userRole);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserRoleExists(Guid id)
        {
          return _context.UserRole.Any(e => e.Id == id);
        }
    }
}
