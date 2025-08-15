using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Models;

namespace RetailInventory.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();

    }
    
}
