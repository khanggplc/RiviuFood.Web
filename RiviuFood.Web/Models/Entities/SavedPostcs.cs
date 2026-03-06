using System.Data;
using System;

namespace RiviuFood.Web.Models.Entities
{
    public class SavedPost
    {
        public required string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
