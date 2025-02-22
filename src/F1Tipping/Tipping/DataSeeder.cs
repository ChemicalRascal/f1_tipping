using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Tipping
{
    public class DataSeeder
    {
        private ModelDbContext _modelDb;

        public DataSeeder(ModelDbContext modelDb)
        {
            _modelDb = modelDb;
        }

        public async Task Seed2025SeasonAsync()
        {
            _modelDb.Add(Season2025);
            foreach (var round in Rounds2025)
            {
                _modelDb.Add(round);
            }
            await _modelDb.SaveChangesAsync();

            var season = await _modelDb.Seasons.ToListAsync();
        }

        private static Season Season2025 = new() { Year = new(2025), Id = Guid.NewGuid() };
        private static List<Round> Rounds2025 = [
            new() { Season = Season2025, Index = 1, Title="Australia",      StartDate = new(2025, 03, 14, 01, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 2, Title="China",          StartDate = new(2025, 03, 21, 03, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 3, Title="Japan",          StartDate = new(2025, 04, 04, 02, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 4, Title="Bahrain",        StartDate = new(2025, 04, 11, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 5, Title="Saudi Arabia",   StartDate = new(2025, 04, 18, 13, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 6, Title="Miami",          StartDate = new(2025, 05, 02, 16, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 7, Title="Emilia-Romagna", StartDate = new(2025, 05, 16, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 8, Title="Monaco",         StartDate = new(2025, 05, 23, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 9, Title="Spain",          StartDate = new(2025, 05, 30, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 10, Title="Canada",        StartDate = new(2025, 06, 13, 17, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 11, Title="Austria",       StartDate = new(2025, 06, 27, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 12, Title="Great Britain", StartDate = new(2025, 07, 04, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 13, Title="Belgium",       StartDate = new(2025, 07, 25, 10, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 14, Title="Hungary",       StartDate = new(2025, 08, 01, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 15, Title="Netherlands",   StartDate = new(2025, 08, 29, 10, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 16, Title="Italy",         StartDate = new(2025, 09, 05, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 17, Title="Azerbaijan",    StartDate = new(2025, 09, 19, 08, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 18, Title="Singapore",     StartDate = new(2025, 10, 03, 09, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 19, Title="United States", StartDate = new(2025, 10, 17, 17, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 20, Title="Mexico",        StartDate = new(2025, 10, 24, 18, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 21, Title="Brazil",        StartDate = new(2025, 11, 07, 14, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 22, Title="Las Vegas",     StartDate = new(2025, 11, 21, 00, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 23, Title="Qatar",         StartDate = new(2025, 11, 28, 13, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            new() { Season = Season2025, Index = 24, Title="Abu Dhabi",     StartDate = new(2025, 12, 05, 09, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset) },
            ];
    }
}
