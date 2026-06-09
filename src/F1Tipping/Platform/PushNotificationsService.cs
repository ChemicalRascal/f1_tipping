using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using Microsoft.EntityFrameworkCore;
using WebPush = Lib.Net.Http.WebPush;

namespace F1Tipping.Platform;

public class PushNotificationsService(
    AppDbContext appDb,
    IConfiguration config,
    ILogger<PushNotificationsService> logger)
{
    private readonly WebPush.PushServiceClient client = new()
    {
        DefaultAuthentication = new(
            config.GetValue<string>("Vapid:publicKey"),
            config.GetValue<string>("Vapid:privateKey"))
        {
            Subject = config.GetValue<string>("Vapid:subject"),
        },
    };

    public record PushSubError(int Id, string Type);

    public IQueryable<User> GetUsersWithPushSubs()
    {
        return appDb.UserPushNotificationSubscriptions
            .Select(x => x.User)
            .Where(u => u.Settings != null
                     && u.Settings.NotificationsSettings != null
                     && u.Settings.NotificationsSettings.ScheduleType != NotificationsScheduleType.NotSet)
            .Distinct();
    }

    public async Task SendNotificationToUserAsync(
        User user, WebPush.PushMessage pMessage)
    {
        ArgumentNullException.ThrowIfNull(user);

        var errors = new List<PushSubError>();
        foreach (var sub in PushSubscriptions(user))
        {
            var error = await SendNotificationAsync(sub, pMessage);
            if (error is not null)
            {
                errors.Add(error);
            }
        }
        await UpdateSubState(errors);
    }

    public async Task<bool> UserHasAnySubsAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return await PushSubscriptions(user).AnyAsync();
    }

    public async Task<PushSubscription?> GetSubscriptionForEndpointAsync(
        User user,
        string endpoint)
    {
        ArgumentNullException.ThrowIfNull(user);

        return await PushSubscriptions(user).FirstOrDefaultAsync(
            sub => sub.DeviceEndpoint == endpoint);
    }

    private IQueryable<PushSubscription> PushSubscriptions(User user)
    {
        return appDb.UserPushNotificationSubscriptions
            .Where(sub => sub.User.Id == user.Id
                       && sub.Error == null);
    }

    private async Task UpdateSubState(IEnumerable<PushSubError> errors)
    {
        var groupByError = errors.GroupBy(e => e.Type, e => e.Id);

        foreach (var errorGroup in groupByError)
        {
            await appDb.UserPushNotificationSubscriptions
                .Where(x => errorGroup.Contains(x.Id))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Error, x => errorGroup.Key));
        }
    }

    private async Task<PushSubError?> SendNotificationAsync(
        PushSubscription sub,
        WebPush.PushMessage pMessage)
    {
        var pSub = new WebPush.PushSubscription();
        pSub.SetKey(WebPush.PushEncryptionKeyName.P256DH, sub.PublicKey);
        pSub.SetKey(WebPush.PushEncryptionKeyName.Auth, sub.AuthSecret);
        pSub.Endpoint = sub.DeviceEndpoint;

        try
        {
            await client.RequestPushMessageDeliveryAsync(pSub, pMessage);
        }
        catch (WebPush.PushServiceClientException e)
        {
            if (logger?.IsEnabled(LogLevel.Error) ?? false)
            {
                logger.LogError(e, "Exception on sub ID: {}", sub.Id);
            }
            return new PushSubError(sub.Id, e.Message);
        }
        catch (Exception e)
        {
            if (logger?.IsEnabled(LogLevel.Error) ?? false)
            {
                logger.LogError(e, "Exception on sub ID: {}", sub.Id);
            }
            throw;
        }

        return null;
    }
}
