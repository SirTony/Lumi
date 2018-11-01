using System;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class VariableParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
        {
            var literal = parser.Take( ShellTokenKind.Literal, token ).Text;
            if( !literal.Contains( ":" ) )
                return new VariableSegment( parent, Program.Config.DefaultVariableScope, literal );

            var colon = literal.IndexOf( ":", StringComparison.Ordinal );
            var scope = literal.Substring( 0, colon );
            var name = literal.Substring( colon + 1 );

            return new VariableSegment( parent, scope, name );
        }
    }
}
