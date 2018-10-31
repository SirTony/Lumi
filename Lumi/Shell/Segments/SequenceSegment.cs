using System.Collections.Generic;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    internal sealed class SequenceSegment : IShellSegment
    {
        /// <summary>
        ///     If <see langword="true" />, stop segment execution on a non-zero exit code.
        /// </summary>
        [JsonProperty]
        public bool Safe { get; }

        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public IShellSegment Right { get; }

        public SequenceSegment( IShellSegment parent, IShellSegment left, IShellSegment right, bool safe = true )
        {
            this.Parent = parent;
            this.Safe = safe;
            this.Left = left;
            this.Right = right;
        }

        public override string ToString()
            => $"{this.Left}{( this.Safe ? " & " : "; " )}{this.Right}";

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var result = this.Left.Execute();
            if( this.Safe && result.ExitCode != 0 )
                return result;

            return this.Right.Execute();
        }
    }
}
