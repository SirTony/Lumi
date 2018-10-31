using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumi.Config;
using Lumi.Shell.Segments;

namespace Lumi.Shell
{
    internal static class ShellUtil
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
            => parts.Select( ShellUtil.Escape );

        public static IReadOnlyList<string> GetLines( string result )
            => result.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim() ).ToArray();

        public static string GetFriendlyName( this IShellSegment segment )
            => segment.GetType().Name.ToLowerInvariant().Replace( "segment", "" );

        public static string ProcessTilde( string path )
        {
            if( !path.StartsWith( "~" ) || !ConfigManager.Instance.UseTilde )
                return path;
            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
            var withoutTilde = path.Substring( 1 )
                                   .Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar )
                                   .Trim( Path.DirectorySeparatorChar )
                                   .Split(
                                        new[] { Path.DirectorySeparatorChar },
                                        StringSplitOptions.RemoveEmptyEntries
                                    );

            var newPath = Path.Combine( withoutTilde.Prepend( home ).ToArray() );
            return Kernel32.GetWindowsPhysicalPath( newPath );
        }
    }
}
