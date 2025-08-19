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
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<Store>>> GetAllStores()
        {
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
        [Authorize(Roles = "SystemAdmin,Owner")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            if (User.IsInRole("Owner"))
            {
                var userStoreId = GetUserStoreId();
                if (userStoreId == null || store.Id != userStoreId.Value)
                {
                    return Forbid();
                }
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
