using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly ApplicationDbContext _context;

    public UserController(ILogger<UserController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy tất cả dữ liệu và truyền vào ViewBag
        ViewBag.Users = await _context.Users.ToListAsync();
        ViewBag.GameLevels = await _context.GameLevels.ToListAsync();
        ViewBag.Regions = await _context.Regions.ToListAsync();
        ViewBag.Questions = await _context.Questions.ToListAsync();
        ViewBag.Roles = await _context.Roles.ToListAsync();
        ViewBag.GameResults = await _context.GameResults.ToListAsync();

        return View();
    }

    public IActionResult UserPrivacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult UserError()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}