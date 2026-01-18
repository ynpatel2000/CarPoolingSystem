using Microsoft.Extensions.Configuration;
using Serilog;

namespace Carpooling.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static void ConfigureSerilog(IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/app-.log",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
