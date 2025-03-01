using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace F1Tipping.PlayerData
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class PlayerMustBeInitalizedAttribute : Attribute {}

    public class ForcePlayerInitalizationMiddleware
    {
        private readonly RequestDelegate _next;

        private const string INIT_PATH = "/PlayerAdmin/Init";

        public ForcePlayerInitalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ModelDbContext modelDb,
            UserManager<IdentityUser<Guid>> userManager)
        {
            if (context.GetEndpoint()?.Metadata
                ?.GetMetadata<PlayerMustBeInitalizedAttribute>() is not null
                && (context.User.Identity?.IsAuthenticated ?? false)
                && context.User.IsInRole("Player"))
            {
                var user = await userManager.GetUserAsync(context.User);
                var player = await modelDb.Players.SingleOrDefaultAsync(
                    p => p.AuthUserId == user!.Id);
                if (player is null
                    || player.Status == Model.Tipping.PlayerStatus.Uninitialized)
                {
                    context.Response.Redirect(INIT_PATH);
                }
            }

            await _next(context);
        }
    }

    public static class ForcePlayerInitalizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseForcePlayerInitialization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ForcePlayerInitalizationMiddleware>();
        }
    }
}
