using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Data;
using RiviuFood.Web.Models;

namespace RiviuFood.Web.Controllers;

public class HomeController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {

        var posts = await _context.Posts
            .Include(p => p.Restaurant)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(posts);
    }
}