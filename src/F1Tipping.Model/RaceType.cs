namespace F1Tipping.Model
{
    public enum RaceType
    {
        NotSet,
        [ScoreMult("1")]
        Main,
        [ScoreMult("0.5")]
        Sprint,
    }

    // TODO: Figure out how to do this without strings?
    public class ScoreMultAttribute(string mult) : Attribute
    {
        public decimal Mult { get; } = decimal.TryParse(mult, out var result)
            ? result
            : throw new ApplicationException($"\"{mult}\" is not decimal!");
    }

    public class RaceTypeHelper
    {
        // TODO: Make these generic Enum extensions
        public static IEnumerable<Attribute> GetAttributes(RaceType rt)
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

        public static IEnumerable<T> GetAttributes<T>(RaceType rt)
        {
            foreach (var attribute in GetAttributes(rt))
            {
                if (attribute is T particular)
                {
                    yield return particular;
                }
            }
        }
    }
}