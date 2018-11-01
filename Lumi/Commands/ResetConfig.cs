using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class ResetConfig : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "Reset-Config";

        public ShellResult Execute( IReadOnlyList<string> input )
        {
            AppConfig.SaveDefaultConfig();
            Program.ReloadConfig();
            return ShellResult.Ok();
        }
    }
}
