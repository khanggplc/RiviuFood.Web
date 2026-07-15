using Microsoft.AspNetCore.Mvc;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace RiviuFood.Web.Controllers
{
    public class HomeController(IGenericRepository<Post> _postRepo) : Controller
    {
        // Nhận trực tiếp repository qua Primary Constructor
        private readonly IGenericRepository<Post> _postRepo = _postRepo;

        [HttpGet]
        public async Task<IActionResult> Index(string? searchString, string? locationFilter)
        {
            // 1. Lấy toàn bộ bài viết từ Database lên, nạp kèm thông tin Quán ăn và Người đăng
            var allPosts = await _postRepo.GetAllAsync("Restaurant, User, Comments, PostLikes");
            var postsQuery = allPosts.AsQueryable();

            // 2. Bộ lọc tìm kiếm thông minh theo Tiêu đề bài viết hoặc Tên món ăn
            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)
                                                || p.Content.Contains(searchString, System.StringComparison.OrdinalIgnoreCase));
            }

            // 3. Bộ lọc tìm kiếm theo Địa điểm / Tên nhà hàng
            if (!string.IsNullOrEmpty(locationFilter))
            {
                postsQuery = postsQuery.Where(p => p.Restaurant != null && p.Restaurant.Name.Contains(locationFilter, System.StringComparison.OrdinalIgnoreCase));
            }

            // 4. Sắp xếp bài viết mới nhất lên đầu tiên và gửi ra giao diện
            var result = postsQuery.OrderByDescending(p => p.CreatedAt).ToList();

            // Lưu lại từ khóa tìm kiếm để hiển thị lại trên thanh input cho người dùng biết họ vừa gõ gì
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentLocation = locationFilter;

            return View(result);
        }
    }
}