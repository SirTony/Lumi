using System;
using System.Collections.Generic;
using System.Linq;
using Lumi.Shell.Parsing;

namespace Lumi.Shell
{
    internal static class Utility
    {
        public static string Escape( string segment )
        {
            if( segment.Any( c => c == '"' ) )
                segment = segment.Replace( "\"", "\\\"" );

            return segment.Any( c => Char.IsWhiteSpace( c ) || ShellLexer.IsSpecialChar( c ) )
                       ? $"\"{segment}\""
                       : segment;
        }

        public static IEnumerable<string> Escape( IEnumerable<string> parts )
            => parts.Select( Utility.Escape );
    }
}
