using System;
using EnsureThat;

namespace Lumi.Core
{
    public static class StringParamExtensions
    {
        /// <summary>
        ///     Ensure that a string is not empty and that it does not consist only of whitespace.
        ///     String may still be <see langword="null" />
        /// </summary>
        /// <param name="param"></param>
        public static void IsNotEmptyOrWhitespace( this StringParam param )
        {
            if( param.Value is null )
                return;

            if( String.IsNullOrWhiteSpace( param.Value ) )
                throw new ArgumentException( param.Name );
        }
    }
}
