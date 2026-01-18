namespace Carpooling.Worker.Services;

public sealed class EmailSettings
{
    // Configuration section name
    public const string SectionName = "EmailSettings";

    // SMTP
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;

    // Security
    public bool UseSsl { get; init; } = true;

    // Sender
    public string FromName { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;

    // Credentials
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    // Optional
    public int TimeoutSeconds { get; init; } = 30;
}
