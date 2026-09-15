using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels
{
    public class CheckoutViewModel
    {

        [Required]
        public string PaymentMethod { get; set; } = "Cash On Delivery";
    }
}
