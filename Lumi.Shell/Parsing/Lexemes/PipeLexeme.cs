using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal sealed class PipeLexeme : IBinaryLexeme
    {
        public Precedence Precedence { get; } = Precedence.Pipe;

        public IShellSegment Parse( ShellParser parser, IShellSegment left, SyntaxToken<ShellTokenKind> token )
        {
            var right = parser.Parse( this.Precedence );
            return new PipeSegment( left, right );
        }
    }
}
