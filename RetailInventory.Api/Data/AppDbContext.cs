using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Models;

namespace RetailInventory.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // Custom method for seeding
        public static void SeedData(AppDbContext context)
        {
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { Name = "Sample Product A", Quantity = 10, Price = 140.00m },
                    new Product { Name = "Sample Product B", Quantity = 5, Price = 55.00m },
                    new Product { Name = "Sample Product C", Quantity = 20, Price = 200.00m }
                );

                context.SaveChanges();
            }
        }
    }
}
