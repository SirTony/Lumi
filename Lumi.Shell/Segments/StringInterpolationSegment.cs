using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class StringInterpolationSegment : IShellSegment
    {
        [JsonProperty]
        public IReadOnlyList<IShellSegment> Segments { get; }

        public StringInterpolationSegment( IReadOnlyList<IShellSegment> segments )
        {
            Ensure.That( segments, nameof( segments ) ).IsNotNull();

            this.Segments = segments;
        }

        public override string ToString() => $"\"{this.Segments.Join( "" )}\"";

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.StringInterpolation;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var lines = new List<string>();
            foreach( var segment in this.Segments )
            {
                var result = segment.Execute( config, captureOutput: true );
                if( !result ) return result;

                lines.AddRange( GetValue( result.Value ) );
            }

            return ShellResult.Ok( lines.Join( "" ) );

            IReadOnlyList<string> GetValue( object value )
            {
                switch( value )
                {
                    case StandardStreams std when std.StandardOutput != null:
                        return std.StandardOutput;

                    case IEnumerable<string> enumerable:
                        return enumerable.ToList();

                    default:
                        return value is null ? Array.Empty<string>() : new[] { value.ToString() };
                }
            }
        }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );
    }
}
