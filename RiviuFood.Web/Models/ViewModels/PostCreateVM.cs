using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace RiviuFood.Web.Models.ViewModels;

public class PostCreateVM
{
    [Required(ErrorMessage = "Nhập tiêu đề bài viết")]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Nhập nội dung bài viết")]
    public string Content { get; set; } = string.Empty;
    [Range(1,5)]
    public int Rating { get; set; }
    [Required(ErrorMessage = "Chọn quán để tạo bài viết")]
    public int RestaurantId { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Restaurants { get; set; }         //Đổ dữ liệu DropdownList Quán ăn/cafe

    public IFormFile? ImageFile { get; set; }    //nhận file ảnh từ form

}

