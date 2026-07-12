using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Models.ViewModels;
using RiviuFood.Web.Repositories;
using X.PagedList.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RiviuFood.Web.Controllers;

public class PostController(
    IGenericRepository<Post> _postRepo,
    IGenericRepository<Comment> _commentRepo,
    IGenericRepository<Restaurant> _restaurantRepo,
    UserManager<ApplicationUser> _userManager,
    IWebHostEnvironment _webHostEnvironment,
    IGenericRepository<PostLike> _likeRepo) : Controller
{
    // 1. Trang chi tiết bài review
    [HttpGet]
    [AllowAnonymous]
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

    // 2. Thêm bình luận bằng AJAX
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

    // 3. Sửa bình luận của người dùng
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

    // 4. Xóa bình luận của người dùng
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

    // 5. Giao diện Tạo bài viết mới [GET]
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var restaurants = await _restaurantRepo.GetAllAsync();
        var viewModel = new PostCreateVM
        {
            Restaurants = restaurants.Cast<Restaurant>().Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList()
        };
        return View(viewModel);
    }

    // 6. Xử lý POST tạo bài viết mới kèm upload ảnh
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
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

        // ĐÃ SỬA: Dùng Cast<Restaurant>() loại bỏ hoàn toàn cảnh báo ép kiểu
        var restaurants = await _restaurantRepo.GetAllAsync();
        model.Restaurants = restaurants.Cast<Restaurant>().Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();

        return View(model);
    }

    // 7. Giao diện Sửa bài viết [GET] - Đã đồng bộ sang int id
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();

        var restaurants = await _restaurantRepo.GetAllAsync();

        var viewModel = new PostEditVM
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            RestaurantId = post.RestaurantId,
            Rating = post.Rating,
            ExistingImageUrl = post.ImageUrl,
            Restaurants = restaurants.Cast<Restaurant>().Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList()
        };

        return View(viewModel);
    }

    // 8. Xử lý lưu bài viết sau khi Sửa [POST]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(PostEditVM model)
    {
        if (ModelState.IsValid)
        {
            var post = await _postRepo.GetByIdAsync(model.Id);
            if (post == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId) return Forbid();

            string? fileName = post.ImageUrl;

            if (model.ImageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadsFolder = Path.Combine(wwwRootPath, "uploads", "posts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
                fileName = "/uploads/posts/" + fileName;
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.RestaurantId = model.RestaurantId;
            post.Rating = model.Rating;
            post.ImageUrl = fileName;

            _postRepo.Update(post);
            await _postRepo.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Profile");
        }

        var restaurants = await _restaurantRepo.GetAllAsync();
        model.Restaurants = restaurants.Cast<Restaurant>().Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();

        return View(model);
    }

    // 9. Trang quản lý bài viết cá nhân phân trang
    [Authorize]
    [HttpGet]
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

    // 10. Hàm Xóa bài viết dứt điểm [POST] - Đã hợp nhất logic an toàn bao gồm xóa file vật lý
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        var currentUserId = _userManager.GetUserId(User);

        if (post == null || post.UserId != currentUserId) return Forbid();

        // Xóa file ảnh vật lý khỏi ổ cứng Server để dọn bộ nhớ rác
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

        return RedirectToAction("Dashboard", "Profile");
    }

    // 11. Toggle like/unlike bài viết bằng AJAX
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

        var updatedLikesQuery = await _likeRepo.GetAllAsync();
        var updatedLikes = updatedLikesQuery.Cast<PostLike>();
        var totalLikes = updatedLikes.Count(l => l.PostId == postId);

        return Json(new { success = true, isLiked = isLiked, totalLikes = totalLikes });
    }
}