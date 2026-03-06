using Microsoft.AspNetCore.Identity;
using RiviuFood.Web.Models.Entities;

namespace RiviuFood.Web.Data;

public static class DbInitializer
{
    public static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        context.Database.EnsureCreated();

        // 1. Tạo User Admin mẫu (nếu chưa có)
        if (!context.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "bosskhang@riviu.com",
                Email = "bosskhang@riviu.com",
                FullName = "Boss Khang Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
        }

        var adminId = context.Users.First().Id;

        // 2. Tạo Danh mục (Categories)
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Cà phê muối", Slug = "ca-phe-muoi" },
                new Category { Name = "Trà sữa", Slug = "tra-sua" },
                new Category { Name = "Trà trái cây", Slug = "tra-trai-cay" },
                new Category { Name = "Bánh ngọt", Slug = "banh-ngot" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        // 3. Tạo Quán ăn/Cafe mẫu (Restaurants)
        if (!context.Restaurants.Any())
        {
            var restaurants = new List<Restaurant>
            {
                new Restaurant { Name = "Tiệm Cà Phê Tháng 3", Address = "Quận 10, TP.HCM" },
                new Restaurant { Name = "Trà Sữa Phê La", Address = "Quận 1, TP.HCM" }
            };
            context.Restaurants.AddRange(restaurants);
            context.SaveChanges();
        }

        // 4. Tạo Bài viết mẫu (Posts)
        if (!context.Posts.Any())
        {
            var allRestaurants = context.Restaurants.ToList();

            var firstResId = allRestaurants[0].Id; 
            var secondResId = allRestaurants[1].Id; 

            var posts = new List<Post>
{
    new Post
    {
        Title = "Review Cà Phê Muối cực phẩm tại Quận 10",
        Content = "Vị kem béo ngậy kết hợp với cà phê đậm đà...",
        Rating = 5,
        UserId = adminId,
        RestaurantId = firstResId, 
        CreatedAt = DateTime.UtcNow
    },
    new Post
    {
        Title = "Trà sữa Ô Long đậm vị tại Phê La",
        Content = "Trà thơm, không quá ngọt...",
        Rating = 4,
        UserId = adminId,
        RestaurantId = secondResId, 
        CreatedAt = DateTime.UtcNow
    }
};
            context.Posts.AddRange(posts);
            context.SaveChanges();
        }
    }
}