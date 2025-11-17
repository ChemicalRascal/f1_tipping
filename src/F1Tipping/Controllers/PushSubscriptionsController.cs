using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPush = Lib.Net.Http.WebPush;

namespace F1Tipping.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PushSubscriptionsController(
    UserManager<User> userManager,
    AppDbContext appDb,
    PushNotificationsService pushNotifications
    ) : Controller
{
    public class PushSubscriptionRequest
    {
        public required string DeviceEndpoint { get; init; }
        public required string PublicKey { get; init; }
        public required string AuthSecret { get; init; }
    }

    [HttpPost]
    public async Task<ActionResult<PushSubscriptionRequest>>
        PostPushSubscription([FromBody] PushSubscriptionRequest request)
    {
        // TODO: Refactor this into a base api controller class if a second api
        // controller is ndeeded. Or API-specific middleware?
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if ((await pushNotifications.GetSubscriptionForEndpoint(
            user, request.DeviceEndpoint)) is not null)
        {
            return Conflict("Endpoint subscription already exists.");
        }

        var newPushSub = new PushSubscription()
        {
            DeviceEndpoint = request.DeviceEndpoint,
            PublicKey = request.PublicKey,
            AuthSecret = request.AuthSecret,
            User = user,
            Created = DateTime.UtcNow,
        };

        user.Settings.NotificationsSettings = new()
        {
            TipDeadlineStartOffset = new(),
            NotifyForOldTips = true,
            ScheduleType = NotificationsScheduleType.ExponentialDecay,
        };

        appDb.UserPushNotificationSubscriptions.Add(newPushSub);

        await appDb.SaveChangesAsync();
        return Created();
    }

    [HttpDelete]
    public async Task<ActionResult<PushSubscriptionRequest>> DeletePushSubscription(
        [FromBody] PushSubscriptionRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var sub = await pushNotifications.GetSubscriptionForEndpoint(
            user, request.DeviceEndpoint);
        if (sub is null)
        {
            // Is this an attack vector? The user has to be logged in, so I
            // seriously doubt it.
            return NotFound();
        }

        if (sub.DeviceEndpoint != request.DeviceEndpoint
            || sub.PublicKey != request.PublicKey
            || sub.AuthSecret != request.AuthSecret)
        {
            // TODO: Add logging?
            return NotFound();
        }

        appDb.UserPushNotificationSubscriptions.Remove(sub);
        user.Settings.NotificationsSettings = null;

        await appDb.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("validate")]
    public async Task<ActionResult<bool>> EndpointIsRegistered(string endpoint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var sub = await pushNotifications.GetSubscriptionForEndpoint(user, endpoint);
        if (sub is null)
        {
            return Ok(false);
        }

        sub.LastCheck = DateTime.UtcNow;
        await appDb.SaveChangesAsync();

        return Ok(true);
    }

    [HttpGet("userHasSub")]
    public async Task<ActionResult<bool>> UserHasSub()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await pushNotifications.UserHasAnySubs(user));
    }

    [HttpHead]
    public async Task<ActionResult> HeadPushSubscription()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var pMessage = new WebPush.PushMessage("Debug Notification.");
        pMessage.Urgency = WebPush.PushMessageUrgency.High;

        await pushNotifications.SendNotificationToUser(user, pMessage);
        return Ok();
    }
}