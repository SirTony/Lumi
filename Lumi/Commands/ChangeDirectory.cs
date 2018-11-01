using System;
using System.Collections.Generic;
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
    internal sealed class ChangeDirectory : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        [ArgDefaultValue( "" )]
        private string Path { get; set; }

        [ArgIgnore]
        public string Name { get; } = "cd";

        public ShellResult Execute( IReadOnlyList<string> input )
        {
            if( String.IsNullOrWhiteSpace( this.Path ) )
                this.Path = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );

            var fullPath = System.IO.Path.GetFullPath( ShellUtil.ProcessTilde( this.Path ) );

            if( !Directory.Exists( fullPath ) )
                return ShellResult.Error( -1, $"cd: directory '{this.Path}' could not be found" );

            Directory.SetCurrentDirectory( fullPath );
            return ShellResult.Ok();
        }
    }
}
