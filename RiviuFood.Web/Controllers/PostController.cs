using Microsoft.AspNetCore.Mvc;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;

namespace RiviuFood.Web.Controllers;

public class PostController(IGenericRepository<Post> postRepo) : Controller
{
    private readonly IGenericRepository<Post> _postRepo = postRepo;

    // Trang chi tiết bài review
    public async Task<IActionResult> Details(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();

        return View(post);
    }

    // Trang tạo bài review mới (Chỉ cho người dùng đã đăng nhập)
    public IActionResult Create()
    {
        return View();
    }
}