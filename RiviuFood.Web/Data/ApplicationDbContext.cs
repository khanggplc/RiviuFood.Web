using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RiviuFood.Web.Models.Entities;

namespace RiviuFood.Web.Data
{
    // LƯU Ý SỬA: Phải có <ApplicationUser> ở đây
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bắt buộc phải có dòng này khi dùng Identity
            base.OnModelCreating(builder);

            // 1. Cấu hình bảng Like (Khóa chính hỗn hợp)
            builder.Entity<Like>(entity => {
                entity.HasKey(l => new { l.UserId, l.PostId });

                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // Ngăn lỗi vòng lặp xóa

                entity.HasOne(l => l.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa bài viết thì mất Like
            });

            // 2. Cấu hình bảng SavedPost (Khóa chính hỗn hợp)
            builder.Entity<SavedPost>(entity => {
                entity.HasKey(s => new { s.UserId, s.PostId });

                entity.HasOne(s => s.User)
                      .WithMany(u => u.SavedPosts)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.Post)
                      .WithMany()
                      .HasForeignKey(s => s.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Cấu hình bảng Comment (Giải quyết dứt điểm lỗi Hình 3 của Boss)
            builder.Entity<Comment>(entity => {
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            // 4. Cấu hình quan hệ Nhiều-Nhiều giữa Post và Category
            builder.Entity<Post>(entity => {
                entity.HasMany(p => p.Categories)
                      .WithMany(c => c.Posts)
                      .UsingEntity(j => j.ToTable("PostCategories")); // Tự động tạo bảng trung gian tên đẹp
            });

            // 5. Ràng buộc dữ liệu (Constraints) tối ưu hóa DB
            builder.Entity<Restaurant>(entity => {
                entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Address).HasMaxLength(500);
            });

            builder.Entity<Category>(entity => {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Slug).IsRequired().HasMaxLength(100);
                entity.HasIndex(c => c.Slug).IsUnique(); // Bảo vệ URL không bị trùng lặp
            });

            builder.Entity<Post>(entity => {
                entity.Property(p => p.Title).IsRequired().HasMaxLength(255);

                // User -> Post
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // Xóa User không tự động xóa Post
            });
        }
    }
}