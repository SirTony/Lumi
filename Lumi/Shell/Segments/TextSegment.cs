using Lumi.Shell.Visitors;
using System.Collections.Generic;

namespace Lumi.Shell.Segments
{
    internal sealed class TextSegment : IShellSegment
    {
        public string Text { get; }
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public TextSegment( IShellSegment parent, string text )
        {
            this.Parent = parent;
            this.Text = text;
        }

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
            => new ShellResult( 0, new[] { this.Text }, null );

        public override string ToString() => ShellUtil.Escape( this.Text );
    }
}
