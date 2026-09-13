using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models;

public class Wishlist
{
    public int WishlistId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
}