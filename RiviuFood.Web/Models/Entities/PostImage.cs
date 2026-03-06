namespace RiviuFood.Web.Models.Entities
{
    public class PostImage
    {
        public int Id { get; set; }
        public required string ImageUrl { get; set; }
        public bool IsThumbnail { get; set; }
        public int PostId { get; set; }
    }
}
