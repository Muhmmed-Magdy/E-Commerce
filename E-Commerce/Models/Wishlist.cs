using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models;

public class Wishlist
{
    public int WishlistId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    public ApplicationUser? User { get; set; }

    public Product? Product { get; set; }
}