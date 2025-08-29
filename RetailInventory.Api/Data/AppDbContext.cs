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

            // Apply global query filters for soft delete
            modelBuilder.Entity<Store>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        }

        // Custom method for seeding
        public static void SeedData(AppDbContext context)
        {
            // Ensure a demo store exists
            Store demoStore, secondDemoStore;
            if (!context.Stores.Any())
            {
                demoStore = new Store { Name = "Demo Store", Address = "123 Main" };
                secondDemoStore = new Store { Name = "Second Demo Store", Address = "456 Side St." };

                context.Stores.AddRange(demoStore, secondDemoStore);

                // System admin
                context.Users.Add(
                    new User
                    {
                        Username = "sysadmin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "SystemAdmin"
                    }
                );

                // Owner + Staff for Demo Store
                context.Users.AddRange(
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

                // Owner + Staff for Branch Store
                context.Users.AddRange(
                    new User
                    {
                        Username = "owner2",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("owner456"),
                        Role = "Owner",
                        Store = secondDemoStore
                    },
                    new User
                    {
                        Username = "staff2",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff456"),
                        Role = "Staff",
                        Store = secondDemoStore
                    }
                );
            }
            else
            {
                demoStore = context.Stores.First(); // reuse existing store
                secondDemoStore = context.Stores.Skip(1).FirstOrDefault()
                      ?? new Store { Name = " Second Demo Store", Address = "456 Side St" };
            }

            // Now seed products with valid StoreId
            if (!context.Products.Any())
            {
                // Seed products for Demo Store
                context.Products.AddRange(
                    new Product { Name = "Sample Product A", Quantity = 10, Price = 140.00m, Store = demoStore },
                    new Product { Name = "Sample Product B", Quantity = 5, Price = 55.00m, Store = demoStore },
                    new Product { Name = "Sample Product C", Quantity = 20, Price = 200.00m, Store = demoStore }
                );

                // Seed products for Second Demo Store
                context.Products.AddRange(
                    new Product { Name = "Second Branch Product X", Quantity = 15, Price = 99.00m, Store = secondDemoStore },
                    new Product { Name = "Second Branch Product Y", Quantity = 7, Price = 120.00m, Store = secondDemoStore }
                );
            }

            context.SaveChanges();
        }


    }
}
