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
    public int? RestaurantId { get; set; }
    public string? NewRestaurantName { get; set; }
    public string? NewRestaurantAddress { get; set; }
    public string? NewRestaurantArea { get; set; }      // Khu vực quán mới
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Restaurants { get; set; }         //Đổ dữ liệu DropdownList Quán ăn/cafe
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Areas { get; set; }              //Danh sách khu vực

    public IFormFile? ImageFile { get; set; }    //nhận file ảnh từ form

}
