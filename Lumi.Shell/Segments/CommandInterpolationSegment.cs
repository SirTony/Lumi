using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class CommandInterpolationSegment : IShellSegment
    {
        [JsonProperty]
        public IShellSegment Segment { get; }

        public CommandInterpolationSegment( IShellSegment segment )
        {
            Ensure.That( segment, nameof( segment ) ).IsNotNull();
            this.Segment = segment;
        }

        public override string ToString() => $"$({this.Segment})";

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.CommandInterpolation;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
            => this.Segment.Execute( config, input, captureOutput );

        public T Accept<T>( ISegmentVisitor<T> visitor ) => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor ) => visitor.Visit( this );
    }
}
