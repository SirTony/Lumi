using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class Echo : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        public List<string> Text { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "echo";

        public ShellResult Execute( IReadOnlyList<string> input )
        {
            var line = this.Text.Concat( input ?? Array.Empty<string>() ).Join( " " );
            return ShellResult.Ok( line );
        }
    }
}
