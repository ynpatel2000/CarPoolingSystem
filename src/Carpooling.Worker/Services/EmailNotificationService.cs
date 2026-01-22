using Carpooling.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Carpooling.Worker.Services;

public sealed class EmailNotificationService : INotificationService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IOptions<EmailSettings> options,
        ILogger<EmailNotificationService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string subject,
        string message,
        CancellationToken cancellationToken = default)
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
            _logger.LogInformation(
                "Sending booking email to {Email}",
                toEmail
            );

            var secureSocketOption = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                secureSocketOption,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await smtp.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
            }

            await smtp.SendAsync(email, cancellationToken);

            _logger.LogInformation(
                "Booking email sent successfully to {Email}",
                toEmail
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send booking email to {Email}",
                toEmail
            );

            throw; // IMPORTANT: let worker retry / DLQ
        }
        finally
        {
            if (smtp.IsConnected)
                await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
