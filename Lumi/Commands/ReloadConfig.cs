using System.Collections.Generic;
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
    internal sealed class ReloadConfig : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "Reload-Config";

        public ShellResult Execute( IReadOnlyList<string> input )
        {
            Program.ReloadConfig();
            return ShellResult.Ok();
        }
    }
}
