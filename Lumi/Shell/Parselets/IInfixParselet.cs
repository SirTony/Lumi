using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal interface IInfixParselet
    {
        Precedence Precedence { get; }

        IShellSegment Parse( ShellParser parser, IShellSegment parent, IShellSegment left, ShellToken token );
    }
}
