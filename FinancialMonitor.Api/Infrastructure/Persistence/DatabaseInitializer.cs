using Microsoft.EntityFrameworkCore;

namespace FinancialMonitor.Api.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private static readonly SemaphoreSlim initializationLock = new(1, 1);

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await initializationLock.WaitAsync(cancellationToken);
        try
        {
            await using var scope = services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<FinancialMonitorDbContext>();
            await database.Database.EnsureCreatedAsync(cancellationToken);
        }
        finally
        {
            initializationLock.Release();
        }
    }
}