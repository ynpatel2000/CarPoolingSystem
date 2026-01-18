using System.Diagnostics;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("Slow API {Path} - {Time}ms",
                context.Request.Path, sw.ElapsedMilliseconds);
        }
    }
}
