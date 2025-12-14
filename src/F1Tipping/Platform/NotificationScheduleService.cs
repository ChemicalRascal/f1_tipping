using F1Tipping.Common;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Model;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Platform;

public class NotificationScheduleService(
    AppDbContext appDb,
    RoundDataService roundData,
    PushNotificationsService pushService)
{
    private async Task<User> GetUserAsync(Guid userId)
    {
        return await appDb.Users.Where(u => u.Id == userId)
            .SingleOrDefaultAsync()
            ?? throw new ApplicationException($"No user found for {userId}");
    }

    private async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<Guid> userIds)
    {
        return await appDb.Users.Where(u => userIds.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<IDictionary<Guid, DateTimeOffset?>> GetNextNotificationDatesAsync(IEnumerable<Guid> userIds)
    {
        var users = await GetUsersAsync(userIds);

        return GetNextNotificationDates(users);
    }

    public IDictionary<Guid, DateTimeOffset?> GetNextNotificationDates(IEnumerable<User> users)
        => users.ToDictionary(u => u.Id, u => u.TemporalData.NextNotification);

    /// <summary>
    /// Gets the next datetime a user should be sent a notification for a given round.
    /// </summary>
    /// <param name="userId">The user to calculate for.</param>
    /// <param name="roundId">The round to calculate for. If null, the current semantic round.</param>
    /// <param name="mostRecentNotification">The last time a user was sent a notification for the round. If null,
    /// The first notification scheduled for the round is returned.</param>
    /// <returns>The next notification date for the given user.</returns>
    public async Task<DateTimeOffset?> CalculateNextNotificationDateAsync(
        Guid userId,
        Guid? roundId = null,
        DateTimeOffset? mostRecentNotification = null)
    {
        var user = await GetUserAsync(userId);
        if (user.Settings.NotificationsSettings is null)
        {
            return null;
        }

        var round = roundId is not null
            ? await roundData.GetRoundByIdAsync(roundId.Value)
            : await roundData.GetSemanticNextRoundAsync();

        if (round is null)
        {
            return null;
        }

        var notificationSchedule = ResolveFullSchedule(user.Settings.NotificationsSettings, round);
        DateTimeOffset? nextNotification;

        if (mostRecentNotification is null)
        {
            nextNotification = notificationSchedule.First();
        }
        else
        {
            var cuttoff = DateTimeOffset.UtcNow - (user.Settings.NotificationsSettings.MinimumTimeBetweenNotifications / 2);
            nextNotification = notificationSchedule
                .Where(t => t > mostRecentNotification.Value)
                .FirstOrDefault(t => t > cuttoff);
        }

        return nextNotification;
    }

    public async Task PopulateFirstNotificationDatesForRoundAsync(Guid roundId)
    {
        var users = pushService.GetUsersWithPushSubs().ToList();
        foreach (var user in users)
        {
            user.TemporalData.NextNotification =
                await CalculateNextNotificationDateAsync(user.Id, roundId: roundId);
        }
        await appDb.SaveChangesAsync();
    }

    private static IEnumerable<DateTimeOffset> ResolveFullSchedule(NotificationsSettings settings, Round round)
    {
        var notificationTime =
            round.StartDate - settings.TipDeadlineStartOffset.Abs();
        var minGap = settings.MinimumTimeBetweenNotifications.Abs();

        TimeSpan gap() => round.StartDate - notificationTime;
        while (gap() >= minGap)
        {
            yield return notificationTime;
            notificationTime = settings.ScheduleType switch
            {
                NotificationsScheduleType.FixedPeriods => notificationTime + minGap,
                NotificationsScheduleType.ExponentialDecay => notificationTime + (gap()/2),
                _ => throw new ApplicationException(),
            };
        }
    }
}
