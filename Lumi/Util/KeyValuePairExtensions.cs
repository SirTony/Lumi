namespace System.Collections.Generic
{
    internal static class KeyValuePairExtensions
    {
        public static void Deconstruct<TK, TV>( this KeyValuePair<TK, TV> pair, out TK key, out TV value )
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
