using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumi.Shell
{
    internal sealed class ShellLexer
    {
        private static readonly IReadOnlyDictionary<char, ShellTokenKind> Punctuation;

        static ShellLexer()
        {
            Punctuation = new Dictionary<char, ShellTokenKind>
            {
                ['$'] = ShellTokenKind.Dollar,
                ['#'] = ShellTokenKind.Octothorpe,
                [';'] = ShellTokenKind.Semicolon,
                //[':'] = ShellTokenKind.Colon,
                ['&'] = ShellTokenKind.Ampersand,
                ['|'] = ShellTokenKind.Pipe,
                ['<'] = ShellTokenKind.LeftAngle,
                ['>'] = ShellTokenKind.RightAngle,
                ['('] = ShellTokenKind.LeftParen,
                [')'] = ShellTokenKind.RightParen,
                ['['] = ShellTokenKind.LeftSquare,
                [']'] = ShellTokenKind.RightSquare,
            };
        }

        private readonly string _commandLine;
        private readonly int _length;
        private int _index;

        public bool EndOfInput => this._index >= this._length;

        public ShellLexer( string commandLine )
        {
            this._commandLine = commandLine;
            this._length = commandLine.Length;
        }

        public static bool IsSpecialChar( char c )
            => Punctuation.ContainsKey( c );

        public IEnumerable<ShellToken> Tokenize()
        {
            this._index = 0;

            while( !this.EndOfInput )
            {
                this.SkipWhile( Char.IsWhiteSpace );

                if( this.EndOfInput )
                    break;

                var c = this.Peek();
                var i = this._index;
                var success = this.TryLexLiteral( c, out var token )
                           || this.TryLexPunctuation( c, out token );

                if( !success )
                    throw new ShellSyntaxException( $"unexpected character '{c}' (0x{Convert.ToUInt16( c ):X4}) at position {i}", i, i );

                yield return token;
            }

            yield return new ShellToken( ShellTokenKind.EndOfInput, null, this._index );
        }

        private bool TryLexLiteral( char c, out ShellToken token )
        {
            if( Punctuation.ContainsKey( c ) || Char.IsControl( c ) )
            {
                token = null;
                return false;
            }

            var start = this._index;
            var sb = new StringBuilder();
            var isQuoted = c == '\'' || c == '"';
            var terminator = isQuoted ? c : default;

            if( isQuoted )
            {
                this.Take();
                c = this.Peek();
            }

            while( !this.EndOfInput && ( ( isQuoted && c != terminator ) || ( !Char.IsWhiteSpace( c ) && !Char.IsControl( c ) && !Punctuation.ContainsKey( c ) ) ) )
            {
                if( c == terminator )
                    break;

                if( c == '\\' && this.Peek( 1 ) == terminator )
                    this.Take();

                sb.Append( this.Take() );
                c = this.Peek();
            }

            if( isQuoted )
            {
                var last = this.Take();
                if( last == default || last != terminator )
                    throw new ShellSyntaxException( $"unexpected end-of-input, unterminated quoted literal at position {start}", start, this._index );
            }

            token = new ShellToken( ShellTokenKind.Literal, sb.ToString(), start );
            return true;
        }

        private bool TryLexPunctuation( char c, out ShellToken token )
        {
            var start = this._index;
            if( Punctuation.TryGetValue( c, out var kind ) )
            {
                if( kind == ShellTokenKind.RightAngle )
                {
                    if( this.TakeIfNext( ">>>" ) )
                    {
                        token = new ShellToken( ShellTokenKind.TripleRightAngle, ">>>", start );
                        return true;
                    }

                    if( this.TakeIfNext( ">>" ) )
                    {
                        token = new ShellToken( ShellTokenKind.DoubleRightAngle, ">>", start );
                        return true;
                    }
                }

                token = new ShellToken( kind, this.Take().ToString(), start );
                return true;
            }

            token = null;
            return false;
        }

        private char Peek( int distance = 0 )
        {
            var newIndex = this._index + distance;
            return newIndex >= this._length || newIndex < 0 ? default : this._commandLine[newIndex];

        }

        private char Take()
        {
            var c = this.Peek();
            ++this._index;
            return c;
        }

        private bool IsNext( string search )
            => this._index + search.Length >= this._length
             ? false
             : this._commandLine.Substring( this._index, search.Length ) == search;

        private bool TakeIfNext( string search )
        {
            if( this.IsNext( search ) )
            {
                Enumerable.Range( 1, search.Length ).ForEach( _ => this.Take() );
                return true;
            }

            return false;
        }

        private void SkipWhile( Predicate<char> pred )
        {
            while( !this.EndOfInput && pred( this.Peek() ) )
                this.Take();
        }
    }
}
