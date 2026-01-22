namespace Carpooling.Application.Interfaces;

public interface INotificationService
{
    Task SendBookingConfirmationAsync(
        string toEmail,
        string subject,
        string message,
        CancellationToken cancellationToken = default);
}
