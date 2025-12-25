using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using F1Tipping.Common;

namespace F1Tipping.Platform;

public partial class TippingDataSeedingService
{
    private record BlockRecord(
        [Index(0)] string BlockRecordType,
        [Index(1)] string BlockContents,
        [Index(2)] int RecordCount);

    private record Driver(
        [Index(0)] string FirstName,
        [Index(1)] string LastName,
        [Index(2)] string Nationality,
        [Index(3)] string Number,
        [Index(4)] string Team,
        [Index(5)] bool Active);

    private record Team(
        [Index(0)] string Name,
        [Index(1)] bool Active);

    private record Season(
        [Index(0)] int Year,
        [Index(1)] bool Persist);

    private record Round(
        [Index(0)] int Year,
        [Index(1)] int Index,
        [Index(2)] string? Title,
        [Index(3)] DateTimeOffset? RoundStart,
        [Index(4)] DateTimeOffset? QualiStart,
        [Index(5)] DateTimeOffset? RaceStart);

    private record SprintRace(
        [Index(0)] int Year,
        [Index(1)] int RoundIndex,
        [Index(2)] DateTimeOffset? QualiStart,
        [Index(3)] DateTimeOffset? RaceStart);

    private class DataSet
    {
        public List<Driver> Drivers { get; set; } = [];
        public List<Team> Teams { get; set; } = [];
        public List<Season> Seasons { get; set; } = [];
        public List<Round> Rounds { get; set; } = [];
        public List<SprintRace> SprintRaces { get; set; } = [];

        public void AddObject(object? obj) =>
            /* Having to do this crappy workaround is beyond frustrating.
             * Just let me have switch expressions as expression statements,
             * Microsoft.
             */
            (obj switch
            {
                null => (Action)(() => throw new ArgumentNullException(nameof(obj))),
                Driver d => () => Drivers.Add(d),
                Team t => () => Teams.Add(t),
                Season s => () => Seasons.Add(s),
                Round r => () => Rounds.Add(r),
                SprintRace sr => () => SprintRaces.Add(sr),
                _ => throw new NotSupportedException(obj.GetType().FullName)
            })();
    }

    private static class DataFileReader
    {
        public static DataSet ReadFile(string filename)
        {
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,

            };

            var data = new DataSet();

            using (var streamReader = new StreamReader(filename))
            using (var csv = new CsvReader(streamReader, config))
            {
                var expectedType = typeof(BlockRecord);
                var expectedBlockLength = -1;
                while (csv.Read())
                {
                    if (expectedType == typeof(BlockRecord))
                    {
                        (expectedType, expectedBlockLength) = ParseBlockRecord(csv);
                    }
                    else
                    {
                        var record = csv.GetRecord(expectedType);
                        data.AddObject(record);
                        if (--expectedBlockLength <= 0)
                        {
                            expectedType = typeof(BlockRecord);
                        }
                    }
                }
            }

            return data;
        }

        private static (Type, int) ParseBlockRecord(CsvReader csv)
        {
            var rec = csv.GetRecord<BlockRecord>();

            return rec.BlockRecordType switch
            {
                "Header" => (rec.RecordCount != 0
                    ? rec.BlockContents switch
                    {
                        "Driver" => typeof(Driver),
                        "Team" => typeof(Team),
                        "Season" => typeof(Season),
                        "Round" => typeof(Round),
                        "SprintRace" => typeof(SprintRace),
                        _ => throw new NotSupportedException($"Block type of {rec.BlockContents} not supported."),
                    }
                    : typeof(BlockRecord)
                    , rec.RecordCount),
                "Footer" => (typeof(BlockRecord), 0),
                _ => throw new NotSupportedException($"BlockRecordType of {rec.BlockRecordType} not supported."),
            };
        }
    }
}