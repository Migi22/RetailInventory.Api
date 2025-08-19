using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailInventory.Api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Foreign key to Store
        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; } = null!; // Non-nullable reference type, Store must exist
    }
}
