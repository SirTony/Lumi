using CommandLine;

namespace Lumi
{
    internal sealed class CommandLineArguments
    {
        [Option(
            'e', "eval",
            Default = null,
            MetaValue = "command",
            Required = false,
            HelpText = "Execute a command then exit without entering interactive mode"
        )]
        public string EvaluateCommand { get; private set; }

        [Option(
            't', "tokens",
            Default = null,
            Required = false,
            HelpText = "Print all tokens in a command"
        )]
        public bool PrintTokens { get; private set; }

        [Option(
            'T', "tree",
            Default = null,
            Required = false,
            HelpText = "Print the parse tree for a command"
        )]
        public bool PrintTree { get; private set; }

        [Option(
            'E', "no-execute",
            Default = null,
            Required = false,
            HelpText = "Do not execute commands"
        )]
        public bool NoExecute { get; private set; }
    }
}
