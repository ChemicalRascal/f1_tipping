﻿using F1Tipping.Common;

namespace F1Tipping.Model
{
    public abstract class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool Completed { get; set; } = false;
        public DateTimeOffset TipsDeadline { get; set; }
        public abstract float OrderKey { get; }
        public abstract string EventName { get; }
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
        public override string EventName =>
            $"{Weekend.Season.Year}, Round {Weekend.Index} - {
                Type switch {
                    RaceType.Main => "Main Race",
                    RaceType.Sprint => "Sprint Race",
                    _ => throw new NotImplementedException(),
                }} - {Weekend.Title}";

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
        public override string EventName => $"{Year} Season";

        public static IEnumerable<ResultType> GetApplicableResultTypes() => results;
        public IEnumerable<ResultType> GetResultTypes() => GetApplicableResultTypes();
    }

    public interface IEventWithResults
    {
        public abstract static IEnumerable<ResultType> GetApplicableResultTypes();
        public IEnumerable<ResultType> GetResultTypes();
    }
}
