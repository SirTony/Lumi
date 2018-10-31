using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class ChangeDirectoryCommand : ICommand
    {
        [CustomHelpHook( "cd" )]
        [ArgShortcut( "?" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        [ArgDefaultValue( "" )]
        public string Path { get; set; }

        [ArgIgnore]
        public string Name { get; } = "cd";

        public ShellResult Execute()
        {
            if( this.Path == null )
                this.Path = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );

            var fullPath = System.IO.Path.GetFullPath( ShellUtil.ProcessTilde( this.Path ) );

            if( !Directory.Exists( fullPath ) )
                return ShellResult.Error( -1, $"cd: directory '{this.Path}' could not be found" );

            Directory.SetCurrentDirectory( fullPath );
            return ShellResult.Ok();
        }
    }
}
