using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;
using System.Security.Claims;

namespace RetailInventory.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "Owner,Staff,SystemAdmin")]
    public class ProductController(AppDbContext db) : ControllerBase
    {
        /// <summary>
        /// Get all products.
        /// SystemAdmin can see all products.
        /// Owner and Staff can only see products for their own store.
        /// </summary>
        /// <returns>List of products visible to the current user.</returns>
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

        /// <summary>
        /// Get a specific product by ID.
        /// SystemAdmin can access any product.
        /// Owner and Staff can only access products for their own store.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The product with the specified ID if accessible; otherwise; 403 Forbidden</returns>
        // GET: api/products/22
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");

            var product = await db.Products.FindAsync(id);

            if (product is null) return NotFound();

            if (role != "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid("You do not have access to this product.");


            return product is null ? NotFound() : product;
        }

        /// <summary>
        /// Create a new product.
        /// Staff and Owners can only create products for their own store.
        /// SystemAdmin can create products for any store.
        /// </summary>
        /// <param name="product">The product object to create.</param>
        /// <returns>The newly created product along with a 201 Created response.</returns>
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

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>
        /// Update an existing product.
        /// Staff and Owners can only update products for their own store.
        /// SystemAdmin can update any product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="product">The updated product object.</param>
        /// <returns>204 NoContent if successful, 400 BadRequest if IDs mismatch, 403 Forbidden if access is denied, or
        /// 404 NotFound if the product does not exist.</returns>
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

        /// <summary>
        /// Delete a product by ID.
        /// Staff and Owners can only delete products for their own store.
        /// SystemAdmin can delete any product.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>204 NoContent if successful, 403 Forbidden if access is denied, or
        /// 404 NotFound if the product does not exist.</returns>
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
