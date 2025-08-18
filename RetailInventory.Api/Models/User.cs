using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailInventory.Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(32)]
        public string Role { get; set; } = "Staff"; // Roles can be "Staff", "Owner", "SystemAdmin"

        // Foreign key to Store (Nullable for SystemAdmin)
        public int? StoreId { get; set; }
        [ForeignKey("StoreId")]
        public Store? Store { get; set; }
    }
}
