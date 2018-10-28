using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class SequenceParselet : IInfixParselet
    {
        public Precedence Precedence { get; } = Precedence.Sequence;
        public bool Safe { get; }

        public SequenceParselet( bool safe ) => this.Safe = safe;

        public IShellSegment Parse( ShellParser parser, IShellSegment parent, IShellSegment left, ShellToken token )
        {
            var right = parser.Parse( this.Precedence );
            var seq = new SequenceSegment( parent, left, right, this.Safe );

            left.Parent = seq;
            right.Parent = seq;

            return seq;
        }
    }
}
