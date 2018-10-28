using Lumi.Shell.Visitors;
using System.Collections.Generic;

namespace Lumi.Shell.Segments
{
    internal sealed class SequenceSegment : IShellSegment
    {
        /// <summary>
        /// If <see langword="true" />, stop segment execution on a non-zero exit code.
        /// </summary>
        public bool Safe { get; }
        public IShellSegment Left { get; }
        public IShellSegment Right { get; }
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public SequenceSegment( IShellSegment parent, IShellSegment left, IShellSegment right, bool safe = true )
        {
            this.Parent = parent;
            this.Safe = safe;
            this.Left = left;
            this.Right = right;
        }

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var result = this.Left.Execute();
            if( this.Safe && result.ExitCode != 0 )
                return result;

            return this.Right.Execute();
        }

        public override string ToString()
            => $"{this.Left.ToString()}{( this.Safe ? " & " : "; " )}{this.Right.ToString()}";
    }
}
