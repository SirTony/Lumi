using System;
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
    internal sealed class Exit : ICommand
    {
        [Positional( 0 )]
        public int ExitCode { get; } = 0;

        public string Name { get; } = "exit";

        public ShellResult Execute( AppConfig config, object input )
        {
            Environment.Exit( this.ExitCode );
            return default;
        }
    }
}
