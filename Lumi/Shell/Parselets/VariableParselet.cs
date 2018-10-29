using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class VariableParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
        {
            if( parser.Peek().Kind != ShellTokenKind.LeftSquare )
                return new VariableSegment( parent, parser.Take( ShellTokenKind.Literal, token ).Text );

            parser.Take( ShellTokenKind.LeftSquare, token );
            var scope = parser.Take( ShellTokenKind.Literal, token );
            parser.Take( ShellTokenKind.RightSquare, token );

            var name = parser.Take( ShellTokenKind.Literal, token );
            return new VariableSegment( parent, scope.Text, name.Text );
        }
    }
}
