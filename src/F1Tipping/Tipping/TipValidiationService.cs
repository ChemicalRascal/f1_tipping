using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using System.ComponentModel.DataAnnotations;

namespace F1Tipping.Tipping
{
    public class TipValidiationService
    {
        public IDictionary<ResultType, string> GetErrors(IEnumerable<IThinTip> tips)
        {
            var errors = new Dictionary<ResultType, string>();

            var tipMap = tips.ToDictionary(t => t.Type);
            foreach (var rt in tipMap.Keys)
            {
                foreach (var mustNotEqual in ResultTypeHelper.GetAttributes<MustNotEqualAttribute>(rt))
                {
                    foreach (var otherType in mustNotEqual.Others)
                    {
                        if (tipMap[otherType].Selection == tipMap[rt].Selection)
                        {
                            var otherTypeName = ResultTypeHelper
                                .GetAttributes<DisplayAttribute>(otherType)
                                .First().Name;
                            var newError = $"Must not match {otherTypeName}";

                            errors[rt] = errors.TryGetValue(rt, out var current)
                                ? current + ", " + newError
                                : newError;
                        }
                    }
                }
            }

            return errors;
        }
    }
}
