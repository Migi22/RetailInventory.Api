using System.ComponentModel.DataAnnotations;

namespace RetailInventory.Api.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Address { get; set; }

        // Navigation property: One store can have many users
        public ICollection<User>? Users { get; set; } = new List<User>();
    }
}
