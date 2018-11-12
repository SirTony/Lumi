using System;
using System.Collections.Generic;
using Lumi.Parsing;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Lexemes
{
    internal sealed class StringLexeme : IPrimaryLexeme
    {
        public IShellSegment ParseString( ShellParser parser, SyntaxToken<ShellTokenKind> token )
        {
            /* whatif: the top-level token's Data property is a list of tokens,
             *         but any child tokens in that list have their Data property
             *         set to an array of tokens due to how the lexer processes interpolations.
             *         Potentially confusing?
            */
            if( token.Text != null || !( token.Data is List<SyntaxToken<ShellTokenKind>> pieces ) )
                return new TextSegment( token.Text );

            var segments = new List<IShellSegment>();
            foreach( var piece in pieces )
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch( piece.Kind )
                {
                    case ShellTokenKind.String:
                        segments.Add( new TextSegment( piece.Text ) );
                        break;

                    case ShellTokenKind.StringInterpolation:
                        var interpolationParser = new ShellParser(
                            (SyntaxToken<ShellTokenKind>[]) piece.Data
                        );

                        segments.Add( interpolationParser.ParseAll() );
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return new StringInterpolationSegment( segments );
        }

        public IShellSegment Parse( ShellParser parser, SyntaxToken<ShellTokenKind> token )
        {
            var first = this.ParseString( parser, token );
            if( parser.CommandParsingDisabled )
                return first;

            if( !parser.CommandParsingDisabled && !parser.HasSegment() )
                return new CommandSegment( first );

            var args = new List<IShellSegment>();
            while( parser.HasSegment() )
                args.Add( parser.WithoutCommandParsing( () => parser.Parse( Precedence.Command ) ) );

            return new CommandSegment( first, args );
        }
    }
}
