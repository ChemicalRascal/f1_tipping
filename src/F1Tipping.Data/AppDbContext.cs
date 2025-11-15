using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1Tipping.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<PushSubscription> UserPushNotificationSubscriptions { get; set; }
        public DbSet<UserExtraSettings> UserExtraSettings { get; set; }

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            return (await SystemSettings.SingleOrDefaultAsync()) ?? new SystemSettings();
        }

        public async Task<IEnumerable<PushSubscription>> GetPushSubscriptions(Guid? userId)
        {
            if (userId is null)
            {
                return await UserPushNotificationSubscriptions.ToListAsync();
            }

            return await UserPushNotificationSubscriptions
                .Where(ps => ps.User.Id == userId).ToListAsync();
        }
    }

    // TODO: Move all this to the Model project or something

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
        public DateTime Created { get; set; }
        public DateTime? LastCheck { get; set; }
    }

    public enum NotificationsScheduleType
    {
        NotSet,
        ExponentialDecay,
        FixedPeriods,
    }

    [Owned]
    public class NotificationsSettings
    {
        public TimeSpan TipDeadlineStartOffset { get; set; }
        public bool NotifyForOldTips { get; set; }
        public NotificationsScheduleType ScheduleType { get; set; }
    }

    public class UserExtraSettings
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public required IdentityUser<Guid> User { get; set; }
        public NotificationsSettings? NotificationsSettings { get; set; }
    }
}
