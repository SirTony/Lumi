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
    [CommandAlias( "cls" )]
    internal sealed class ClearScreen : ICommand
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "clear";

        public ShellResult Execute( AppConfig config, object input )
        {
            Console.Clear();
            return ShellResult.Ok();
        }
    }
}
