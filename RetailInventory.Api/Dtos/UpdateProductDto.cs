using System.ComponentModel.DataAnnotations;

namespace RetailInventory.Api.Dtos
{
    public class UpdateProductDto
    {
        [Required, MaxLength(128)]
        public string Name { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Only Admin should set StoreId (optional in request)
        public int? StoreId { get; set; }
    }
}
