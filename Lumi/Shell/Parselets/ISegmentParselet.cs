using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal interface ISegmentParselet
    {
        IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token );
    }
}
