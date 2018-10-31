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
    [SuppressMessage(
        "ReSharper",
        "MemberCanBePrivate.Global",
        Justification = "Setting PowerArgs properties to private hides them from the help screen."
    )]
    internal sealed class Split : ICommand
    {
        [CustomHelpHook( "cd" )]
        [ArgShortcut( "?" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 )]
        [ArgRequired]
        [ArgDescription( "The text to split according to a separator." )]
        public string Text { get; private set; }

        [ArgPosition( 1 )]
        [ArgDescription( "The separator to split the input text by." )]
        public string Separator { get; } = null;

        [ArgDefaultValue( false )]
        [ArgDescription( "Whether or not to retain empty entries." )]
        public bool NoEmpty { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "split";

        public ShellResult Execute( IReadOnlyList<string> input )
        {
            var options = this.NoEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            var parts = this.Separator == null
                            ? input.SelectMany( x => x.Split( new[] { this.Text }, options ) ).ToArray()
                            : this.Text.Split( new[] { this.Separator ?? " " }, options );

            return ShellResult.Ok( parts );
        }
    }
}
