using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public string? ImagePath { get; set; }

    // Foreign Key
    public int CategoryId { get; set; }

    // Navigation Property
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}