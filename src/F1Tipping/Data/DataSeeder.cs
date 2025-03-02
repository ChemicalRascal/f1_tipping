using F1Tipping.Model;

namespace F1Tipping.Data
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
            foreach (var race in Races2025)
            {
                _modelDb.Add(race);
            }
            foreach (var sprintRace in SprintRaces2025)
            {
                _modelDb.Add(sprintRace);
            }

            foreach (var team in Teams2025.Values)
            {
                _modelDb.Add(team);
            }
            foreach (var driver in Drivers2025)
            {
                _modelDb.Add(driver);
            }

            await _modelDb.SaveChangesAsync();
        }

        private static Season Season2025 = new() { Year = new(2025) };

        private static List<Round> Rounds2025 = [
            new() { Season = Season2025, Index = 1, Title="Australia",      StartDate = new(2025, 03, 14, 01, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 2, Title="China",          StartDate = new(2025, 03, 21, 03, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 3, Title="Japan",          StartDate = new(2025, 04, 04, 02, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 4, Title="Bahrain",        StartDate = new(2025, 04, 11, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 5, Title="Saudi Arabia",   StartDate = new(2025, 04, 18, 13, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 6, Title="Miami",          StartDate = new(2025, 05, 02, 16, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 7, Title="Emilia-Romagna", StartDate = new(2025, 05, 16, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 8, Title="Monaco",         StartDate = new(2025, 05, 23, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 9, Title="Spain",          StartDate = new(2025, 05, 30, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 10, Title="Canada",        StartDate = new(2025, 06, 13, 17, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 11, Title="Austria",       StartDate = new(2025, 06, 27, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 12, Title="Great Britain", StartDate = new(2025, 07, 04, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 13, Title="Belgium",       StartDate = new(2025, 07, 25, 10, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 14, Title="Hungary",       StartDate = new(2025, 08, 01, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 15, Title="Netherlands",   StartDate = new(2025, 08, 29, 10, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 16, Title="Italy",         StartDate = new(2025, 09, 05, 11, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 17, Title="Azerbaijan",    StartDate = new(2025, 09, 19, 08, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 18, Title="Singapore",     StartDate = new(2025, 10, 03, 09, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 19, Title="United States", StartDate = new(2025, 10, 17, 17, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 20, Title="Mexico",        StartDate = new(2025, 10, 24, 18, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 21, Title="Brazil",        StartDate = new(2025, 11, 07, 14, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 22, Title="Las Vegas",     StartDate = new(2025, 11, 21, 00, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 23, Title="Qatar",         StartDate = new(2025, 11, 28, 13, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            new() { Season = Season2025, Index = 24, Title="Abu Dhabi",     StartDate = new(2025, 12, 05, 09, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset), Events = [] },
            ];

        private static List<DateTimeOffset> QualiStartTimes2025 = [
            new(2025, 03, 15, 05, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 03, 22, 07, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 05, 06, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 12, 16, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 19, 17, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 03, 20, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 17, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 24, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 31, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 06, 14, 20, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 06, 28, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 05, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 26, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 08, 02, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 08, 30, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 09, 06, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 09, 20, 12, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 04, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 18, 21, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 25, 21, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 08, 18, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 22, 04, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 29, 18, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 12, 06, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            ];

        private static List<DateTimeOffset> SprintQualiStartTimes2025 = [
            new(2025, 03, 21, 07, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 02, 20, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 25, 14, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 17, 21, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 07, 18, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 28, 17, 30, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            ];

        private static List<DateTimeOffset> RaceStartTimes2025 = [
            new(2025, 03, 16, 04, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 03, 23, 07, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 06, 05, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 13, 15, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 04, 20, 17, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 04, 20, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 18, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 25, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 06, 01, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 06, 15, 18, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 06, 29, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 06, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 27, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 08, 03, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 08, 31, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 09, 07, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 09, 21, 11, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 05, 12, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 19, 19, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 26, 20, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 09, 17, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 23, 04, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 30, 16, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 12, 07, 13, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            ];

        private static List<DateTimeOffset> SprintRaceStartTimes2025 = [
            new(2025, 03, 22, 03, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 05, 03, 16, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 07, 26, 10, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 10, 18, 17, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 08, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            new(2025, 11, 29, 14, 00, 00, TimeZoneInfo.Utc.BaseUtcOffset),
            ];

        private static IEnumerable<Race> Races2025 = RaceStartTimes2025.Select(raceStart =>
            new Race() {
                Type = RaceType.Main,
                RaceStart = raceStart,
                QualificationStart = QualiStartTimes2025
                                    .Where(qualiStart => qualiStart < raceStart)
                                    .OrderByDescending(qualiStart => qualiStart).First(),
                Weekend = Rounds2025.Where(round => round.StartDate < raceStart)
                                    .OrderByDescending(round => round.StartDate).First(),
            });

        private static IEnumerable<Race> SprintRaces2025 = SprintRaceStartTimes2025.Select(raceStart =>
            new Race() {
                Type = RaceType.Sprint,
                RaceStart = raceStart,
                QualificationStart = SprintQualiStartTimes2025
                                    .Where(qualiStart => qualiStart < raceStart)
                                    .OrderByDescending(qualiStart => qualiStart).First(),
                Weekend = Rounds2025.Where(round => round.StartDate < raceStart)
                                    .OrderByDescending(round => round.StartDate).First(),
            });

        private static List<(string first, string last, string nationality,
            string teamname, string number)> DriverNames2025 = [
                ("Oscar","Piastri","Australia","McLaren","81"),
                ("Lando","Norris","United Kingdom","McLaren","4"),
                ("Charles","Leclerc","Monaco","Ferrari","16"),
                ("Lewis","Hamilton","United Kingdom","Ferrari","44"),
                ("Max","Verstappen","Netherlands","Red Bull Racing","1"),
                ("Liam","Lawson","New Zealand","Red Bull Racing","30"),
                ("George","Russell","United Kingdom","Mercedes","63"),
                ("Andrea Kimi","Antonelli","Italy","Mercedes","12"),
                ("Lance","Stroll","Canada","Aston Martin","18"),
                ("Fernando","Alonso","Spain","Aston Martin","14"),
                ("Pierre","Gasly","France","Alpine","10"),
                ("Jack","Doohan","Australia","Alpine","7"),
                ("Isack","Hadjar","France","Racing Bulls","6"),
                ("Yuki","Tsunoda","Japan","Racing Bulls","22"),
                ("Esteban","Ocon","France","Haas","31"),
                ("Oliver","Bearman","United Kingdom","Haas","87"),
                ("Alexander","Albon","Thailand","Williams","23"),
                ("Carlos","Sainz","Spain","Williams","55"),
                ("Nico","Hulkenberg","Germany","Kick Sauber","27"),
                ("Gabriel","Bortoleto","Brazil","Kick Sauber","5"),
            ];

        private static IDictionary<string, Team> Teams2025 = DriverNames2025
            .Select(dn => dn.teamname).Distinct()
            .ToDictionary(tn => tn, tn => new Team() { Name = tn });

        private static IEnumerable<Driver> Drivers2025 = DriverNames2025
            .Select(dn => new Driver() {
                FirstName = dn.first,
                LastName = dn.last,
                Nationality = dn.nationality,
                Number = dn.number,
                Team = Teams2025[dn.teamname],
            });
    }
}
