using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace E_Commerce.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings settings;

    public EmailSender(IOptions<EmailSettings> options)
    {
        settings = options.Value;
    }

    public async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string message)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                "E-Commerce",
                settings.Email));

        email.To.Add(
            MailboxAddress.Parse(recipientEmail));

        email.Subject = subject;

        email.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            settings.Host,
            settings.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            settings.Email,
            settings.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}