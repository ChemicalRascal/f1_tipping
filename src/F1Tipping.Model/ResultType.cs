using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Model
{
    public enum ResultType
    {
        NotSet,

        [Display(Name = "Driver's Championship")]
        [Scores(15)]
        DriversChampionship,

        [Display(Name = "Constructor's Championship")]
        [Scores(15)]
        ConstructorsChampionship,

        [Display(Name = "Pole Position")]
        [Scores(1)]
        PolePosition,

        [Display(Name = "First Place")]
        [MustNotEqual(SecondPlace, ThirdPlace)]
        [Scores(2, 1, SecondPlace, ThirdPlace)]
        FirstPlace,

        [Display(Name = "Second Place")]
        [MustNotEqual(FirstPlace, ThirdPlace)]
        [Scores(2, 1, FirstPlace, ThirdPlace)]
        SecondPlace,

        [Display(Name = "Third Place")]
        [MustNotEqual(FirstPlace, SecondPlace)]
        [Scores(2, 1, FirstPlace, SecondPlace)]
        ThirdPlace,

        [Display(Name = "Fastest Lap")]
        [Scores(1)]
        FastestLap,

        [Display(Name = "First DNF")]
        [Scores(3)]
        FirstDnf,
    }

    public class MustNotEqualAttribute : Attribute
    {
        public ResultType[] Others { get; set; }

        public MustNotEqualAttribute(params ResultType[] others)
        {
            Others = others;
        }
    }

    public class ScoresAttribute(
            int matchPoints,
            int alternatePoints = 0,
            params ResultType[] alternateResults) : Attribute
    {
        public int MatchPoints { get; } = matchPoints;
        public int AlternatePoints { get; } = alternatePoints;
        public ResultType[] AlternateResults { get; } = alternateResults;
    }

    public static class ResultTypeHelper
    {
        public static IEnumerable<Attribute> GetAttributes(ResultType rt)
        {
            var rtMemberInfo = rt.GetType().GetMember(rt.ToString());
            var attributes = rtMemberInfo
                .Select(memberInfo => memberInfo.GetCustomAttributes(false))
                .SelectMany(a => a);
            foreach (var attribute in attributes)
            {
                if (attribute is Attribute)
                {
                    yield return (Attribute)attribute;
                }
            }
        }

        public static IEnumerable<T> GetAttributes<T>(ResultType rt)
        {
            foreach (var attribute in GetAttributes(rt))
            {
                if (attribute is T particular)
                {
                    yield return particular;
                }
            }
        }

        public static IEnumerable<Type> RacingEntityTypes(ResultType result)
        {
            return result switch
            {
                ResultType.PolePosition => [typeof(DriverTeam)],
                ResultType.FirstPlace => [typeof(DriverTeam)],
                ResultType.SecondPlace => [typeof(DriverTeam)],
                ResultType.ThirdPlace => [typeof(DriverTeam)],
                ResultType.FastestLap => [typeof(DriverTeam)],
                ResultType.FirstDnf => [typeof(DriverTeam)],
                ResultType.DriversChampionship => [typeof(DriverTeam)],
                ResultType.ConstructorsChampionship => [typeof(Team)],
                _ => throw new NotImplementedException(),
            };
        }

        public static Type ResultStructure(ResultType result)
        {
            return result switch
            {
                ResultType.PolePosition => typeof(Result),
                ResultType.FirstPlace => typeof(Result),
                ResultType.SecondPlace => typeof(Result),
                ResultType.ThirdPlace => typeof(Result),
                ResultType.FastestLap => typeof(Result),
                ResultType.FirstDnf => typeof(MultiEntityResult),
                ResultType.DriversChampionship => typeof(Result),
                ResultType.ConstructorsChampionship => typeof(Result),
                _ => throw new NotImplementedException(),
            };
        }
    }
}