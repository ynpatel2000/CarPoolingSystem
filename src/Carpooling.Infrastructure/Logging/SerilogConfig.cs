using Serilog;

namespace Carpooling.Infrastructure.Logging;

public static class SerilogConfig
{
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
