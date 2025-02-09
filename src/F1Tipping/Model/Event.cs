namespace F1Tipping.Model
{
    public abstract class Event
    {
        public Guid Id { get; set; }
    }

    public class Race : Event, IEventWithResults
    {
        private static ResultType[] results = [
            ResultType.FirstPlace,
            ResultType.SecondPlace,
            ResultType.ThirdPlace,
            ResultType.PolePosition,
            ResultType.FirstDnf,
        ];

        public required Round Weekend { get; set; }
        public RaceType Type { get; set; }

        public static IEnumerable<ResultType> GetApplicableResultTypes() => results;
    }

    public class Season : Event, IEventWithResults
    {
        private static ResultType[] results = [
            ResultType.DriversChampionship,
            ResultType.ConstructorsChampionship,
        ];

        public DateTime Year { get; set; }

        public static IEnumerable<ResultType> GetApplicableResultTypes() => results;
    }

    public interface IEventWithResults
    {
        public abstract static IEnumerable<ResultType> GetApplicableResultTypes();
    }
}
