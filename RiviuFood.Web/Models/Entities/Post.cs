namespace RiviuFood.Web.Models.Entities;

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    public int RestaurantId { get; set; }
    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}