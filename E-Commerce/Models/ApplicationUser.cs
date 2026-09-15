using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;



    // One user can have one cart.
    public Cart? Cart { get; set; }

    // One user can have many orders.
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // One user can have many wishlist items.
    public ICollection<Wishlist> WishlistItems { get; set; } = new List<Wishlist>();
}