using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<SystemSettings> SystemSettings { get; set; }

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            return (await SystemSettings.SingleOrDefaultAsync()) ?? new SystemSettings();
        }
    }

    public class SystemSettings
    {
        public int Id { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
    }
}
