using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal sealed class RedirectionLexeme : IBinaryLexeme
    {
        private readonly RedirectionMode _mode;

        public RedirectionLexeme( RedirectionMode mode ) => this._mode = mode;

        public Precedence Precedence { get; } = Precedence.Pipe;

        public IShellSegment Parse( ShellParser parser, IShellSegment left, SyntaxToken<ShellTokenKind> token )
        {
            var right = parser.WithoutCommandParsing(
                () => parser.Parse<StringInterpolationSegment, TextSegment>( this.Precedence )
            );

            return new RedirectionSegment( left, right, this._mode );
        }
    }
}
