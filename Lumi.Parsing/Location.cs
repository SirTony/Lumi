using System;
using Newtonsoft.Json;

namespace Lumi.Parsing
{
    [JsonObject( MemberSerialization.OptIn )]
    public readonly struct Location : IEquatable<Location>
    {
        [JsonProperty]
        public int Index { get; }

        [JsonProperty]
        public int Line { get; }

        [JsonProperty]
        public int Column { get; }

        public Location( int index, int line, int column )
        {
            this.Index = index;
            this.Line = line;
            this.Column = column;
        }

        public bool Equals( Location other )
            => this.Index == other.Index
            && this.Line == other.Line
            && this.Column == other.Column;

        public override bool Equals( object obj )
            => !Object.ReferenceEquals( null, obj ) && obj is Location other && this.Equals( other );

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Index;
                hashCode = ( hashCode * 397 ) ^ this.Line;
                hashCode = ( hashCode * 397 ) ^ this.Column;
                return hashCode;
            }
        }

        public static explicit operator TextSpan( Location self )
            => new TextSpan( self, self );
    }
}
