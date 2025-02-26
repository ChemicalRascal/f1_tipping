using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Model
{
    public enum ResultType
    {
        NotSet,
        [Display(Name = "Pole Position")]
        PolePosition,
        [Display(Name = "First Place")]
        FirstPlace,
        [Display(Name = "Second Place")]
        SecondPlace,
        [Display(Name = "Third Place")]
        ThirdPlace,
        [Display(Name = "First DNF")]
        FirstDnf,
        [Display(Name = "Driver's Championship")]
        DriversChampionship,
        [Display(Name = "Constructor's Championship")]
        ConstructorsChampionship,
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
                _ => throw new NotImplementedException(),
            };
        }
    }
}