using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data.AppModel;

namespace F1Tipping.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<SystemSettings> SystemSettings { get; set; }
    public DbSet<GlobalTemporalData> GlobalTemporalData { get; set; }
    public DbSet<PushSubscription> UserPushNotificationSubscriptions { get; set; }
    public DbSet<UserSettings> UserExtraSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(b =>
        {
            b.OwnsOne(u => u.Settings, b =>
            {
                b.WithOwner();
                b.OwnsOne(s => s.NotificationsSettings, b =>
                {
                    b.WithOwner();
                });
                b.Navigation(s => s.NotificationsSettings)
                    .IsRequired(false).AutoInclude();
            });
            b.Navigation(u => u.Settings)
                .IsRequired().AutoInclude();

            b.OwnsOne(u => u.TemporalData, b =>
            {
                b.WithOwner();
            });
            b.Navigation(u => u.TemporalData)
                .IsRequired().AutoInclude();
        });
    }

    public async Task<SystemSettings> GetSystemSettingsAsync()
    {
        return (await SystemSettings.SingleOrDefaultAsync()) ?? new SystemSettings();
    }

    public async Task<GlobalTemporalData> GetGlobalTemporalDataAsync()
    {
        return (await GlobalTemporalData.SingleOrDefaultAsync()) ?? new GlobalTemporalData();
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

public class GlobalTemporalData
{
    public int Id { get; set; }
    public Guid? LastNotifiedRoundId { get; set; } = null;
}

public class PushSubscription
{
    public int Id { get; set; }
    public required User User { get; set; }
    public required string DeviceEndpoint { get; set; }
    public required string PublicKey { get; set; }
    public required string AuthSecret { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastCheck { get; set; }
}