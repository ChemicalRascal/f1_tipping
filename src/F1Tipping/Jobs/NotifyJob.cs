using F1Tipping.Data;
using F1Tipping.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebPush = Lib.Net.Http.WebPush;

namespace F1Tipping.Jobs;

public partial class NotifyJob(
    AppDbContext appDb,
    PushNotificationsService pushService,
    NotificationScheduleService scheduleService,
    CurrentDataService currentDataService,
    ILogger<NotifyJob> logger)
{
    private readonly TimeSpan FORWARD_WINDOW = TimeSpan.FromMinutes(1);

    public class NotifyJobException(Guid userId, Exception innerException)
        : Exception($"Error on {userId}!", innerException);

    public async Task<IList<Exception>> Execute()
    {
        var jobExceptions = new List<Exception>();

        var cuttoff = DateTimeOffset.UtcNow + FORWARD_WINDOW;

        var users = await pushService.GetUsersWithPushSubs()
            .ToDictionaryAsync(u => u.Id);
        var validDates = scheduleService
            .GetNextNotificationDates(users.Values)
            .Where(kvp => kvp.Value < cuttoff);

        var round = await currentDataService.GetCurrentRoundAsync() ?? await currentDataService.GetNextRoundAsync();

        // TODO: Detailed push message
        var pMessage = new WebPush.PushMessage($"Round {round!.Index} tips due soon!");

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

                user.TemporalData.LastNotification = DateTimeOffset.UtcNow;
                user.TemporalData.NextNotification = next;
                appDb.Update(user);
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(e, "Exception for user ID: {}", userId);
                }
                jobExceptions.Add(new NotifyJobException(userId, e));
            }
        }
        await appDb.SaveChangesAsync();

        return jobExceptions;
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

        if (scheduleService.AnyUsersUnscheduled(users.Values))
        {
            validDates = validDates.Append(DateTimeOffset.UtcNow);
        }

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

        var newTrigger = Quartz.TriggerBuilder.Create()
            .ForJob(JobKey)
            .WithIdentity(JobKey.Name, JobKey.Group)
            .StartAt(runTime)
            .Build();

        var existingJob = await scheduler.GetJobDetail(JobKey);

        var existingTriggers = await scheduler.GetTriggersOfJob(JobKey);
        if (existingTriggers.Count > 0)
        {
            foreach (var trigger in existingTriggers)
            {
                await scheduler.RescheduleJob(trigger.Key, newTrigger);
            }
        }
        else if (existingJob is not null)
        {
            await scheduler.ScheduleJob(newTrigger);
        }
        else
        {
            var job = Quartz.JobBuilder.Create<NotifyJob>()
                .WithIdentity(JobKey)
                .PersistJobDataAfterExecution(true)
                .StoreDurably(true)
                .Build();

            await scheduler.ScheduleJob(job, newTrigger);
        }
    }
}

public partial class NotifyJob : Quartz.IJob
{
    async Task Quartz.IJob.Execute(Quartz.IJobExecutionContext context)
    {
        var errors = await Execute();

        context.Result = new Dictionary<string, object>
        {
            { "Success", errors.IsNullOrEmpty() },
            { "Error", new AggregateException([.. errors]) }
        };

        await TriggerSchedulingCheckAsync(new(context.Scheduler));
    }
}
