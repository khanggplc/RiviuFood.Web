using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RiviuFood.Web.Models.Entities;
using RiviuFood.Web.Repositories;

namespace RiviuFood.Web.Controllers;

public class PostController(IGenericRepository<Post> postRepo,
    IGenericRepository<Comment> commentRepo,
    UserManager<ApplicationUser> userManager) : Controller
{
    private readonly IGenericRepository<Post> _postRepo = postRepo;
    private readonly IGenericRepository<Comment> _commentRepo = commentRepo;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
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
    public IActionResult Create()
    {
        return View();
    }

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

        if (comment == null || comment.UserId != _userManager.GetUserId(User)) return Forbid();

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



}