using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels;

public class CategoryViewModel
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}