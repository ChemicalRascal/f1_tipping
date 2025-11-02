using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<PushSubscription> UserPushNotificationSubscriptions { get; set; }

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            return (await SystemSettings.SingleOrDefaultAsync()) ?? new SystemSettings();
        }

        public async Task<IEnumerable<PushSubscription>> GetPushSubscriptions(Guid UserId)
        {
            return Array.Empty<PushSubscription>();
        }
    }

    public class SystemSettings
    {
        public int Id { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
    }

    public class PushSubscription
    {
        public int Id { get; set; }
        public required IdentityUser<Guid> User { get; set; }
        public required string DeviceEndpoint { get; set; }
        public required string PublicKey { get; set; }
        public required string AuthSecret { get; set; }
    }
}
