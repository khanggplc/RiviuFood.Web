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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var restaurants = await _restaurantRepo.GetAllAsync();
        var viewModel = new PostCreateVM
        {
            Restaurants = restaurants.Select(r => {
                var res = r as RiviuFood.Web.Models.Entities.Restaurant;
                return new SelectListItem
                {
                    Value = res.Id.ToString(),
                    Text = res.Name
                };
            }).ToList()
        };
        return View(viewModel);
    }

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