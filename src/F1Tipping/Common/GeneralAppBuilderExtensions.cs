using F1Tipping.Data.AppModel;
using Microsoft.AspNetCore.Authentication;

namespace F1Tipping.Common;

public static class GeneralAppBuilderExtensions
{
    private static bool PathMatches(HttpContext context, string pathPrefix)
        => (context.Request.Path.Value ?? string.Empty)
            .StartsWith(pathPrefix, StringComparison.InvariantCultureIgnoreCase);

    private static bool UserAuthenticated(HttpContext context)
        => context.User.Identity?.IsAuthenticated ?? false;

    private static bool UserAuthorized(HttpContext context, string requiredRole)
        => context.User.IsInRole(requiredRole);

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Sets up middleware challenging user to login if they aren't logged in for the
        /// given path prefix.
        /// </summary>
        /// <param name="pathPrefix">Request path prefix to require authentication for.</param>
        /// <returns>The application builder being modified.</returns>
        public IApplicationBuilder RequireAuthenticationOn(string pathPrefix)
        {
            app.Use((context, next) =>
            {
                if (PathMatches(context, pathPrefix) && !UserAuthenticated(context))
                {
                    return context.ChallengeAsync();
                }

                return next();
            });

            return app;
        }

        /// <summary>
        /// Sets up middleware forbidding access if the logged in user doesn't have at least
        /// one of the specified roles for the given path prefix.
        /// </summary>
        /// <param name="pathPrefix">Request path prefix to require authorization for.</param>
        /// <param name="roles">Roles permitted access to the given path.</param>
        /// <returns>The application builder being modified.</returns>
        public IApplicationBuilder RequireAuthorizationRoleOn(string pathPrefix, params string[] roles)
        {
            app.Use((context, next) =>
            {
                if (PathMatches(context, pathPrefix)
                    && roles.All(roleName => !UserAuthorized(context, roleName)))
                {
                    return context.ForbidAsync();
                }

                return next();
            });

            return app;
        }
    }
}
