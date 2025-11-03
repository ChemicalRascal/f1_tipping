using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        // controller is ndeeded
        var user = await userManager.GetUserAsync(User);
        if (user is not null)
        {
            var newPushSub = new PushSubscription()
            {
                DeviceEndpoint = request.DeviceEndpoint,
                PublicKey = request.PublicKey,
                AuthSecret = request.AuthSecret,
                User = user,
            };

            appDb.UserPushNotificationSubscriptions.Add(newPushSub);
            await appDb.SaveChangesAsync();

            return Created();
        }

        return Unauthorized();
    }
}
