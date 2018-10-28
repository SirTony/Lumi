using System.Collections.Generic;

namespace System
{
    internal static class StringExtensions
    {
        public static string Join<T>( this IEnumerable<T> parts, string delim )
            => String.Join( delim, parts );
    }
}
