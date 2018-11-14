using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Lumi.Core;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class ChangeDirectory : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        public string Path { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "cd";

        public ShellResult Execute( AppConfig config, object input )
        {
            if( String.IsNullOrWhiteSpace( this.Path ) )
                this.Path = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );

            var fullPath = System.IO.Path.GetFullPath( ShellUtility.ProcessTilde( this.Path ) );

            if( !Directory.Exists( fullPath ) )
                throw new DirectoryNotFoundException( $"cd: directory '{this.Path}' could not be found" );

            Directory.SetCurrentDirectory( fullPath );
            return ShellResult.Ok();
        }
    }
}
