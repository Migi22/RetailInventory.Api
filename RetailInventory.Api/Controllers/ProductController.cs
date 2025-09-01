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

            // Owner and Staff are limited to their own store
            if (role == "Owner" || role == "Staff")
            {
                if (storeIdClaim != null)
                {
                    int storeId = int.Parse(storeIdClaim);
                    query = query.Where(p => p.StoreId == storeId);
                }
            }

            // SystemAdmin gets all products, no filter applied
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

            var product = await db.Products.AsNoTracking()
                                            .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            if (role != "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid();


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

            // explicitly reset audit fields just to be safe
            product.IsDeleted = false;
            product.DeletedAt = null;
            product.DeletedBy = null;

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

            // Prevent updates to deleted products
            if (existing.IsDeleted) return BadRequest();

            if (role != "SystemAdmin" && storeIdClaim != null && existing.StoreId != int.Parse(storeIdClaim))
                return Forbid();

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
        /// SystemAdmin can delete any product.
        /// Owners can only delete products for their own store.
        /// Staff cannot delete products.
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
            var username = User.Identity?.Name ?? "Unknown";

            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();

            // Prevent double deletion
            if (product.IsDeleted) 
                return BadRequest();

            if (role != "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid();

            // Soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.DeletedBy = username;

            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted product by ID.
        /// SystemAdmin can restore any product.
        /// Owners can restore products for their own store.
        /// Staff cannot restore products.
        /// </summary>
        /// <param name="id">The ID of the product to restore.</param>
        /// <returns>
        /// 204 NoContent if successful, 400 BadRequest if the product is not deleted,
        /// 403 Forbidden if access is denied
        /// </returns>
        /// PATCH: api/products/22/restore
        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "Owner,SystemAdmin")]
        public async Task<IActionResult> Restore(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var storeIdClaim = User.FindFirstValue("StoreId");
            var username = User.Identity?.Name ?? "Unknown";

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            // Prevent restoring an active product
            if(!product.IsDeleted) 
                return BadRequest();

            // Check permissions
            if (role != "SystemAdmin" && storeIdClaim != null && product.StoreId != int.Parse(storeIdClaim))
                return Forbid();

            // Restore
            product.IsDeleted = false;
            product.RestoredAt = DateTime.UtcNow;
            product.RestoredBy = username;

            // Save changes to database
            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return NoContent();
        }


    }
}
