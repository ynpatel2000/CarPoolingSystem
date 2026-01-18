using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Carpooling.Worker.HealthChecks;

public class WorkerHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            HealthCheckResult.Healthy("Worker is running")
        );
    }
}
