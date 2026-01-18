using Carpooling.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Carpooling.Worker.Services;

public sealed class EmailNotificationService : INotificationService
{
    private readonly EmailSettings _settings;

    public EmailNotificationService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string subject,
        string message)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _settings.FromName,
            _settings.FromEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        email.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtp = new SmtpClient
        {
            Timeout = _settings.TimeoutSeconds * 1000
        };

        try
        {
            var secureOption = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                secureOption);

            await smtp.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await smtp.SendAsync(email);
        }
        finally
        {
            if (smtp.IsConnected)
                await smtp.DisconnectAsync(true);
        }
    }
}
