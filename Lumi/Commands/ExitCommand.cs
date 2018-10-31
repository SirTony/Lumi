using System;
using System.Diagnostics.CodeAnalysis;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class ExitCommand : ICommand
    {
        [CustomHelpHook( "exit" )]
        [ArgShortcut( "?" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        [ArgDefaultValue( 0 )]
        public int ExitCode { get; set; }

        [ArgIgnore]
        public string Name { get; } = "exit";

        public ShellResult Execute()
        {
            Environment.Exit( this.ExitCode );
            return default;
        }
    }
}
