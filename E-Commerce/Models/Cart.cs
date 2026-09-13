using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models;

public class Cart
{
    public int CartId { get; set; }

    [Required]
    public string UserId { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}