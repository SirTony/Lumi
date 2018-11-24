using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Without<T>( this IEnumerable<T> source, T item )
            => source.Where( x => !Object.Equals( x, item ) );

        public static bool Contains<T>( this IEnumerable<T> source, Predicate<T> pred )
            => source.Any( x => pred( x ) );

        public static bool TryGetValue<T>( this IEnumerable<T> source, Predicate<T> pred, out T item )
        {
            item = source.FirstOrDefault( x => pred( x ) );
            return !Object.Equals( item, default( T ) );
        }
    }
}
