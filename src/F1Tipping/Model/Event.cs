using F1Tipping.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace F1Tipping.Model
{
    public abstract class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public abstract float OrderKey { get; }
        //public abstract DateTimeOffset TipsDeadline { get; }
    }

    public class Race : Event, IEventWithResults
    {
        private static ResultType[] results = [
            ResultType.FirstPlace,
            ResultType.SecondPlace,
            ResultType.ThirdPlace,
            ResultType.PolePosition,
            ResultType.FirstDnf,
            ResultType.FastestLap,
        ];

        public RaceType Type { get; set; } = RaceType.NotSet;
        public required Round Weekend { get; set; }
        public required DateTimeOffset RaceStart { get; set; }
        public required DateTimeOffset QualificationStart { get; set; }

        public override float OrderKey => Weekend.Index
            + Type switch
            {
                RaceType.Sprint => 0.1f,
                RaceType.Main => 0.2f,
                _ => throw new NotImplementedException(),
            };

        public static IEnumerable<ResultType> GetApplicableResultTypes() => results;
        public IEnumerable<ResultType> GetResultTypes() => GetApplicableResultTypes();
    }

    public class Season : Event, IEventWithResults
    {
        private static ResultType[] results = [
            ResultType.DriversChampionship,
            ResultType.ConstructorsChampionship,
        ];

        public Year Year { get; set; }
        public List<Round> Rounds { get; set; } = [];

        public override float OrderKey => -1f;

        public static IEnumerable<ResultType> GetApplicableResultTypes() => results;
        public IEnumerable<ResultType> GetResultTypes() => GetApplicableResultTypes();
    }

    public interface IEventWithResults
    {
        public abstract static IEnumerable<ResultType> GetApplicableResultTypes();
        public IEnumerable<ResultType> GetResultTypes();
    }
}
