namespace F1Tipping.Common
{
    public static class Extensions
    {
        public static string? NullIfEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
