﻿using System;
using System.IO;
using System.Linq;

namespace Lumi
{
    public static class ShellUtil
    {
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
                               ShellUtil.GetProperDirectoryCapitalization( parentDirInfo.FullName ),
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
                               ShellUtil.GetProperDirectoryCapitalization( dirInfo.FullName ),
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
            if( !path.StartsWith( "~" ) || !Program.Config.UseTilde )
                return ShellUtil.GetProperDirectoryCapitalization( path );

            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
            var withoutTilde = path.Substring( 1 )
                                   .Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar )
                                   .Trim( Path.DirectorySeparatorChar )
                                   .Split(
                                        new[] { Path.DirectorySeparatorChar },
                                        StringSplitOptions.RemoveEmptyEntries
                                    );

            var newPath = Path.Combine( withoutTilde.Prepend( home ).ToArray() );
            return ShellUtil.GetProperDirectoryCapitalization( newPath );
        }
    }
}