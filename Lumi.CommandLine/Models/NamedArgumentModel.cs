using System;

namespace Lumi.CommandLine.Models
{
    internal sealed class NamedArgumentModel : ArgumentModel, IEquatable<NamedArgumentModel>
    {
        public char ShortName { get; }
        public string LongName { get; }

        public bool HasShortName => this.ShortName != default;
        public bool HasLongName => this.LongName != null;

        public NamedArgumentModel( char shortName, string longName )
        {
            this.ShortName = shortName;
            this.LongName = longName;
        }

        public override string ToString()
        {
            if( this.HasShortName && this.HasLongName )
                return $"-{this.ShortName}/--{this.LongName}";

            if( this.HasShortName )
                return $"-{this.ShortName}";

            if( this.HasLongName )
                return $"--{this.LongName}";

            // this shouldn't happen
            // ReSharper disable once ThrowExceptionInUnexpectedLocation
            throw new NotSupportedException();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( this.ShortName.GetHashCode() * 397 )
                     ^ ( this.LongName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode( this.LongName ) : 0 );
            }
        }

        public override bool Equals( object obj )
        {
            if( Object.ReferenceEquals( null, obj ) ) return false;
            if( Object.ReferenceEquals( this, obj ) ) return true;
            return obj is NamedArgumentModel other && this.Equals( other );
        }

        public bool Equals( NamedArgumentModel other )
        {
            if( Object.ReferenceEquals( null, other ) ) return false;
            if( Object.ReferenceEquals( this, other ) ) return true;
            return this.ShortName == other.ShortName
                && String.Equals( this.LongName, other.LongName, StringComparison.OrdinalIgnoreCase );
        }
    }
}
