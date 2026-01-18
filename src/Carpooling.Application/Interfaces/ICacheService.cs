namespace Carpooling.Application.Interfaces;

/// <summary>
/// Abstraction for distributed cache (Redis)
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan expiry);

    Task RemoveAsync(string key);
}
