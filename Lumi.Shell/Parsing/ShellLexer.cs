using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lumi.Parsing;
using ShellToken = Lumi.Parsing.SyntaxToken<Lumi.Shell.Parsing.ShellTokenKind>;

namespace Lumi.Shell.Parsing
{
    public sealed class ShellLexer : LexerBase<ShellTokenKind>
    {
        private enum LexerMode { Normal, StringInterpolation }

        private const char InterpolationStart = '{';
        private const char InterpolationEnd = '}';

        private static readonly IReadOnlyDictionary<string, ShellTokenKind> Punctuation;

        private readonly LexerMode _mode;

        static ShellLexer() => ShellLexer.Punctuation = new Dictionary<string, ShellTokenKind>
        {
            ["$"] = ShellTokenKind.Dollar,
            [";"] = ShellTokenKind.Semicolon,
            ["&"] = ShellTokenKind.Ampersand,
            ["|"] = ShellTokenKind.Pipe,
            ["<"] = ShellTokenKind.LeftAngle,
            [">"] = ShellTokenKind.RightAngle,
            [">>"] = ShellTokenKind.DoubleRightAngle,
            [">>>"] = ShellTokenKind.TripleRightAngle,
            ["("] = ShellTokenKind.LeftParen,
            [")"] = ShellTokenKind.RightParen
        };

        public ShellLexer( string source ) : base( source )
            => this._mode = LexerMode.Normal;

        private ShellLexer( ShellLexer parent ) : base( parent.Source )
        {
            this._mode = LexerMode.StringInterpolation;
            this.Index = parent.Index;
            this.Line = parent.Line;
            this.Column = parent.Column;
        }

        public static bool IsSpecialChar( char c )
            => ShellLexer.Punctuation.SelectMany( x => x.Key.ToCharArray() ).Distinct().Contains( c );

        public override IEnumerable<ShellToken> Tokenize()
        {
            // This makes the lexer reusable.
            if( this._mode == LexerMode.Normal )
            {
                this.Index = 0;
                this.Line = 1;
                this.Column = 1;
            }

            if( this.EndOfInput && this.Index > 0 && this._mode == LexerMode.StringInterpolation )
                throw new InvalidOperationException( "String interpolation lexer is not reusable" );

            while( !this.EndOfInput )
            {
                this.SkipWhile( Char.IsWhiteSpace );
                if( this.EndOfInput ) break;

                var c = this.Peek();
                if( this._mode == LexerMode.StringInterpolation && c == ShellLexer.InterpolationEnd )
                    break;

                var success = this.TryLexQuotedString( c, out var token )
                           || this.TryLexUnquotedString( c, out token )
                           || this.TryLexPunctuation( c, out token );

                if( !success )
                {
                    throw new ShellSyntaxException(
                        $"Unexpected character '{c}' (0x{Convert.ToUInt16( c ):X4})",
                        (TextSpan) this.CurrentLocation
                    );
                }

                yield return token;
            }

            yield return this.MakeToken( null, ShellTokenKind.EndOfInput );
        }

        private bool TryLexUnquotedString( char c, out ShellToken token )
        {
            if( !IsValid( c ) )
            {
                token = default;
                return false;
            }

            this.MarkLocation();
            token = this.MakeToken( this.TakeWhile( IsValid ), ShellTokenKind.String );
            return true;

            bool IsValid( char ch )
                => !Char.IsWhiteSpace( ch ) && !Char.IsControl( ch ) && !ShellLexer.IsSpecialChar( ch );
        }

        private bool TryLexPunctuation( char c, out ShellToken token )
        {
            this.MarkLocation();
            foreach( var pair in ShellLexer.Punctuation )
            {
                if( this.TakeIfNext( pair.Key ) )
                {
                    token = this.MakeToken( pair.Key, pair.Value );
                    return true;
                }
            }

            this.PopLocation();
            token = default;
            return false;
        }

        private bool TryLexQuotedString( char c, out ShellToken token )
        {
            if( c != '"' && c != '\'' && c != '`' )
            {
                token = default;
                return false;
            }

            this.MarkLocation();

            var interpolationTokens = new List<ShellToken>();
            var sb = new StringBuilder();
            var terminator = this.Take();
            var justTake = false;
            var markNew = false;
            while( !this.EndOfInput && ( c = this.Peek() ) != terminator )
            {
                if( markNew )
                {
                    this.MarkLocation();
                    markNew = false;
                }

                if( justTake )
                {
                    sb.Append( this.Take() );
                    justTake = false;
                    continue;
                }

                switch( c )
                {
                    case '\\':
                        this.Take();
                        justTake = true;
                        continue;

                    case ShellLexer.InterpolationStart:
                        interpolationTokens.Add( this.MakeToken( sb.ToString(), ShellTokenKind.String ) );
                        sb.Clear();
                        markNew = true;

                        this.MarkLocation();
                        this.Take(); // interpolation start
                        var interpolator = new ShellLexer( this );
                        var tokens = (IReadOnlyList<ShellToken>) interpolator.Tokenize().ToArray();

                        /*
                         * NOTE: the tokens variable MUST be enumerated when it is assigned
                         *       otherwise it fucks up index/line/column numbers for the normal mode lexer
                        */

                        if( interpolator.EndOfInput || interpolator.Take() != ShellLexer.InterpolationEnd )
                        {
                            throw new ShellSyntaxException(
                                "Unexpected end of input when processing string interpolation",
                                (TextSpan) this.PopLocation()
                            );
                        }

                        interpolationTokens.Add( this.MakeToken( null, ShellTokenKind.StringInterpolation, tokens ) );

                        this.Index = interpolator.Index;
                        this.Line = interpolator.Line;
                        this.Column = interpolator.Column;

                        break;

                    default:
                        sb.Append( this.Take() );
                        break;
                }
            }

            if( this.EndOfInput || this.Take() != terminator )
            {
                throw new ShellSyntaxException(
                    "Unexpected end of input when processing string",
                    (TextSpan) this.PopLocation()
                );
            }

            var isInterpolated = interpolationTokens.Count > 0;

            if( isInterpolated )
                interpolationTokens.Add( this.MakeToken( sb.ToString(), ShellTokenKind.String ) );

            token = this.MakeToken(
                isInterpolated ? null : sb.ToString(),
                ShellTokenKind.String,
                interpolationTokens
            );

            return true;
        }
    }
}
