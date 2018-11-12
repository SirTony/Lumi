using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal interface IPrimaryLexeme
    {
        IShellSegment Parse( ShellParser parser, SyntaxToken<ShellTokenKind> token );
    }
}
