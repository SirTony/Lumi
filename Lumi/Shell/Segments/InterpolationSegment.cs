using Lumi.Shell.Visitors;
using System.Collections.Generic;

namespace Lumi.Shell.Segments
{
    internal sealed class InterpolationSegment : IShellSegment
    {
        public IShellSegment Segment { get; }
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public InterpolationSegment( IShellSegment parent, IShellSegment segment )
        {
            this.Parent = parent;
            this.Segment = segment;
        }

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
            => this.Segment.Execute( inputs, capture );

        public override string ToString() => $"#({this.Segment})";
    }
}
