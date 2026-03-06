namespace RiviuFood.Web.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; } // Ví dụ: Cà phê, Trà sữa, Nước ép, Bánh ngọt...
        public string? Icon { get; set; }
        public required string Slug { get; set; }

        // Một thể loại có thể có nhiều bài viết review
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
