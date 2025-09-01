using System.ComponentModel.DataAnnotations;

namespace RetailInventory.Api.Dtos
{
    public class CreateProductDto
    {
        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Admin can set this, but for owners/staff it will be overridden
        public int StoreId { get; set; }
    }
}
