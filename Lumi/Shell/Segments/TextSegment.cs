using System.Collections.Generic;
using Lumi.Shell.Visitors;

namespace Lumi.Shell.Segments
{
    internal sealed class TextSegment : IShellSegment
    {
        public string Text { get; }

        public TextSegment( IShellSegment parent, string text )
        {
            this.Parent = parent;
            this.Text = text;
        }

        public override string ToString() => ShellUtil.Escape( this.Text );
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
            => new ShellResult( 0, new[] { this.Text }, null );
    }
}
