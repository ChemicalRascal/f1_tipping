using F1Tipping.Common;
using F1Tipping.Data.AppModel;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
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
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<RacingEntity> RacingEntities { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<DriverTeam> DriverTeams { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Tip> Tips { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var parentType in new Type[] {
                typeof(Event),
                typeof(RacingEntity),
                typeof(Result)})
            {
                foreach (var type in parentType.GetDerivedClasses())
                {
                    builder.Entity(type);
                }
            }

            // TODO: Clean all this up, figure out lazy loading

            builder.Entity<Player>().OwnsOne(player => player.Details);
            builder.Entity<Player>().Navigation(player => player.Details).AutoInclude();

            builder.Entity<Season>(b =>
            {
                b.HasMany(s => s.Rounds).WithOne(r => r.Season);
                b.Navigation(s => s.Rounds);
            });

            builder.Entity<Round>().HasOne(round => round.Season);
            builder.Entity<Round>().Navigation(round => round.Season).AutoInclude();
            builder.Entity<Round>().HasMany(round => round.Events).WithOne(race => race.Weekend);
            builder.Entity<Round>().Navigation(round => round.Events).AutoInclude();

            builder.Entity<Race>().Navigation(race => race.Weekend).AutoInclude();

            builder.Entity<Tip>().HasOne(tip => tip.Target);
            builder.Entity<Tip>().Navigation(tip => tip.Target).AutoInclude();
            builder.Entity<Tip>().HasOne(tip => tip.Selection);
            builder.Entity<Tip>().Navigation(tip => tip.Selection).AutoInclude();

            builder.Entity<Team>(b =>
            {
                b.HasMany(team => team.DriverTeams).WithOne(dt => dt.Team);
                b.Navigation(team => team.DriverTeams).AutoInclude();
            });

            builder.Entity<DriverTeam>(b =>
            {
                b.Navigation(dt => dt.Driver).AutoInclude();
                b.Navigation(dt => dt.Team).AutoInclude();
            });

            builder.Entity<DriverTeam>().HasOne(driver => driver.Driver);
            builder.Entity<DriverTeam>().Navigation(driver => driver.Driver).AutoInclude();
            builder.Entity<DriverTeam>().HasOne(driver => driver.Team);
            builder.Entity<DriverTeam>().Navigation(driver => driver.Team).AutoInclude();

            builder.Entity<Result>().HasOne(result => result.Event);
            builder.Entity<Result>().HasKey(nameof(Result.Event)+"Id", nameof(Result.Type));

            builder.Entity<MultiEntityResult>().HasMany(result => result.ResultHolders).WithMany();
            builder.Entity<MultiEntityResult>().Navigation(result => result.ResultHolders).AutoInclude();

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

        public async Task<bool> CreatePlayerIfNeededAsync(User user)
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
