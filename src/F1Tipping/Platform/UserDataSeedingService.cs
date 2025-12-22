using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace F1Tipping.Platform;


public partial class UserDataSeedingService(
    IConfiguration configuration,
    IServiceProvider serviceProvider)
{
    private record SeedUser(string Email, string Password, string[] Roles)
    {
        public bool IsValid => !(Email.IsNullOrEmpty() || Password.IsNullOrEmpty());
    }

    private readonly string[] seededRoles = [Role.Administrator, Role.Player];

    public async Task SeedRolesAndUsers()
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            foreach (var role in seededRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new(role)).ThrowOnFailure();
                }
            }

            var users = configuration.GetSection("SeededUsers").Get<SeedUser[]>() ?? [];
            foreach (var user in users)
            {
                if (!user.IsValid)
                {
                    continue;
                }

                var dbUser = await userManager.FindByEmailAsync(user.Email);
                if (dbUser is not null)
                {
                    continue;
                }

                await userManager.CreateAsync(new(user.Email)
                {
                    Email = user.Email,
                    EmailConfirmed = true,
                }, user.Password).ThrowOnFailure();
                dbUser = await userManager.FindByEmailAsync(user.Email);
                await userManager.AddToRolesAsync(dbUser!, user.Roles).ThrowOnFailure();
            }
        }
    }
}

public static class UserSeedingExtensions
{
    extension(IdentityResult result)
    {
        public IdentityResult ThrowOnFailure() => result.Succeeded
            ? result
            : throw new ApplicationException($"Couldn't seed data: {result}");
    }

    extension(Task<IdentityResult> resultTask)
    {
        public async Task<IdentityResult> ThrowOnFailure() =>
            (await resultTask).ThrowOnFailure();
    }
}

public partial class UserDataSeedingService : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        await SeedRolesAndUsers();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}