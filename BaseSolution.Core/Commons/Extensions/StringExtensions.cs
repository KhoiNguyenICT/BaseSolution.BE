namespace BaseSolution.Core.Commons.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCasing(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}