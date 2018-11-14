using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class TextSegment : IShellSegment
    {
        [JsonProperty]
        public string Value { get; }

        public TextSegment( string value ) => this.Value = value;

        public override string ToString() => ShellUtility.Escape( this.Value );

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Text;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
            => ShellResult.Ok( this.Value );

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );
    }
}
