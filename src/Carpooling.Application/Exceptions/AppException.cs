namespace Carpooling.Application.Exceptions;

/// <summary>
/// Represents a controlled business exception
/// that should be returned to API clients safely.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    public AppException(
        string message,
        int statusCode = 400,
        string errorCode = "APP_ERROR")
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
