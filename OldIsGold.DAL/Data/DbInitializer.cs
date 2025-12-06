using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Models;

namespace OldIsGold.DAL.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            // Create database if it doesn't exist (development approach)
            try
            {
                // First try EnsureCreated for simplicity in development
                await context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnsureCreated failed: {ex.Message}");
                // If that fails, try migrations
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception migrationEx)
                {
                    Console.WriteLine($"Migration failed: {migrationEx.Message}");
                    // Database might already exist, continue with seeding
                }
            }

            // Create Roles
            string[] roleNames = { "Admin", "Seller", "Buyer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin User
            var adminEmail = "admin@oldsgold.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    UserType = UserType.Buyer,  // Admin doesn't need seller/buyer type
                    EmailConfirmed = true,
                    JoinDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create Demo Buyer
            var buyerEmail = "buyer@test.com";
            if (await userManager.FindByEmailAsync(buyerEmail) == null)
            {
                var buyerUser = new ApplicationUser
                {
                    UserName = buyerEmail,
                    Email = buyerEmail,
                    FullName = "Demo Buyer",
                    UserType = UserType.Buyer,
                    EmailConfirmed = true,
                    JoinDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(buyerUser, "Test@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(buyerUser, "Buyer");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Coins", Description = "Rare and collectible coins", Icon = "🪙", IsActive = true },
                    new Category { Name = "Jewelry", Description = "Vintage and antique jewelry", Icon = "💎", IsActive = true },
                    new Category { Name = "Anime Cards", Description = "Collectible anime trading cards", Icon = "🎴", IsActive = true },
                    new Category { Name = "Trading Cards", Description = "Pokemon, Yu-Gi-Oh, FIFA cards", Icon = "🃏", IsActive = true },
                    new Category { Name = "Sports Memorabilia", Description = "Signed jerseys and sports items", Icon = "⚽", IsActive = true },
                    new Category { Name = "Vintage Electronics", Description = "Old tech items", Icon = "📟", IsActive = true },
                    new Category { Name = "Historical Items", Description = "Historical artifacts", Icon = "📜", IsActive = true },
                    new Category { Name = "Old Books", Description = "Rare and antique books", Icon = "📚", IsActive = true },
                    new Category { Name = "Art Pieces", Description = "Vintage art and paintings", Icon = "🎨", IsActive = true }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Note: For a full implementation, you would create 15 items per category here with AI-generated images
            // For this demo, we're creating a minimal working version
            // Items can be added through the seller interface once the app is running
        }
    }
}
