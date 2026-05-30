using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RiviuFood.Web.Models.ViewModels
{
    public class PostEditVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung review đâu rồi Boss ơi?")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public int RestaurantId { get; set; }

        public string? ExistingImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem>? Restaurants { get; set; }
    }
}
