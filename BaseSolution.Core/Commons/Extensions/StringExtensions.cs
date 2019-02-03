using System.Collections.Generic;
using System.Linq;

namespace BaseSolution.Core.Commons.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCasing(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> input)
        {
            if (input == null) return true;
            var tmp = input as IList<T> ?? input.ToList();
            if (default(T) == null)
                return !tmp.Any() || tmp.All(t => t == null);
            return !tmp.Any();
        }
    }
}