using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile Image { get; set; }
    }
}