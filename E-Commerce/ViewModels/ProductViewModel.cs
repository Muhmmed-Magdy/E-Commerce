using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels;

public class ProductViewModel
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

    [Required]
    public int CategoryId { get; set; }
}