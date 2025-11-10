using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PushSubscriptionsController(
    UserManager<IdentityUser<Guid>> userManager,
    AppDbContext appDb
    ) : Controller
{
    public class PushSubscriptionRequest
    {
        public required string DeviceEndpoint { get; init; }
        public required string PublicKey { get; init; }
        public required string AuthSecret { get; init; }
    }

    [HttpPost]
    public async Task<ActionResult<PushSubscriptionRequest>> PostPushSubscription([FromBody] PushSubscriptionRequest request)
    {
        // TODO: Refactor this into a base api controller class if a second api
        // controller is ndeeded. Or API-specific middleware?
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if ((await GetSubscriptionForEndpoint(user, request.DeviceEndpoint)) is not null)
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

        appDb.UserPushNotificationSubscriptions.Add(newPushSub);
        await appDb.SaveChangesAsync();

        return Created();
    }

    [HttpGet("validate")]
    public async Task<ActionResult<bool>> EndpointIsRegistered(string endpoint)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        //var sub = await appDb.UserPushNotificationSubscriptions
        //    .FirstOrDefaultAsync(sub => sub.DeviceEndpoint == endpoint);
        var sub = await GetSubscriptionForEndpoint(user, endpoint);

        if (sub is null)
        {
            return Ok(false);
        }

        sub.LastCheck = DateTime.UtcNow;
        await appDb.SaveChangesAsync();

        return Ok(true);
    }

    private async Task<PushSubscription?> GetSubscriptionForEndpoint(
        IdentityUser<Guid> user,
        string endpoint)
    {
        return await appDb.UserPushNotificationSubscriptions
            .FirstOrDefaultAsync(sub => 
            sub.User.Id == user.Id
            && sub.DeviceEndpoint == endpoint);
    }
}
