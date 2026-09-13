using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models;

public class CartItem
{
    public int CartItemId { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
}