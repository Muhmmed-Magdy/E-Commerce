using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models;

public class Order
{
    public int OrderId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
    
    [Required]
    public string PaymentMethod { get; set; } = "Cash On Delivery";

    [Required]
    public string PaymentStatus { get; set; } = "Pending";


    public ApplicationUser? User { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}