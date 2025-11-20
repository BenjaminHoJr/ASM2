namespace WebApplication1.Controllers;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Region)
            .ToListAsync();
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Region)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return NotFound();

        return user;
    }

    // POST: api/User
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        // nếu client gửi nested Role/Region, mark state Unchanged
        if (user.Role != null)
            _context.Entry(user.Role).State = EntityState.Unchanged;
        if (user.Region != null)
            _context.Entry(user.Region).State = EntityState.Unchanged;

        // nếu client gửi RoleId, đảm bảo tham chiếu đúng
        if (user.RoleId.HasValue && user.Role == null)
        {
            var existingRole = await _context.Roles.FindAsync(user.RoleId.Value);
            if (existingRole != null)
                user.Role = existingRole;
        }

        if (user.RegionId.HasValue && user.Region == null)
        {
            var existingRegion = await _context.Regions.FindAsync(user.RegionId.Value);
            if (existingRegion != null)
                user.Region = existingRegion;
        }

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.UserId)
            return BadRequest();

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}