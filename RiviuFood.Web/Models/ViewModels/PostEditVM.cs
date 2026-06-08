using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RiviuFood.Web.Models.ViewModels
{
    public class PostEditVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài review!")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung bài review!")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public int RestaurantId { get; set; }

        public string? ExistingImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem>? Restaurants { get; set; }
    }
}
