using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.PlayerData
{
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
            // TODO: Fix this redirecting stuff like static assets
            // Just need to redirect Tipping pages and the home page.
            if (context.Request.Path != INIT_PATH
                && (context.User.Identity?.IsAuthenticated ?? false)
                && context.User.IsInRole("Player"))
            {
                var user = await userManager.GetUserAsync(context.User);
                var player = await modelDb.Players.SingleOrDefaultAsync(
                    p => p.AuthUserId == user!.Id);
                if (player is not null
                    && player.Status == Model.Tipping.PlayerStatus.Uninitialized)
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
