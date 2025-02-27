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
                ResultType.FirstDnf => [typeof(Driver)],
                ResultType.DriversChampionship => [typeof(Driver)],
                ResultType.ConstructorsChampionship => [typeof(Team)],
                ResultType.FastestLap => [typeof(Driver)],
                _ => throw new NotImplementedException(),
            };
        }

        public static Type ResultStructure(ResultType result)
        {
            return result switch
            {
                ResultType.FirstDnf => typeof(MultiEntityResult),
                ResultType.NotSet => throw new NotImplementedException(),
                _ => typeof(Result),
            };
        }
    }
}