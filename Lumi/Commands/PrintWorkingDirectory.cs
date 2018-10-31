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
    internal sealed class PrintWorkingDirectory : ICommand
    {
        [CustomHelpHook( "cd" )]
        [ArgShortcut( "?" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "pwd";

        public ShellResult Execute()
            => ShellResult.Ok( Program.GetCurrentDirectory() );
    }
}
