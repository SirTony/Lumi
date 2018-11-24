using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumi.Shell.Parsing;

namespace Lumi.Shell
{
    public static class ShellUtility
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
            => parts.Select( ShellUtility.Escape );

        public static string GetProperDirectoryCapitalization( string dir )
        {
            try
            {
                if( !Directory.Exists( dir ) )
                    return dir;

                var dirInfo = new DirectoryInfo( dir );
                var parentDirInfo = dirInfo.Parent;

                return parentDirInfo is null
                           ? dirInfo.Name
                           : Path.Combine(
                               ShellUtility.GetProperDirectoryCapitalization( parentDirInfo.FullName ),
                               parentDirInfo.GetDirectories( dirInfo.Name )[0].Name
                           );
            }
            catch
            {
                return dir;
            }
        }

        public static string GetProperFilePathCapitalization( string fileName )
        {
            try
            {
                if( !File.Exists( fileName ) )
                    return fileName;

                var fileInfo = new FileInfo( fileName );
                var dirInfo = fileInfo.Directory;

                return dirInfo is null
                           ? fileInfo.FullName
                           : Path.Combine(
                               ShellUtility.GetProperDirectoryCapitalization( dirInfo.FullName ),
                               dirInfo.GetFiles( fileInfo.Name )[0].Name
                           );
            }
            catch
            {
                return fileName;
            }
        }

        public static string ProcessTilde( string path )
        {
            if( !path.StartsWith( "~" ) )
                return ShellUtility.GetProperDirectoryCapitalization( path );

            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
            var withoutTilde = path.Substring( 1 )
                                   .Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar )
                                   .Trim( Path.DirectorySeparatorChar )
                                   .Split(
                                        new[] { Path.DirectorySeparatorChar },
                                        StringSplitOptions.RemoveEmptyEntries
                                    );

            var newPath = Path.Combine( withoutTilde.Prepend( home ).ToArray() );
            return ShellUtility.GetProperDirectoryCapitalization( newPath );
        }

        public static string GetCurrentDirectory()
        {
            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ).ToLowerInvariant();
            var current = ShellUtility.GetProperDirectoryCapitalization( Directory.GetCurrentDirectory() );

            return current.ToLowerInvariant().StartsWith( home )
                       ? $"~{current.Substring( home.Length )}"
                       : current;
        }

        public static string WordWrap( string text, int startIndex )
        {
            var width = Console.BufferWidth - startIndex - 1;
            if( text.Length < width )
                return text;

            var lines = text.InChunksOf( width ).Select( x => x.Trim() ).ToArray();
            var indent = new string( ' ', startIndex );

            return $"{lines.First()}{Environment.NewLine}"
                 + $"{lines.Skip( 1 ).Select( x => $"{indent}{x}" ).Join( Environment.NewLine )}";
        }
    }
}
