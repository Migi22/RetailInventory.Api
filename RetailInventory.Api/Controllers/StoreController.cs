using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RetailInventory.Api.Controllers
{
    [ApiController]
    [Route("api/stores")]
    public class StoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StoreController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method to get the StoreId from the user's claims
        private int? GetUserStoreId()
        {
            var storeClaim = User.FindFirst("StoreId");
            if (storeClaim == null || !int.TryParse(storeClaim.Value, out var userStoreId))
            {
                return null; // No StoreId in token or invalid format
            }
            return userStoreId;
        }

        // GET: api/stores
        [HttpGet]
        [Authorize(Roles = "SystemAdmin")] // only the system admin can view all stores
        public async Task<ActionResult<IEnumerable<Store>>> GetAllStores(bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                // Admin can view all, including soft-deleted
                return await _context.Stores
                    .IgnoreQueryFilters()
                    .ToListAsync();
            }

            // Default: show only active (not deleted)
            return await _context.Stores.ToListAsync();
        }

        // GET: api/stores/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "SystemAdmin,Owner,Staff")]
        public async Task<ActionResult<Store>> GetStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            if (User.IsInRole("Owner") || User.IsInRole("Staff"))
            {
                var userStoreId = GetUserStoreId();
                if (userStoreId == null || store.Id != userStoreId.Value)
                {
                    return Forbid();
                }
            }

            return store;
        }

        // POST: api/stores
        [HttpPost]
        [Authorize(Roles = "SystemAdmin")] // only the system admin can create stores
        public async Task<ActionResult<Store>> CreateStore(Store store)
        {
            if (store == null || string.IsNullOrWhiteSpace(store.Name))
            {
                return BadRequest("Store name is required.");
            }

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStore), new { id = store.Id }, store);
        }

        // PUT: api/stores/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SystemAdmin,Owner")]
        public async Task<IActionResult> UpdateStore(int id, Store store)
        {
            if (id != store.Id) return BadRequest("Store ID mismatch.");

            var existingStore = await _context.Stores.FindAsync(id);
            if (existingStore == null) return NotFound();

            if (User.IsInRole("Owner"))
            {
                var userStoreId = GetUserStoreId();
                if (userStoreId == null || existingStore.Id != userStoreId.Value)
                {
                    return Forbid();
                }
            }

            existingStore.Name = store.Name;
            existingStore.Address = store.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/stores/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            store.IsDeleted = true; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/stores/{id}/restore
        [HttpPatch("{id}/restore")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> RestoreStore(int id)
        {
            var store = await _context.Stores
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);

            if (store == null) 
                return NotFound("Store not found or not deleted.");

            if (!store.IsDeleted)
                return BadRequest("Store is already active.");

            store.IsDeleted = false; // Restore the store
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
