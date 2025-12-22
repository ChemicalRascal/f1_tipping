using F1Tipping.Data;
using F1Tipping.Platform;
using Microsoft.EntityFrameworkCore;
using WebPush = Lib.Net.Http.WebPush;

namespace F1Tipping.Jobs;

public partial class NotifyJob(
    AppDbContext appDb,
    PushNotificationsService pushService,
    NotificationScheduleService scheduleService,
    ILogger<NotifyJob> logger)
{
    private readonly TimeSpan FORWARD_WINDOW = TimeSpan.FromMinutes(1);

    public async Task Execute()
    {
        var cuttoff = DateTimeOffset.UtcNow + FORWARD_WINDOW;

        var users = await pushService.GetUsersWithPushSubs()
            .ToDictionaryAsync(u => u.Id);
        var validDates = scheduleService
            .GetNextNotificationDates(users.Values)
            .Where(kvp => kvp.Value < cuttoff);

        // TODO: Detailed push message
        var pMessage = new WebPush.PushMessage("DEBUG: Notif. from NotifyJob");
        //DateTimeOffset? nextJobRun = null;

        foreach (var (userId, date) in validDates)
        {
            try
            {
                var user = users[userId];
                await pushService.SendNotificationToUserAsync(user,
                    pMessage);
                var next = await scheduleService.CalculateNextNotificationDateAsync(
                    userId,
                    mostRecentNotification: date);

                appDb.Update(user);
                user.TemporalData.LastNotification = DateTimeOffset.UtcNow;
                user.TemporalData.NextNotification = next;

                //if (next is not null && (nextJobRun is null || next < nextJobRun))
                //{
                //    nextJobRun = next;
                //}
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(e, "Exception for user ID: {}", userId);
                }
            }
        }
        await appDb.SaveChangesAsync();

        //return nextJobRun;
    }
}

public partial class NotifyJob : ISelfSchedulingJob
{
    public Quartz.JobKey JobKey { get; } = new(typeof(NotifyJob).FullName!);

    public async Task TriggerSchedulingCheckAsync(JobContext context)
    {
        var users = await pushService.GetUsersWithPushSubs()
            .ToDictionaryAsync(u => u.Id, u => u);
        var validDates = scheduleService
            .GetNextNotificationDates(users.Values)
            .Where(kvp => kvp.Value.HasValue)
            .Select(kvp => kvp.Value!.Value);

        if (validDates.Any())
        {
            await ScheduleNoLaterThanAsync(context, validDates.Min());
        }
    }

    public async Task ScheduleNoLaterThanAsync(JobContext context, DateTimeOffset runTime)
    {
        var scheduler = context.QuartzScheduler;

        var existingTriggers = await scheduler.GetTriggersOfJob(JobKey);
        if (existingTriggers.All(t =>
            t.GetNextFireTimeUtc() is null
            || t.GetNextFireTimeUtc() >= runTime))
        {
            await ScheduleAsync(context, runTime);
        }
    }

    public async Task ScheduleAsync(JobContext context, DateTimeOffset runTime)
    {
        var scheduler = context.QuartzScheduler;

        var existingTriggers = await scheduler.GetTriggersOfJob(JobKey);
        if (existingTriggers.Count > 0)
        {
            await scheduler.UnscheduleJobs(
                [.. existingTriggers.Select(t => t.Key)]);
        }

        var job = Quartz.JobBuilder.Create<NotifyJob>()
            .WithIdentity(JobKey)
            .Build();

        var newTrigger = Quartz.TriggerBuilder.Create()
            .ForJob(JobKey)
            .WithIdentity(JobKey.Name, JobKey.Group)
            .StartAt(runTime)
            .Build();

        await scheduler.ScheduleJob(job, newTrigger);
    }
}

public partial class NotifyJob : Quartz.IJob
{
    async Task Quartz.IJob.Execute(Quartz.IJobExecutionContext context)
    {
        await Execute();
        await TriggerSchedulingCheckAsync(new(context.Scheduler));
    }
}
