using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPush = Lib.Net.Http.WebPush;

namespace F1Tipping.Platform;

public class PushNotificationsService(AppDbContext appDb, IConfiguration config)
{
    private WebPush.PushServiceClient client = new()
    {
        DefaultAuthentication = new(
            config.GetValue<string>("Vapid:publicKey"),
            config.GetValue<string>("Vapid:privateKey"))
        {
            Subject = config.GetValue<string>("Vapid:subject"),
        },
    };

    public async Task SendNotificationToUser(
        IdentityUser<Guid> user, WebPush.PushMessage pMessage)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        foreach (var sub in PushSubscriptions(user))
        {
            await SendNotification(sub, pMessage);
        }
    }

    public async Task<bool> UserHasAnySubs(IdentityUser<Guid> user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return await PushSubscriptions(user).AnyAsync();
    }

    public async Task<PushSubscription?> GetSubscriptionForEndpoint(
        IdentityUser<Guid> user,
        string endpoint)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return await PushSubscriptions(user).FirstOrDefaultAsync(
            sub => sub.DeviceEndpoint == endpoint);
    }

    private IQueryable<PushSubscription> PushSubscriptions(IdentityUser<Guid> user)
    {
        return appDb.UserPushNotificationSubscriptions
            .Where(sub => sub.User.Id == user.Id);
    }

    private async Task SendNotification(PushSubscription sub, WebPush.PushMessage pMessage)
    {
        var pSub = new WebPush.PushSubscription();
        pSub.SetKey(WebPush.PushEncryptionKeyName.P256DH, sub.PublicKey);
        pSub.SetKey(WebPush.PushEncryptionKeyName.Auth, sub.AuthSecret);
        pSub.Endpoint = sub.DeviceEndpoint;

        try
        {
            await client.RequestPushMessageDeliveryAsync(pSub, pMessage);
        }
        catch (Exception)
        {
            // TODO: Do logging here!
        }
    }
}
