using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailInventory.Api.Data;
using RetailInventory.Api.Models;

namespace RetailInventory.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(AppDbContext db) : ControllerBase
    {
        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
            => await db.Products.AsNoTracking().ToListAsync();

        // GET: api/products/22
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await db.Products.FindAsync(id);
            return product is null ? NotFound() : product;
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new {id = product.Id}, product);
        }

        // PUT: api/products/22
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

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
            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();

            return NoContent();
        }




    }
}
