using Microsoft.AspNetCore.Identity;

namespace RiviuFood.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
