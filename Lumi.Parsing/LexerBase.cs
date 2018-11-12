using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumi.Parsing
{
    public abstract class LexerBase<T> where T : Enum
    {
        private readonly Stack<Location> _locations;

        protected string Source { get; }
        protected int SourceLength { get; }
        protected int Index { get; set; }
        protected int Line { get; set; }
        protected int Column { get; set; }

        protected bool EndOfInput => this.Index >= this.SourceLength;

        protected Location CurrentLocation => new Location( this.Index, this.Line, this.Column );

        protected LexerBase( string source )
        {
            this._locations = new Stack<Location>();
            this.Source = source;
            this.SourceLength = source.Length;
        }

        public abstract IEnumerable<SyntaxToken<T>> Tokenize();

        protected void MarkLocation()
            => this._locations.Push( this.CurrentLocation );

        protected Location PopLocation()
            => this._locations.Pop();

        protected Location PeekLocation()
            => this._locations.Peek();

        protected TextSpan PopCurrentSpan()
            => new TextSpan( this.PopLocation(), this.CurrentLocation );

        protected SyntaxToken<T> MakeToken( string text, T kind, object data = null )
        {
            if( this._locations.Count == 0 )
                this.MarkLocation();

            return new SyntaxToken<T>( text, kind, this.PopCurrentSpan(), data );
        }

        protected char Peek( int distance = 0 )
        {
            var newIndex = this.Index + distance;
            return newIndex >= this.SourceLength || newIndex < 0 ? default : this.Source[newIndex];
        }

        protected char Take()
        {
            if( this.EndOfInput )
                return default;

            var c = this.Peek();
            ++this.Index;
            return c;
        }

        protected bool IsNext( string search )
            => search.Length == 1 && search[0] == this.Peek()
            || this.Index + search.Length < this.SourceLength
            && this.Source.Substring( this.Index, search.Length ) == search;

        protected bool TakeIfNext( string search )
        {
            if( !this.IsNext( search ) )
                return false;

            Enumerable.Range( 0, search.Length ).ForEach( _ => this.Take() );
            return true;
        }

        protected string TakeWhile( Predicate<char> pred )
        {
            var sb = new StringBuilder();
            while( !this.EndOfInput && pred( this.Peek() ) )
                sb.Append( this.Take() );

            return sb.ToString();
        }

        protected void SkipWhile( Predicate<char> pred )
        {
            while( !this.EndOfInput && pred( this.Peek() ) )
                this.Take();
        }
    }
}
