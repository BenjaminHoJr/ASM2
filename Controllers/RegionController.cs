using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RegionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Region
        public async Task<IActionResult> Index()
        {
            var regions = await _context.Regions.ToListAsync();
            return View(regions);
        }

        // GET: Region/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Region/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Regions.Add(region);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Region created.";
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Region/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            var region = await _context.Regions.FirstOrDefaultAsync(r => r.regionId == id.Value);
            if (region == null) return NotFound();
            return View(region);
        }

        // POST: Region/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Region region)
        {
            if (id != region.regionId) return BadRequest();
            if (!ModelState.IsValid) return View(region);

            var existing = await _context.Regions.FindAsync(id);
            if (existing == null) return NotFound();

            existing.regionName = region.regionName;

            try
            {
                _context.Regions.Update(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Region updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Regions.Any(e => e.regionId == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Region/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var region = await _context.Regions.FirstOrDefaultAsync(r => r.regionId == id.Value);
            if (region == null) return NotFound();
            return View(region);
        }

        // POST: Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null) return NotFound();

            try
            {
                _context.Regions.Remove(region);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Region deleted.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Unable to delete region. It may be referenced by other data.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
