using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Lumi.Parsing
{
    public abstract class ParserBase<TKind, TResult> where TKind : Enum
    {
        protected IReadOnlyList<SyntaxToken<TKind>> Tokens { get; }
        protected int Length { get; }
        protected int Index { get; set; }

        protected bool EndOfInput => this.Index >= this.Length;

        protected ParserBase( IEnumerable<SyntaxToken<TKind>> tokens )
        {
            this.Tokens = tokens.ToArray();
            this.Length = this.Tokens.Count;
        }

        public abstract TResult ParseAll();

        public SyntaxToken<TKind> Peek( int distance = 0 )
        {
            var newIndex = this.Index + distance;
            return newIndex >= this.Length || newIndex < 0 ? default : this.Tokens[newIndex];
        }

        public SyntaxToken<TKind> Take()
        {
            if( this.EndOfInput )
                return default;

            var token = this.Peek();
            ++this.Index;
            return token;
        }

        public SyntaxToken<TKind> Take( TKind kind )
        {
            var token = this.Peek();
            if( this.MatchOne( kind ) )
                return this.Take();

            var expected = Enum.GetName( typeof( TKind ), kind );
            var actual = Enum.GetName( typeof( TKind ), token.Kind );

            throw new SyntaxException( $"Unexpected token {actual}, expecting {expected}", token.Span );
        }

        public SyntaxToken<TKind> TakeFirstOf( params TKind[] kinds )
        {
            Ensure.That( kinds, nameof( kinds ) ).IsNotNull();

            var token = this.Peek();
            if( this.MatchAny( kinds ) )
                return this.Take();

            var actual = Enum.GetName( typeof( TKind ), token.Kind );
            var expected = kinds.Select( x => Enum.GetName( typeof( TKind ), x ) ).Join( ", " );

            throw new SyntaxException( $"Unexpected token {actual}, expecting one of: {expected}", token.Span );
        }

        public bool MatchOne( TKind kind )
            => this.Peek().Kind.Equals( kind );

        public bool MatchAny( params TKind[] kinds )
            => kinds.Length == 0 || kinds.Any( this.MatchOne );

        public bool MatchAll( params TKind[] kinds )
            => kinds.Length == 0 || kinds.All( this.MatchOne );
    }
}
