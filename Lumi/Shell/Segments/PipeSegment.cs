using System.Collections.Generic;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    internal sealed class PipeSegment : IShellSegment
    {
        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public IShellSegment Right { get; }

        public PipeSegment( IShellSegment parent, IShellSegment left, IShellSegment right )
        {
            this.Parent = parent;
            this.Left = left;
            this.Right = right;
        }

        public override string ToString()
            => $"{this.Left} | {this.Right}";

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var left = this.Left.Execute( capture: true );
            return left.ExitCode != 0 ? left : this.Right.Execute( left.StandardOutput, true );
        }
    }
}
