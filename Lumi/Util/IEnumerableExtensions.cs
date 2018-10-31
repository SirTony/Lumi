using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static string Join<T>( this IEnumerable<T> values, string delimeter = null )
            => String.Join( delimeter ?? String.Empty, values );

        public static IEnumerable<string> InChunksOf( this string source, int chunkSize )
        {
            var len = source.Length;
            for( var i = 0; i < len; i += chunkSize )
                yield return source.Substring( i, Math.Min( chunkSize, len - i ) );
        }

        public static IEnumerable<IEnumerable<T>> InChunksOf<T>( this IEnumerable<T> enumerable, int chunkSize )
        {
            if( enumerable == null )
                throw new ArgumentNullException( nameof( enumerable ) );

            if( chunkSize < 1 )
            {
                throw new ArgumentException(
                    "Chunk size must be a positive integer greater than zero",
                    nameof( chunkSize )
                );
            }

            return chunkSize == 1
                       ? enumerable.Select( item => new[] { item } )
                       : EnumerableExtensions.InChunksOfImpl( enumerable, chunkSize );
        }

        public static void ForEach<T>( this IEnumerable<T> enumerable, Action<T> callback )
        {
            foreach( var item in enumerable )
                callback( item );
        }

        public static bool None<T>( this IEnumerable<T> enumerable )
            => !enumerable.Any();

        public static bool None<T>( this IEnumerable<T> enumerable, Func<T, bool> predicate )
            => !enumerable.Any( predicate );

        public static IEnumerable<T> Reject<T>( this IEnumerable<T> enumerable, Func<T, bool> predicate )
            => enumerable.Where( x => !predicate( x ) );

        private static IEnumerable<IEnumerable<T>> InChunksOfImpl<T>( IEnumerable<T> enumerable, int chunkSize )
        {
            using( var enumerator = enumerable.GetEnumerator() )
            {
                while( enumerator.MoveNext() )
                {
                    int i;
                    var buffer = new T[chunkSize];
                    for( i = 0; i < chunkSize; ++i )
                    {
                        buffer[i] = enumerator.Current;

                        if( i < chunkSize - 1 && !enumerator.MoveNext() )
                            break;
                    }

                    if( i < chunkSize - 1 )
                        Array.Resize( ref buffer, i + 1 );

                    yield return buffer;
                }
            }
        }
    }
}
