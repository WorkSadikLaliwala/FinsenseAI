using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinSenseAPI.Repositories.Interfaces;

namespace FinSenseAPI.Services;

public class CleanupService : IHostedService, IDisposable
{
    private readonly ILogger<CleanupService> _logger;
    //private readonly IUploadSessionRepository _uploadSessionRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private Task? _executingTask;

    public CleanupService(
        ILogger<CleanupService> logger, 
        //IUploadSessionRepository uploadSessionRepo,
        IServiceScopeFactory serviceScopeFactory
        )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        //_uploadSessionRepo = uploadSessionRepo;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = Task.Run(() => RunAsync(_cts.Token));
        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var _uploadSessionRepo =
                    scope.ServiceProvider.GetRequiredService<IUploadSessionRepository>();
                var now = DateTime.UtcNow;
                var nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(2); // next 2 AM UTC
                if (now > nextRun) nextRun = nextRun.AddDays(1);
                var delay = nextRun - now;
                _logger.LogInformation("CleanupService sleeping until {delay} for next run at {nextRun}", delay, nextRun);
                await Task.Delay(delay, ct);

                _logger.LogInformation("CleanupService running expired guest session cleanup");
                await _uploadSessionRepo.DeleteExpiredGuestSessionsAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CleanupService error while deleting expired sessions");
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null) return;
        _cts.Cancel();
        await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
