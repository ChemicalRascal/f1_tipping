using F1Tipping.Model;
using F1Tipping.Model.Tipping;

namespace F1Tipping.Tipping
{
    public class TipValidiationService
    {
        public IDictionary<ResultType, string> GetErrors(IList<IThinTip> tips)
        {
            var errors = new Dictionary<ResultType, string>();

            var tipMap = tips.ToDictionary(t => t.Type);
            foreach (var rt in tipMap.Keys)
            {
                var rtMemberInfo = rt.GetType().GetMember(rt.ToString());
                var attributes = rtMemberInfo
                    .Select(mi => mi.GetCustomAttributes(false))
                    .SelectMany(a => a);

                foreach (var a in attributes)
                {
                    if (a is MustNotEqualAttribute mustNotEqual)
                    {
                        foreach (var otherType in mustNotEqual.Others)
                        {
                            if (tipMap[otherType].Selection == tipMap[rt].Selection)
                            {
                                var newError = $"Must not match {otherType}";
                                errors[rt] = errors.TryGetValue(rt, out var current)
                                    ? current + ", " + newError
                                    : newError;
                            }
                        }
                    }
                }
            }

            return errors;
        }
    }
}
