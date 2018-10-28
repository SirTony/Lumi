using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class InterpolationParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
        {
            parser.Take( ShellTokenKind.LeftParen, token );
            parser.ClearSegmentOverrides(); // in case we've recursed from a CommandParselet
            var segment = parser.Parse();
            parser.Take( ShellTokenKind.RightParen, token );

            var interp = new InterpolationSegment( parent, segment );
            segment.Parent = interp;

            return interp;
        }
    }
}
