using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public class PipeSegment : IShellSegment
    {
        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public IShellSegment Right { get; }

        public PipeSegment( IShellSegment left, IShellSegment right )
        {
            Ensure.That( left, nameof( left ) ).IsNotNull();
            Ensure.That( right, nameof( right ) ).IsNotNull();

            this.Left = left;
            this.Right = right;
        }

        public override string ToString() => $"{this.Left} | {this.Right}";

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Pipe;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var left = this.Left.Execute( config, input, true );
            return !left ? left : this.Right.Execute( config, left.Value, captureOutput );
        }

        public T Accept<T>( ISegmentVisitor<T> visitor ) => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor ) => visitor.Visit( this );
    }
}
