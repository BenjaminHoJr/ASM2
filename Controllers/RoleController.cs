using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RoleController : Controller
    {
        // GET: RoleController
        private ApplicationDbContext _context;
        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }
        public ActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }
        // GET: Role/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Role/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleId == id.Value);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Role/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.roleId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            var existing = await _context.Roles.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = role.RoleName;
            existing.Description = role.Description;

            try
            {
                _context.Roles.Update(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Role updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Roles.Any(e => e.roleId == id))
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

        // GET: Role/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleId == id.Value);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Role/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            try
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Role deleted successfully.";
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete role. It may be referenced by other data.");
                return View(role);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
