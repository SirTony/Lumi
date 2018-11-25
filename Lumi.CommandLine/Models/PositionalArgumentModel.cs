using System;

namespace Lumi.CommandLine.Models
{
    internal sealed class PositionalArgumentModel : ArgumentModel, IEquatable<PositionalArgumentModel>
    {
        private readonly int _adjust;
        public int Position { get; }

        public int AdjustedPosition => this.Position + this._adjust;

        public PositionalArgumentModel( int position, int adjust )
        {
            this.Position = position;
            this._adjust = adjust;
        }

        public override bool Equals( object obj )
        {
            if( Object.ReferenceEquals( null, obj ) ) return false;
            if( Object.ReferenceEquals( this, obj ) ) return true;
            return obj is PositionalArgumentModel other && this.Equals( other );
        }

        public override int GetHashCode()
        {
            unchecked { return ( this._adjust * 397 ) ^ this.Position; }
        }

        // a cheat for exception messages because im lazy
        public override string ToString() => $"at position {this.Position}";

        public bool Equals( PositionalArgumentModel other )
        {
            if( Object.ReferenceEquals( null, other ) ) return false;
            if( Object.ReferenceEquals( this, other ) ) return true;
            return this._adjust == other._adjust
                && this.Position == other.Position;
        }
    }
}
