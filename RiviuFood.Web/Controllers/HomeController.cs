using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Data;
using RiviuFood.Web.Models;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;
using X.PagedList.Extensions;

namespace RiviuFood.Web.Controllers;

public class HomeController(IGenericRepository<Post> postRepo) : Controller
{
    private readonly IGenericRepository<Post> _postRepo = postRepo;

    public async Task<IActionResult> Index(string searchString, int? page)
    {
        // 1. Khởi tạo pageNumber và pageSize
        int pageSize = 6;
        int pageNumber = (page ?? 1);

        // 2. Lấy dữ liệu bài viết kèm thông tin liên quan 
        var postsQuery = await _postRepo.GetAllAsync(includeProperties: "Restaurant,User,Comments,PostLikes");
        var posts = postsQuery.AsEnumerable(); // Chuyển về Enumerable để xử lý tiếp

        // 3. Logic tìm kiếm 
        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            posts = posts.Where(p => p.Title.ToLower().Contains(searchString)
                                  || p.Restaurant.Name.ToLower().Contains(searchString));

            // Lưu lại từ khóa để hiển thị lại trên ô nhập liệu (ViewBag)
            ViewBag.CurrentSearch = searchString;
        }

        // 4. Phân trang dữ liệu đã lọc
        var pagedList = posts.OrderByDescending(p => p.CreatedAt).ToPagedList(pageNumber, pageSize);

        return View(pagedList);
    }
}