using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models;

public class Order
{
    public int OrderId { get; set; }

    [Required]
    public string UserId { get; set; }

    public DateTime OrderDate { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public string Status { get; set; }

    public ApplicationUser User { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}