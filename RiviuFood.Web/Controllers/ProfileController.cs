using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using RiviuFood.Web.Models;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;

namespace RiviuFood.Web.Controllers
{
    // Bắt buộc phải đăng nhập mới vào được trang quản lý này
    [Authorize]
    public class ProfileController(
        UserManager<ApplicationUser> userManager,
        IGenericRepository<Post> postRepo,
        IGenericRepository<Comment> commentRepo) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IGenericRepository<Post> _postRepo = postRepo;
        private readonly IGenericRepository<Comment> _commentRepo = commentRepo;

        // Trang quản trị chính (Dashboard) của cá nhân người dùng
        public async Task<IActionResult> Dashboard()
        {
            // 1. Lấy thông tin tài khoản người dùng đang đăng nhập hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 2. Lấy toàn bộ bài viết của hệ thống kèm bảng Likes và Comments để tính toán số liệu
            var allPosts = await _postRepo.GetAllAsync("PostLikes, Comments, Restaurant");

            // 3. Lọc riêng danh sách các bài viết do chính User này đăng
            var myPosts = allPosts.Where(p => p.UserId == user.Id).OrderByDescending(p => p.CreatedAt).ToList();

            // 4. Tính toán số liệu thống kê nhanh để hiển thị lên các thẻ (Widgets)
            int totalMyPosts = myPosts.Count;
            int totalLikesReceived = myPosts.Sum(p => p.PostLikes?.Count ?? 0);

            // Lấy thêm số bình luận mà người này đã đi viết ở toàn bộ website (nếu có)
            var allComments = await _commentRepo.GetAllAsync("");
            int totalMyComments = allComments.Count(c => ((Comment)c).UserId == user.Id);

            // 5. Đóng gói số liệu chuyển sang giao diện thông qua ViewBag
            ViewBag.UserInfo = user;
            ViewBag.TotalPosts = totalMyPosts;
            ViewBag.TotalLikes = totalLikesReceived;
            ViewBag.TotalComments = totalMyComments;

            // Trả về giao diện quản trị danh sách bài viết cá nhân
            return View(myPosts);
        }
    }
}