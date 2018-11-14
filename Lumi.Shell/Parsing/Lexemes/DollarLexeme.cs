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

            var varToken = parser.Take( ShellTokenKind.String );
            if( varToken.Text is null )
                throw new ShellSyntaxException( "Variable name cannot be a string interpolation", varToken.Span );

            var varName = varToken.Text;
            if( !varName.Contains( ":" ) )
                return new VariableSegment( null, varName );

            var scopeEnd = varName.IndexOf( ':' );

            var scope = varName.Substring( 0, scopeEnd );
            var name = varName.Substring( scopeEnd + 1 );

            return new VariableSegment( scope, name );
        }
    }
}
