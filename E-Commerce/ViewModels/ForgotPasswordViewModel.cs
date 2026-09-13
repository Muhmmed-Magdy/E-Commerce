using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}