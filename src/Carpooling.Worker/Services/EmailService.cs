using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Carpooling.Worker.Services;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _settings.SenderName,
            _settings.SenderEmail
        ));

        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.SmtpServer,
            _settings.Port,
            SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
