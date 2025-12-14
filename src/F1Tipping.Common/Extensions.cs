using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace F1Tipping.Common;

public static class Extensions
{
    extension(string value)
    {
        public string? NullIfEmpty()
            => string.IsNullOrEmpty(value) ? null : value;
    }

    extension(TimeSpan offset)
    {
        public TimeSpan Abs()
            => offset < TimeSpan.Zero ? (offset * -1) : offset;
    }

    extension(Enum value)
    {
        public string DisplayName()
        {
            var displays = value.GetType().GetField(value.ToString())
                !.GetCustomAttributes<DisplayAttribute>();
            return displays.FirstOrDefault(d => !string.IsNullOrEmpty(d.Name))
                ?.Name ?? value.ToString();
        }

        public string DisplayDescription()
        {
            var displays = value.GetType().GetField(value.ToString())
                !.GetCustomAttributes<DisplayAttribute>();
            return displays.FirstOrDefault(d => !string.IsNullOrEmpty(d.Description))
                ?.Description ?? value.ToString();
        }
    }
}
