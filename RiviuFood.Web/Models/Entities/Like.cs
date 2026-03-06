namespace RiviuFood.Web.Models.Entities
{
    public class Like
    {
        public required string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
    }
}
