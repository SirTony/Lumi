using System;
using System.Diagnostics.CodeAnalysis;
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
    internal sealed class Exit : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        public int ExitCode { get; } = 0;

        [ArgIgnore]
        public string Name { get; } = "exit";

        public ShellResult Execute( AppConfig config, object input )
        {
            Environment.Exit( this.ExitCode );
            return default;
        }
    }
}
