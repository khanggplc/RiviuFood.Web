using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Models.ViewModels;
using RiviuFood.Web.Repositories;
using X.PagedList.Extensions;
using System.Linq;

namespace RiviuFood.Web.Controllers;

// ĐÃ THÊM: Thêm IGenericRepository<PostLike> _likeRepo vào Primary Constructor ở đây Boss nhé
public class PostController(
    IGenericRepository<Post> _postRepo,
    IGenericRepository<Comment> _commentRepo,
    IGenericRepository<Restaurant> _restaurantRepo,
    UserManager<ApplicationUser> _userManager,
    IWebHostEnvironment _webHostEnvironment,
    IGenericRepository<PostLike> _likeRepo) : Controller
{
    // Trang chi tiết bài review
    public async Task<IActionResult> Details(int id)
    {
        var post = await _postRepo.GetFirstOrDefaultAsync(
            p => p.Id == id,
            p => p.Restaurant,
            p => p.User,
            p => p.Comments
        );

        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> PostComment(int postId, string content)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(content) || userId == null) return BadRequest();

        var comment = new Comment
        {
            PostId = postId,
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.Now
        };

        await _commentRepo.AddAsync(comment);
        await _commentRepo.SaveChangesAsync();

        return Ok(new
        {
            user = User.Identity?.Name ?? "Người dùng",
            message = content,
            postId = postId
        });
    }

    //Sửa bình luận của người dùng
    [HttpPost]
    public async Task<IActionResult> EditComment(int commentId, string newContent)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId);
        var currentUserId = _userManager.GetUserId(User);

        if (comment == null || comment.UserId != currentUserId) return Forbid();

        comment.Content = newContent;

        _commentRepo.Update(comment);
        await _commentRepo.SaveChangesAsync();

        return Ok();
    }

    //Xóa bình luận của người dùng
    [HttpPost]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _commentRepo.GetByIdAsync(id);
        var currentUserId = _userManager.GetUserId(User);

        if (comment == null || comment.UserId != currentUserId) return Forbid();

        _commentRepo.Delete(comment);
        await _commentRepo.SaveChangesAsync();
        return Ok();
    }

    // Tạo bài viết mới
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var restaurants = await _restaurantRepo.GetAllAsync();
        var viewModel = new PostCreateVM
        {
            Restaurants = restaurants.Cast<RiviuFood.Web.Models.Entities.Restaurant>().Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList()
        };
        return View(viewModel);
    }
    // Xử lý POST khi người dùng submit form tạo bài viết mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVM model)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return BadRequest();

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Rating = model.Rating,
                RestaurantId = model.RestaurantId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            if (model.ImageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);

                string uploadsFolder = Path.Combine(wwwRootPath, "uploads", "posts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string path = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.ImageUrl = "/uploads/posts/" + fileName;
            }

            await _postRepo.AddAsync(post);
            await _postRepo.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        var restaurants = await _restaurantRepo.GetAllAsync();
        model.Restaurants = restaurants.Select(r => {
            var res = r as RiviuFood.Web.Models.Entities.Restaurant;
            return new SelectListItem
            {
                Value = res.Id.ToString(),
                Text = res.Name
            };
        });

        return View(model);
    }
    // Sửa bài viết của người dùng
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit (Guid id)
    {
        // Tìm bài viết theo Id
        var postObj = await _postRepo.GetByIdAsync(id);
        var post = postObj as RiviuFood.Web.Models.Entities.Post;

        if (post == null) return NotFound();

        // Lấy danh sách nhà hàng cho ô chọn Dropdown
        var restaurants = await _restaurantRepo.GetAllAsync("");

        // Đổ dữ liệu cũ vào ViewModel để mang ra giao diện hiển thị
        var viewModel = new PostEditVM
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            RestaurantId = post.RestaurantId,
            Rating = post.Rating,
            ExistingImageUrl = post.ImageUrl,
            Restaurants = restaurants.Cast<RiviuFood.Web.Models.Entities.Restaurant>().Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList()
        };

        return View(viewModel);

    }

    // Xử lý POST khi người dùng submit form sửa bài viết
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostEditVM model)
    {
        if (ModelState.IsValid)
        {
            // Lấy bài viết gốc từ Database lên để chuẩn bị cập nhật thông tin
            var postObj = await _postRepo.GetByIdAsync(model.Id);
            var post = postObj as RiviuFood.Web.Models.Entities.Post;

            if (post == null) return NotFound();

            // Mặc định giữ lại đường dẫn ảnh cũ
            string? fileName = post.ImageUrl;

            // Nếu người dùng chọn một file ảnh mới, tiến hành ghi đè dữ liệu ảnh
            if (model.ImageFile != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
                // Cập nhật đường dẫn ảnh mới
                fileName = "/uploads/" + fileName;
            }

            // Cập nhật các giá trị thay đổi vào thực thể gốc
            post.Title = model.Title;
            post.Content = model.Content;
            post.RestaurantId = model.RestaurantId;
            post.Rating = model.Rating;
            post.ImageUrl = fileName;

            // Gọi hàm Update của Repository để cập nhật vào Database
            await _postRepo.UpdateAsync(post);

            return RedirectToAction("Dashboard", "Profile");
        }

        // Nếu có lỗi dữ liệu, nạp lại danh sách nhà hàng và trả lại giao diện Form kèm thông báo lỗi
        var restaurants = await _restaurantRepo.GetAllAsync("");
        model.Restaurants = restaurants.Cast<RiviuFood.Web.Models.Entities.Restaurant>().Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();

        return View(model);
    }

    // Xóa bài viết của người dùng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id) {
        // Tim bai viet theo id trong database
        var postObj = await _postRepo.GetByIdAsync(id);
        var post = postObj as RiviuFood.Web.Models.Entities.Post;

        if (post == null) return NotFound();
        // Goi repository de xoa bai viet
        await _postRepo.DeleteAsync(post);

        // Xoa xong quay lai trang dashboard
        return RedirectToAction("Dashboard", "Profile");
    }


    // Trang quản lý bài viết cá nhân của người dùng
    [Authorize]
    public async Task<IActionResult> MyPosts(int? page)
    {
        var userId = _userManager.GetUserId(User);
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var allPosts = await _postRepo.GetAllAsync("Restaurant");
        var myPosts = allPosts.Where(p => p.UserId == userId);

        var pagedList = myPosts.OrderByDescending(p => p.CreatedAt).ToPagedList(pageNumber, pageSize);
        return View(pagedList);
    }
    // Xóa bài viết của người dùng
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        var currentUserId = _userManager.GetUserId(User);

        if (post == null || post.UserId != currentUserId) return Forbid();

        if (!string.IsNullOrEmpty(post.ImageUrl) && !post.ImageUrl.StartsWith("http"))
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fullImagePath = Path.Combine(wwwRootPath, post.ImageUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullImagePath))
            {
                System.IO.File.Delete(fullImagePath);
            }
        }

        _postRepo.Delete(post);
        await _postRepo.SaveChangesAsync();

        return RedirectToAction(nameof(MyPosts));
    }

    // Toggle like/unlike bài viết
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var allLikesQuery = await _likeRepo.GetAllAsync();
        var allLikes = allLikesQuery.Cast<PostLike>();

        var existingLike = allLikes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

        bool isLiked;

        if (existingLike != null)
        {
            _likeRepo.Delete(existingLike);
            isLiked = false;
        }
        else
        {
            var newLike = new PostLike { PostId = postId, UserId = userId };
            await _likeRepo.AddAsync(newLike);
            isLiked = true;
        }

        await _likeRepo.SaveChangesAsync();

        // Đếm lại tổng số lượt thích
        var updatedLikesQuery = await _likeRepo.GetAllAsync();
        var updatedLikes = updatedLikesQuery.Cast<PostLike>();

        var totalLikes = updatedLikes.Count(l => l.PostId == postId);
        

        return Json(new { success = true, isLiked = isLiked, totalLikes = totalLikes });
    }


}