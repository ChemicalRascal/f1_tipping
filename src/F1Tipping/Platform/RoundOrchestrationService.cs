using F1Tipping.Data;
using F1Tipping.Jobs;
using Quartz;

namespace F1Tipping.Platform;

public class RoundOrchestrationService(
    AppDbContext appDb,
    RoundDataService roundData,
    NotificationScheduleService notificationScheduleService)
{
    public async Task SetupNotificationsForRound(CancellationToken cancellationToken = default)
    {
        var globalData = await appDb.GetGlobalTemporalDataAsync();
        var lastNotifiedRound = globalData.LastNotifiedRoundId;
        var upcomingRound = (await roundData.GetSemanticNextRoundAsync())?.Id;

        if (upcomingRound is not null && lastNotifiedRound != upcomingRound)
        {
            await notificationScheduleService
                .PopulateFirstNotificationDatesForRoundAsync(upcomingRound.Value);
            globalData.LastNotifiedRoundId = upcomingRound;
            await appDb.SaveChangesAsync(cancellationToken);
        }
    }
}

public class RoundOrchestrationServiceStarter(
    IServiceProvider serviceProvider,
    ISchedulerFactory schedulerFactory)
    : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
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