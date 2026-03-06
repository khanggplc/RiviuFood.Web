namespace RiviuFood.Web.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public required string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public int PostId { get; set; }
        // THÊM DÒNG NÀY VÀO:
        public virtual Post Post { get; set; } = null!;
    }
}
