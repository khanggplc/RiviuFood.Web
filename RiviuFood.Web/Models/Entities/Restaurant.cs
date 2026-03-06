namespace RiviuFood.Web.Models.Entities
{
    public class Restaurant
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    }
}
