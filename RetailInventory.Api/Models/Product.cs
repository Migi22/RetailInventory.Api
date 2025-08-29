using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailInventory.Api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public required string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Foreign key to Store
        [Required]
        public int StoreId { get; set; }

        // Audit
        public DateTime? DeletedAt { get; set; }
        
        public string? DeletedBy { get; set; }

        // Navigation
        [ForeignKey("StoreId")]
        public required Store Store { get; set; }
    }
}
