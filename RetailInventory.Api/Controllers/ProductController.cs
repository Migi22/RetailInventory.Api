using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;
using System.Security.Claims;

namespace RetailInventory.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner,Staff,SystemAdmin")]
    public class ProductController(AppDbContext db) : ControllerBase
    {
        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            var query = db.Products.AsNoTracking();

            // Staff are limited to their own store
            if (role == "SystemAdmin" && storeIdClaim != null)
            {
                int storeId = int.Parse(storeIdClaim);
                query = query.Where(p => p.StoreId == storeId);
            }

            return await query.ToListAsync();
        }
            

        // GET: api/products/22
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            var product = await db.Products.FindAsync(id);
            
            if(product is null) return NotFound();

            if (role!= "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid("You do not have access to this product.");


            return product is null ? NotFound() : product;
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            // Staff and Owners can only create products for their own store
            if (role != "SystemAdmin" && storeIdClaim != null)
            {
                product.StoreId = int.Parse(storeIdClaim);
            }
                
            db.Products.Add(product);
            await db.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetById), new {id = product.Id}, product);
        }

        // PUT: api/products/22
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            // Check for product ownership
            var existing = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existing is null) return NotFound();

            if (role != "SystemAdmin" && storeIdClaim != null && existing.StoreId != int.Parse(storeIdClaim))
                return Forbid("You do not have access to modify this product.");

            db.Entry(product).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await db.Products.AnyAsync(p => p.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/products/22
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();

            if (role != "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid("You do not have access to delete this product.");

            db.Products.Remove(product);
            await db.SaveChangesAsync();

            return NoContent();
        }

    }
}
