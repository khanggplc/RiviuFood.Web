using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Data;
using RiviuFood.Web.Models;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;

namespace RiviuFood.Web.Controllers;

public class HomeController(IGenericRepository<Post> postRepo) : Controller
{
    private readonly IGenericRepository<Post> _postRepo = postRepo;

    public async Task<IActionResult> Index()
    {
        // Sử dụng Repository để lấy dữ liệu
        var posts = await _postRepo.GetAllAsync();
        return View(posts.OrderByDescending(p => p.CreatedAt));
    }
}