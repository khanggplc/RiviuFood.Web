using Microsoft.AspNetCore.Identity;

namespace RiviuFood.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<SavedPost> SavePosts { get; set; } = new List<SavedPost>();

        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
