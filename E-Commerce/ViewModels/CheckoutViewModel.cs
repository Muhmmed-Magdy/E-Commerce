using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public string? PostalCode { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash On Delivery";
    }
}