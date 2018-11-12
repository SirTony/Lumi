using System;
using Newtonsoft.Json;

namespace Lumi.Parsing
{
    [JsonObject( MemberSerialization.OptIn )]
    public readonly struct TextSpan : IEquatable<TextSpan>
    {
        [JsonProperty]
        public Location Start { get; }

        [JsonProperty]
        public Location End { get; }

        public int Length => this.End.Index - this.Start.Index;

        public TextSpan( Location start, Location end )
        {
            this.Start = start;
            this.End = end;
        }

        public bool Equals( TextSpan other )
            => this.Start.Equals( other.Start )
            && this.End.Equals( other.End );

        public override bool Equals( object obj )
            => !Object.ReferenceEquals( null, obj ) && obj is TextSpan other && this.Equals( other );

        public override int GetHashCode()
        {
            unchecked { return ( this.Start.GetHashCode() * 397 ) ^ this.End.GetHashCode(); }
        }
    }
}
