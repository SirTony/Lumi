using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class VariableParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
        {
            var left = parser.Take( ShellTokenKind.Literal, token );

            if( parser.Peek().Kind != ShellTokenKind.Colon )
                return new VariableSegment( parent, left.Text );

            parser.Take( ShellTokenKind.Colon, token );
            var right = parser.Take( ShellTokenKind.Literal, token );
            return new VariableSegment( parent, left.Text, right.Text );
        }
    }
}
