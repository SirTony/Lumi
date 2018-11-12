using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class SequenceSegment : IShellSegment
    {
        /// <summary>
        ///     If <see langword="true" />, the sequence will stop execution on a non-zero exit code.
        /// </summary>
        [JsonProperty]
        public bool Safe { get; }

        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public IShellSegment Right { get; }

        public SequenceSegment( IShellSegment left, IShellSegment right, bool safe )
        {
            Ensure.That( left, nameof( left ) ).IsNotNull();
            Ensure.That( right, nameof( right ) ).IsNotNull();

            this.Safe = safe;
            this.Left = left;
            this.Right = right;
        }

        public override string ToString()
            => $"{this.Left}{( this.Safe ? " & " : "; " )}{this.Right}";

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Sequence;

        //public ShellResult Execute( object input = null, bool captureOutput = false ) => throw new NotImplementedException();

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var left = this.Left.Execute( config );
            if( this.Safe && !left ) return left;

            return this.Right.Execute( config );
        }

        public T Accept<T>( ISegmentVisitor<T> visitor ) => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor ) => visitor.Visit( this );
    }
}
