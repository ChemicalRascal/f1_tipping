using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace F1Tipping.Data
{
    public class ModelDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<RacingEntity> RacingEntities { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }
        //public DbSet<Player.Identity> Identities { get; set; }
        public DbSet<Tip> Tips { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var parentType in new Type[] {typeof(Event), typeof(RacingEntity)})
            {
                foreach (var type in parentType.GetDerivedClasses())
                {
                    builder.Entity(type);
                }
            }

            builder.Entity<Player>().OwnsOne(player => player.Details);

            base.OnModelCreating(builder);
        }

        public async Task<bool> CreatePlayerIfNeededAsync(IdentityUser<Guid> user)
        {
            if (await Players.SingleOrDefaultAsync(p => p.AuthUserId == user.Id) is null)
            {
                Players.Add(new()
                {
                    AuthUserId = user.Id,
                    Status = PlayerStatus.Uninitialized,
                });
                await SaveChangesAsync();
                return true;
            }

            return false;
        }
    }

    public static class Extensions
    {
        public static Type[] GetDerivedClasses(this Type type, string[]? ignoreTypeNames = null) =>
            Assembly.GetAssembly(type)?
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(type)
                            && !(ignoreTypeNames?.Any(t.Name.Contains) ?? false))
                    .ToArray() ?? [];
    }
}
