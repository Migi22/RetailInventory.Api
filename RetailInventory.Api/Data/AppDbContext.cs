using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Models;

namespace RetailInventory.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // Custom method for seeding
        public static void SeedData(AppDbContext context)
{
    // Ensure a demo store exists
    Store demoStore;
    if (!context.Stores.Any())
    {
        demoStore = new Store { Name = "Demo Store", Address = "123 Main" };
        context.Stores.Add(demoStore);

        context.Users.AddRange(
            new User
            {
                Username = "sysadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "SystemAdmin"
            },
            new User
            {
                Username = "owner1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                Role = "Owner",
                Store = demoStore
            },
            new User
            {
                Username = "staff1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                Role = "Staff",
                Store = demoStore
            }
        );
    }
    else
    {
        demoStore = context.Stores.First(); // reuse existing store
    }

    // Now seed products with valid StoreId
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "Sample Product A", Quantity = 10, Price = 140.00m, Store = demoStore },
            new Product { Name = "Sample Product B", Quantity = 5, Price = 55.00m, Store = demoStore },
            new Product { Name = "Sample Product C", Quantity = 20, Price = 200.00m, Store = demoStore }
        );
    }

    context.SaveChanges();
}


    }
}
