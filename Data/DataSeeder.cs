using ParbaudesDarbs.Api.Models;
using ParbaudesDarbs.Api.Services;

namespace ParbaudesDarbs.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
    {
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Elektronika", Description = "Ierīces un piederumi" },
                new Category { Name = "Grāmatas", Description = "Drukātās un digitālās grāmatas" },
                new Category { Name = "Mājas preces", Description = "Ikdienas mājas lietošanas preces" }
            );
        }

        if (!context.Products.Any())
        {
            var electronics = context.Categories.Local.FirstOrDefault(c => c.Name == "Elektronika") ?? context.Categories.First(c => c.Name == "Elektronika");
            var books = context.Categories.Local.FirstOrDefault(c => c.Name == "Grāmatas") ?? context.Categories.First(c => c.Name == "Grāmatas");

            context.Products.AddRange(
                new Product { Name = "Bluetooth austiņas", Description = "Bezvadu austiņas", Price = 59.99m, Category = electronics },
                new Product { Name = "C# pamati", Description = "Mācību grāmata", Price = 24.50m, Category = books }
            );
        }

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User { Email = "admin@example.com", PasswordHash = hasher.Hash("Admin123!"), Role = "Admin" },
                new User { Email = "user@example.com", PasswordHash = hasher.Hash("User123!"), Role = "Customer" }
            );
        }

        await context.SaveChangesAsync();
    }
}
