using F1Tipping.Jobs;
using Quartz;

namespace F1Tipping.Platform;

public partial class JobScheduleService(
    IServiceProvider serviceProvider,
    ISchedulerFactory schedulerFactory
    )
{
    public static IEnumerable<(Type jobType, string cronSchedule)> GetCronJobs()
    {
        yield break;
    }
}

public partial class JobScheduleService : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        await TriggerChecksForAppStartSelfSchedulingJobsAsync(cancellationToken);
    }

    public async Task TriggerChecksForAppStartSelfSchedulingJobsAsync(
        CancellationToken cancellationToken = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var jobContext = new JobContext(scheduler);

        var jobTypes = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface
                && t.IsAssignableTo(typeof(ISelfScheduleOnAppStart)));

        if (!jobTypes.Any())
        {
            return;
        }

        using (var scope = serviceProvider.CreateScope())
        {
            foreach (var jobType in jobTypes)
            {
                var jobInstance = (ISelfSchedulingJob)ActivatorUtilities.CreateInstance(
                    scope.ServiceProvider, jobType);
                await jobInstance.TriggerSchedulingCheckAsync(jobContext);
            }
        }
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public interface ISelfSchedulingJob
{
    Task TriggerSchedulingCheckAsync(JobContext context);
    Task ScheduleAsync(JobContext context, DateTimeOffset runTime);
    Task ScheduleNoLaterThanAsync(JobContext context, DateTimeOffset runTime);
}

public interface ISelfScheduleOnAppStart : ISelfSchedulingJob
{ }

public record JobContext(IScheduler QuartzScheduler);

public static partial class QuartzExtensions
{
    extension(IServiceCollectionQuartzConfigurator q)
    {
        public void RegisterCronJobs()
        {
            foreach (var (jobType, cronSchedule) in JobScheduleService.GetCronJobs())
            {
                var jobKey = new JobKey(jobType.FullName!);

                q.AddJob(jobType, jobKey, c =>
                {
                    c.DisallowConcurrentExecution();
                });

                q.AddTrigger(c =>
                {
                    c.ForJob(jobKey);
                    c.StartNow().WithCronSchedule(cronSchedule);
                });
            }
        }
    }
}
