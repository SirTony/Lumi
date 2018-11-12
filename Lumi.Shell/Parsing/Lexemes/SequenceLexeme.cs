using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal sealed class SequenceLexeme : IBinaryLexeme
    {
        private readonly bool _safe;

        public SequenceLexeme( bool safe ) => this._safe = safe;
        public Precedence Precedence { get; } = Precedence.Sequence;

        public IShellSegment Parse( ShellParser parser, IShellSegment left, SyntaxToken<ShellTokenKind> token )
        {
            var right = parser.Parse( this.Precedence );
            return new SequenceSegment( left, right, this._safe );
        }
    }
}
