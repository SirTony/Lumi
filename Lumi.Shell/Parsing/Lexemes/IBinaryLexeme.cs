using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal interface IBinaryLexeme
    {
        Precedence Precedence { get; }

        IShellSegment Parse( ShellParser parser, IShellSegment left, SyntaxToken<ShellTokenKind> token );
    }
}
