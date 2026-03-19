using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Models.ViewModels;
using RiviuFood.Web.Repositories;

namespace RiviuFood.Web.Controllers;

public class PostController(IGenericRepository<Post> postRepo,
    IGenericRepository<Comment> commentRepo,
    IGenericRepository<Restaurant> restaurantRepo,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    private readonly IGenericRepository<Post> _postRepo = postRepo;
    private readonly IGenericRepository<Comment> _commentRepo = commentRepo;
    private readonly IGenericRepository<Restaurant> _restaurantRepo = restaurantRepo;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    // Trang chi tiết bài review
    public async Task<IActionResult> Details(int id)
    {
        // Lấy bài viết, kèm theo Quán cafe và thông tin Người đăng
        var post = await _postRepo.GetFirstOrDefaultAsync(
            p => p.Id == id,
            p => p.Restaurant,
            p => p.User,
            p => p.Comments
        );

        if (post == null) return NotFound();
        return View(post);
    }

    // Trang tạo bài review mới (Chỉ cho người dùng đã đăng nhập)
    

    [HttpPost]
    public async Task<IActionResult> PostComment(int postId, string content)
    {
        var userId = _userManager.GetUserId(User); // Lấy ID người dùng hiện tại
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
            user = User.Identity?.Name ?? "Người dùng", // Trả về Name/Email thay vì Guid ID
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

        // Bảo mật: Chỉ chủ comment mới được sửa
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

        // Kiểm tra: Nếu không tìm thấy hoặc người xóa không phải chủ comment
        if (comment == null || comment.UserId != currentUserId)
        {
            return Forbid(); // Trả về lỗi 403 - Cấm truy cập
        }

        _commentRepo.Delete(comment);
        await _commentRepo.SaveChangesAsync();
        return Ok();
    }

    [Authorize] // Chỉ cho phép người đã đăng nhập
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var restaurants = await _restaurantRepo.GetAllAsync();
        var viewModel = new PostCreateVM
        {
            Restaurants = restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            })
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVM model)
    {
        if (ModelState.IsValid)
        {
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Rating = model.Rating,
                RestaurantId = model.RestaurantId,
                UserId = _userManager.GetUserId(User), 
                CreatedAt = DateTime.Now
            };

            // Xử lý Upload Ảnh 
            if (model.ImageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string path = Path.Combine(wwwRootPath, @"uploads\posts", fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
                // post.ImageUrl = @"\uploads\posts\" + fileName; 
            }

            await _postRepo.AddAsync(post);
            await _postRepo.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }





}