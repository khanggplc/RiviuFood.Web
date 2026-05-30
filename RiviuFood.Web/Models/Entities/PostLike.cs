namespace RiviuFood.Web.Models.Entities
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
