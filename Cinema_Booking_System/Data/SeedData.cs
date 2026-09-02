using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace CinemaBookingSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            string[] roles = { "Admin", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin
            const string adminEmail = "admin@cinema.com";
            const string adminPassword = "Admin@123";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Action", Description = "High-octane action movies", IsActive = true },
                    new() { Name = "Comedy", Description = "Light-hearted comedies", IsActive = true },
                    new() { Name = "Drama", Description = "Dramatic story-driven films", IsActive = true },
                    new() { Name = "Horror", Description = "Scary and suspense movies", IsActive = true },
                    new() { Name = "Sci-Fi", Description = "Science fiction adventures", IsActive = true },
                    new() { Name = "Animation", Description = "Animated films for all ages", IsActive = true },
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed Movies
            if (!context.Movies.Any())
            {
                var cat = context.Categories.ToList();
                var movies = new List<Movie>
                {
                    new() { Title = "Galactic Rangers", Description = "Space heroes save the universe from an ancient evil.", Director = "Alex Johnson", Genre = "Sci-Fi", Duration = 135, Language = "English", Rating = "PG-13", Price = 15.00m, CategoryId = cat.First(c => c.Name=="Sci-Fi").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-30) },
                    new() { Title = "Laugh Out Loud", Description = "A comedy about misadventures in a small town.", Director = "Maria Chen", Genre = "Comedy", Duration = 100, Language = "English", Rating = "PG", Price = 12.00m, CategoryId = cat.First(c => c.Name=="Comedy").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-15) },
                    new() { Title = "Shadow Falls", Description = "A thriller set in a mysterious forest.", Director = "David Park", Genre = "Horror", Duration = 110, Language = "English", Rating = "R", Price = 13.00m, CategoryId = cat.First(c => c.Name=="Horror").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-7) },
                    new() { Title = "Heart of Steel", Description = "An inspiring drama about resilience.", Director = "Sarah Lee", Genre = "Drama", Duration = 125, Language = "English", Rating = "PG", Price = 12.00m, CategoryId = cat.First(c => c.Name=="Drama").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-20) },
                    new() { Title = "Turbo Blast", Description = "Non-stop action with stunning stunts.", Director = "Mike Brown", Genre = "Action", Duration = 120, Language = "English", Rating = "PG-13", Price = 15.00m, CategoryId = cat.First(c => c.Name=="Action").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-5) },
                    new() { Title = "Tiny Adventures", Description = "Animated journey through a magical world.", Director = "Lin Zhang", Genre = "Animation", Duration = 95, Language = "English", Rating = "G", Price = 10.00m, CategoryId = cat.First(c => c.Name=="Animation").Id, IsActive = true, ReleasedAt = DateTime.Today.AddDays(-10) },
                };
                context.Movies.AddRange(movies);
                await context.SaveChangesAsync();

                // Seed Showtimes
                var now = DateTime.Today;
                var showtimes = new List<Showtime>();
                foreach (var movie in context.Movies.ToList())
                {
                    for (int d = 0; d < 7; d++)
                    {
                        showtimes.Add(new Showtime { MovieId = movie.Id, ShowDate = now.AddDays(d), ShowTime = new TimeSpan(14, 0, 0), Hall = "Hall 1", TotalSeats = 80, AvailableSeats = 80, Price = movie.Price, IsActive = true });
                        showtimes.Add(new Showtime { MovieId = movie.Id, ShowDate = now.AddDays(d), ShowTime = new TimeSpan(19, 0, 0), Hall = "Hall 2", TotalSeats = 120, AvailableSeats = 120, Price = movie.Price + 2, IsActive = true });
                    }
                }
                context.Showtimes.AddRange(showtimes);
                await context.SaveChangesAsync();
            }
        }
    }
}
