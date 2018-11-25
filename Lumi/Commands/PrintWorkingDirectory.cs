using System.Diagnostics.CodeAnalysis;
using Lumi.CommandLine;
using Lumi.Core;
using Lumi.Shell;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    [Description( "Prints the current working directory" )]
    [Usage( "pwd" )]
    internal sealed class PrintWorkingDirectory : ICommand
    {
        [Named( 'e', "expand-tilde" )]
        [Description( "Expands the tilde to the absolute path." )]
        public bool ExpandTilde { get; private set; }

        public string Name { get; } = "pwd";

        public ShellResult Execute( AppConfig config, object input )
            => ShellResult.Ok( ShellUtility.GetCurrentDirectory( this.ExpandTilde ) );
    }
}
