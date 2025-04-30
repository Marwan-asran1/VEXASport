using Microsoft.EntityFrameworkCore;
using VEXA.Models;

namespace VEXA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("No Connection String Is Found");

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    SeedDatabase(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void SeedDatabase(AppDbContext context)
        {
            // Check if users already exist
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Name = "Admin",
                    Password = "adminPass",
                    PhoneNumber = "9876543210",
                    Email = "admin@shop.com",
                    Address = "456 Admin Ave",
                    UserRole = User.Role.Admin
                };

                var newUser = new User
                {
                    Name = "John Doe",
                    Password = "secure123",
                    PhoneNumber = "1234567890",
                    Email = "john.doe@example.com",
                    Address = "123 Main St",
                    UserRole = User.Role.Customer
                };

                context.Users.Add(adminUser);
                context.Users.Add(newUser);
            }

            // Check if categories already exist
            if (!context.Categories.Any())
            {
                var cat1 = new Category { Name = "Tops" };
                var cat2 = new Category { Name = "Bottoms" };
                var cat3 = new Category { Name = "Shoes" };

                context.Categories.AddRange(cat1, cat2, cat3);
            }

            context.SaveChanges();
        }
    }
}
