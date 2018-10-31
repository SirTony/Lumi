using System.Collections.Generic;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    internal sealed class InterpolationSegment : IShellSegment
    {
        [JsonProperty]
        public IShellSegment Segment { get; }

        public InterpolationSegment( IShellSegment parent, IShellSegment segment )
        {
            this.Parent = parent;
            this.Segment = segment;
        }

        public override string ToString() => $"#({this.Segment})";

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
            => this.Segment.Execute( inputs, capture );
    }
}
