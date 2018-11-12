using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal sealed class DollarLexeme : IPrimaryLexeme
    {
        public IShellSegment Parse( ShellParser parser, SyntaxToken<ShellTokenKind> token )
        {
            if( parser.MatchOne( ShellTokenKind.LeftParen ) )
            {
                parser.Take( ShellTokenKind.LeftParen );
                var segment = parser.WithCommandParsing( () => parser.Parse() );
                parser.Take( ShellTokenKind.RightParen );

                return new CommandInterpolationSegment( segment );
            }

            var varSegment = parser.Parse<TextSegment>();

            if( !varSegment.Value.Contains( ":" ) )
                return new VariableSegment( null, varSegment.Value );

            var scopeEnd = varSegment.Value.IndexOf( ':' );

            var scope = varSegment.Value.Substring( 0, scopeEnd );
            var name = varSegment.Value.Substring( scopeEnd + 1 );

            return new VariableSegment( scope, name );
        }
    }
}
