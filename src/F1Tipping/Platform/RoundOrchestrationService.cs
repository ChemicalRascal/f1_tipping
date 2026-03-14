using F1Tipping.Common;
using F1Tipping.Data;
using F1Tipping.Jobs;
using Quartz;

namespace F1Tipping.Platform;

public class RoundOrchestrationService(
    AppDbContext appDb,
    CurrentDataService roundData,
    NotificationScheduleService notificationScheduleService)
{
    public async Task SetupNotificationsForRound(CancellationToken cancellationToken = default)
    {
        var globalData = await appDb.GetGlobalTemporalDataAsync();
        var lastNotifiedRound = globalData.LastNotifiedRoundId;
        var upcomingRound = (await roundData.GetNextRoundAsync())?.Id;

        if (upcomingRound is not null && lastNotifiedRound != upcomingRound)
        {
            await notificationScheduleService
                .PopulateFirstNotificationDatesForRoundAsync(upcomingRound.Value);
            globalData.LastNotifiedRoundId = upcomingRound;
            await appDb.SaveChangesAsync(cancellationToken);
        }
    }
}

public partial class RoundOrchestrationServiceStarter(
    IServiceProvider serviceProvider,
    ISchedulerFactory schedulerFactory,
    IConfiguration configuration)
    : IHostedService
{
    private const string SKIP_ROUND_ORCHESTRATION_CONFIG_KEY = "SkipRoundOrchestration";

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        if (configuration.GetValue<string>(SKIP_ROUND_ORCHESTRATION_CONFIG_KEY)
            ?.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            return;
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var ros = scope.ServiceProvider.GetRequiredService<RoundOrchestrationService>();
            await ros.SetupNotificationsForRound(cancellationToken);

            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.Start(cancellationToken);

            var notifyJob = ActivatorUtilities.CreateInstance<NotifyJob>(scope.ServiceProvider);
            await notifyJob.TriggerSchedulingCheckAsync(new(scheduler));
        }
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public partial class RoundOrchestrationServiceStarter : IDefineCliArgs
{
    static IEnumerable<KeyValuePair<string, string>> IDefineCliArgs.Switches =>
        [ new("--skip-round-orchestration", SKIP_ROUND_ORCHESTRATION_CONFIG_KEY) ];
}
