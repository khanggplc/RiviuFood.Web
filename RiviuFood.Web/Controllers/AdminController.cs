using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiviuFood.Web.Controllers;

// Đảm bảo chỉ tài khoản có Role = "Admin" mới được phép truy cập
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IGenericRepository<Post> _postRepo;
    private readonly IGenericRepository<Restaurant> _restaurantRepo;
    private readonly IGenericRepository<Comment> _commentRepo;

    // Sử dụng hàm khởi tạo chuẩn để tiêm các Repository vào Controller
    public AdminController(
        IGenericRepository<Post> postRepo,
        IGenericRepository<Restaurant> restaurantRepo,
        IGenericRepository<Comment> commentRepo)
    {
        _postRepo = postRepo;
        _restaurantRepo = restaurantRepo;
        _commentRepo = commentRepo;
    }

    // 1. Trang Dashboard tổng quan (Thống kê số liệu)
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var allPosts = await _postRepo.GetAllAsync();
        var allRestaurants = await _restaurantRepo.GetAllAsync();
        var allComments = await _commentRepo.GetAllAsync();

        ViewBag.TotalPosts = allPosts.Count();
        ViewBag.TotalRestaurants = allRestaurants.Count();
        ViewBag.TotalComments = allComments.Count();

        var topRecentPosts = allPosts
            .Cast<Post>()
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();

        return View(topRecentPosts);
    }

    // 2. Trang danh sách Quản lý Quán ăn
    [HttpGet]
    public async Task<IActionResult> Restaurants()
    {
        var restaurants = await _restaurantRepo.GetAllAsync();
        var result = restaurants.Cast<Restaurant>().OrderByDescending(r => r.CreatedAt).ToList();
        return View(result);
    }

    // 3. Xử lý Thêm quán ăn mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRestaurant(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
        {
            TempData["Error"] = "Tên quán và địa chỉ không được để trống.";
            return RedirectToAction(nameof(Restaurants));
        }

        var restaurant = new Restaurant
        {
            Name = name,
            Address = address,
            CreatedAt = DateTime.Now
        };

        await _restaurantRepo.AddAsync(restaurant);
        await _restaurantRepo.SaveChangesAsync();

        TempData["Success"] = "Thêm quán ăn mới thành công.";
        return RedirectToAction(nameof(Restaurants));
    }

    // 4. Xử lý Xóa quán ăn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
        var restaurant = await _restaurantRepo.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        await _restaurantRepo.DeleteAsync(restaurant);
        await _restaurantRepo.SaveChangesAsync();

        TempData["Success"] = "Xóa quán ăn thành công.";
        return RedirectToAction(nameof(Restaurants));
    }

    // 5. Trang danh sách Quản lý Bài viết review
    [HttpGet]
    public async Task<IActionResult> Posts(string? searchString)
    {
        var allPosts = await _postRepo.GetAllAsync("Restaurant,User");
        var postsQuery = allPosts.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            postsQuery = postsQuery.Where(p => p.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                            || p.Content.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        }

        var result = postsQuery.OrderByDescending(p => p.CreatedAt).ToList();
        ViewBag.CurrentSearch = searchString;

        return View(result);
    }

    // 6. Xử lý Xóa bài viết khỏi hệ thống
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();

        await _postRepo.DeleteAsync(post);
        await _postRepo.SaveChangesAsync();

        TempData["Success"] = "Đã gỡ bỏ bài viết khỏi hệ thống.";
        return RedirectToAction(nameof(Posts));
    }
}