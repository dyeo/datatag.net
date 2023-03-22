namespace Datatag
{
    using System.Collections.Generic;

    public static class EnumerableE
    {
        internal static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> input, int start = 0)
        {
            int i = start;
            foreach (var t in input)
            {
                yield return (i++, t);
            }
        }
    }
}