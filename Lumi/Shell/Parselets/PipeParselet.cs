using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal class PipeParselet : IInfixParselet
    {
        public Precedence Precedence { get; } = Precedence.Pipe;

        public IShellSegment Parse( ShellParser parser, IShellSegment parent, IShellSegment left, ShellToken token )
        {
            var right = parser.Parse( this.Precedence );
            var pipe = new PipeSegment( parent, left, right );

            left.Parent = pipe;
            right.Parent = pipe;

            return pipe;
        }
    }
}
