using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class RedirectionParselet : IInfixParselet
    {
        public Precedence Precedence { get; } = Precedence.Redirection;
        public RedirectionSegment.RedirectionMode Mode { get; }

        public RedirectionParselet( RedirectionSegment.RedirectionMode mode ) => this.Mode = mode;

        public IShellSegment Parse( ShellParser parser, IShellSegment parent, IShellSegment left, ShellToken token )
        {
            var right = parser.Take( ShellTokenKind.Literal, token ).Text;
            var redirect = new RedirectionSegment( parent, left, right, this.Mode );

            left.Parent = redirect;
            return redirect;
        }
    }
}
