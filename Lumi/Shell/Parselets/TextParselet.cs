using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class TextParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
            => new TextSegment( parent, token.Text );
    }
}
