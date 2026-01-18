namespace Carpooling.Worker.Services;

public class NotificationService
{
    public void SendBookingConfirmation(string message)
    {
        Console.WriteLine($"📧 EMAIL SENT: {message}");
        Console.WriteLine($"🔔 PUSH SENT: {message}");
    }
}
