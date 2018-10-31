using System.Collections.Generic;
using Lumi.Shell.Visitors;

namespace Lumi.Shell.Segments
{
    internal sealed class InterpolationSegment : IShellSegment
    {
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
