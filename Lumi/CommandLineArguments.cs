using System.Diagnostics.CodeAnalysis;
using PowerArgs;

namespace Lumi
{
    [ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class CommandLineArguments
    {
        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgDefaultValue( "" )]
        [ArgShortcut( "e" )]
        [ArgDescription( "Evaluate a command then exit without entering interactive mode." )]
        public string EvaluateCommand { get; private set; }

        [ArgDefaultValue( false )]
        [ArgShortcut( ArgShortcutPolicy.NoShortcut )]
        [ArgDescription( "Print all tokens for a given command." )]
        public bool PrintTokens { get; private set; }

        [ArgDefaultValue( false )]
        [ArgShortcut( ArgShortcutPolicy.NoShortcut )]
        [ArgDescription( "Print the parse tree for a given command." )]
        public bool PrintTree { get; private set; }

        [ArgDefaultValue( false )]
        [ArgShortcut( "n" )]
        [ArgDescription( "Parse but do not execute the given command." )]
        public bool NoExecute { get; private set; }
    }
}
