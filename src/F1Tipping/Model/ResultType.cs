using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Model
{
    public enum ResultType
    {
        NotSet,
        [Display(Name = "Driver's Championship")]
        DriversChampionship,
        [Display(Name = "Constructor's Championship")]
        ConstructorsChampionship,
        [Display(Name = "Pole Position")]
        PolePosition,
        [Display(Name = "First Place")]
        FirstPlace,
        [Display(Name = "Second Place")]
        SecondPlace,
        [Display(Name = "Third Place")]
        ThirdPlace,
        [Display(Name = "Fastest Lap")]
        FastestLap,
        [Display(Name = "First DNF")]
        FirstDnf,
    }

    public static class ResultTypeHelper
    {
        public static IEnumerable<Type> RacingEntityTypes(ResultType result)
        {
            return result switch
            {
                ResultType.PolePosition => [typeof(Driver)],
                ResultType.FirstPlace => [typeof(Driver)],
                ResultType.SecondPlace => [typeof(Driver)],
                ResultType.ThirdPlace => [typeof(Driver)],
                ResultType.FastestLap => [typeof(Driver)],
                ResultType.FirstDnf => [typeof(Driver)],
                ResultType.DriversChampionship => [typeof(Driver)],
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