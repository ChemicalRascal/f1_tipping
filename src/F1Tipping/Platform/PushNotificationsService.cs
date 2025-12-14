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

    public IQueryable<User> GetUsersWithPushSubs()
    {
        return appDb.UserPushNotificationSubscriptions
            .Select(x => x.User).Distinct();
    }

    public async Task SendNotificationToUserAsync(
        User user, WebPush.PushMessage pMessage)
    {
        ArgumentNullException.ThrowIfNull(user);

        foreach (var sub in PushSubscriptions(user))
        {
            await SendNotificationAsync(sub, pMessage);
        }
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
            .Where(sub => sub.User.Id == user.Id);
    }

    private async Task SendNotificationAsync(
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
        catch (Exception e)
        {
            // TODO: Validate that these logs go somewhere useful
            if (logger?.IsEnabled(LogLevel.Error) ?? false)
            {
                logger.LogError(e, "Exception on sub ID: {}", sub.Id);
            }
        }
    }
}
