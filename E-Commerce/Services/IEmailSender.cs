namespace E_Commerce.Services;

public interface IEmailSender
{
    Task SendEmailAsync(
        string email,
        string subject,
        string message);
}