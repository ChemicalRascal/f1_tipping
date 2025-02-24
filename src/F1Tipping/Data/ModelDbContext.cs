using F1Tipping.Common;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace F1Tipping.Data
{
    public class ModelDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<RacingEntity> RacingEntities { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }
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
            builder.Entity<Player>().Navigation(player => player.Details).AutoInclude();
            builder.Entity<Season>().HasMany(season => season.Rounds);
            builder.Entity<Season>().Navigation(season => season.Rounds).AutoInclude();
            builder.Entity<Round>().HasOne(round => round.Season);
            builder.Entity<Round>().Navigation(round => round.Season).AutoInclude();

            builder.Entity<Result>().HasOne(result => result.Event);
            builder.Entity<Result>().HasKey(nameof(Result.Event)+"Id", nameof(Result.Type));

            base.OnModelCreating(builder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<Year>().HaveConversion<YearConverter>();
            base.ConfigureConventions(builder);
        }

        public class YearConverter : ValueConverter<Year, int>
        {
            public YearConverter() : base(year => year.ToInt(),
                dbVal => new Year(dbVal)) { }
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
