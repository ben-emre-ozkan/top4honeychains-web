using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services
{
    // D-73: the first request after a start paid for building the EF models, opening the first database connections and
    // loading the reference translations (about 1-2.5 s). This runs once in the background right after start-up, so the
    // first user does not wait for it. Failures are logged and ignored: the app works the same without it.
    public class StartupWarmup : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<StartupWarmup> _logger;

        public StartupWarmup(IServiceScopeFactory scopes, ILogger<StartupWarmup> logger)
        {
            _scopes = scopes;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var started = DateTime.UtcNow;
            try
            {
                using var scope = _scopes.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
                await context.Beekeepers.AsNoTracking().AnyAsync(stoppingToken);
                await context.FinalHoneyProductions.AsNoTracking().AnyAsync(stoppingToken);
                var identity = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await identity.Users.AsNoTracking().AnyAsync(stoppingToken);
                scope.ServiceProvider.GetRequiredService<RefText>().T("Ccp", 1, "Title", null);
                _logger.LogInformation("Start-up warm-up done in {Ms} ms", (int)(DateTime.UtcNow - started).TotalMilliseconds);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogWarning(e, "Start-up warm-up failed; the first request will be slower");
            }
        }
    }
}
