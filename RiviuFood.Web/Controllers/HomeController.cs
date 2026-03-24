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
        // Lấy tất cả bài viết, sắp xếp mới nhất lên đầu
        var posts = await _postRepo.GetAllAsync(
            includeProperties: "Restaurant,User"
        );

        return View(posts.OrderByDescending(p => p.CreatedAt));
    }
}