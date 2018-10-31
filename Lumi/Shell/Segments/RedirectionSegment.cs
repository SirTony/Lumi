using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    internal sealed class RedirectionSegment : IShellSegment
    {
        public enum RedirectionMode
        {
            /// <summary>
            ///     Read all lines from a file and pass it to the process over standard input.
            /// </summary>
            StdIn,

            /// <summary>
            ///     Write standard output from a process to a file (or null device).
            /// </summary>
            StdOut,

            /// <summary>
            ///     Write standard error from a process to a file (or null device).
            /// </summary>
            StdErr,

            /// <summary>
            ///     Write both standard output and standard error from a process to a file (or null device).
            /// </summary>
            StdOutAndErr
        }

        private static readonly IDictionary<string, Stream> NamedStreams;

        [JsonProperty]
        public RedirectionMode Mode { get; }

        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public string Redirection { get; }

        static RedirectionSegment()
            => RedirectionSegment.NamedStreams = new Dictionary<string, Stream>( StringComparer.OrdinalIgnoreCase );

        public RedirectionSegment( IShellSegment parent, IShellSegment left, string redirection, RedirectionMode mode )
        {
            this.Parent = parent;
            this.Mode = mode;
            this.Left = left;
            this.Redirection = redirection;
        }

        public override string ToString()
        {
            string symbol;

            switch( this.Mode )
            {
                case RedirectionMode.StdOut:
                    symbol = ">";
                    break;

                case RedirectionMode.StdErr:
                    symbol = ">>";
                    break;

                case RedirectionMode.StdOutAndErr:
                    symbol = ">>>";
                    break;

                case RedirectionMode.StdIn:
                    symbol = "<";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return $"{this.Left} {symbol} {this.Redirection}";
        }

        private ( Stream, bool ) OpenRedirectionStream()
        {
            if( this.Redirection[0] != ':' )
            {
                return this.Mode == RedirectionMode.StdIn && !File.Exists( this.Redirection )
                           ? ( null, false )
                           : (
                                 File.Open( this.Redirection, FileMode.Create, FileAccess.Write, FileShare.None ),
                                 false
                             );
            }

            var rest = this.Redirection.Substring( 1 );
            if( rest.Equals( "null", StringComparison.OrdinalIgnoreCase ) )
                return ( Stream.Null, true );

            if( RedirectionSegment.NamedStreams.TryGetValue( rest, out var stream ) )
                return ( stream, true );

            return ( RedirectionSegment.NamedStreams[rest] = new MemoryStream(), true );
        }

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            const int BufferSize = 1024 * 4; // 4k

            capture = this.Mode == RedirectionMode.StdOut
                   || this.Mode == RedirectionMode.StdErr
                   || this.Mode == RedirectionMode.StdOutAndErr;

            Stream stream;
            bool leaveOpen;

            if( this.Mode == RedirectionMode.StdIn )
            {
                ( stream, leaveOpen ) = this.OpenRedirectionStream();
                if( stream is null )
                    return ShellResult.Error( -1, $"standard input redirection cannot read from '{this.Redirection}'" );

                var lines = new List<string>();
                using( var reader = new StreamReader( stream, Encoding.UTF8, true, BufferSize, leaveOpen ) )
                {
                    string line;
                    while( ( line = reader.ReadLine() ) != null )
                        lines.Add( line );
                }

                inputs = lines;

                // clear the stream after we read from it to prevent pollution and memory leaks
                if( leaveOpen && stream is MemoryStream memory )
                    memory.SetLength( 0 );
            }

            var result = this.Left.Execute( inputs, capture );

            if( !capture )
                return result;

            ( stream, leaveOpen ) = this.OpenRedirectionStream();
            using( var writer = new StreamWriter( stream, Encoding.UTF8, BufferSize, leaveOpen ) )
            {
                switch( this.Mode )
                {
                    case RedirectionMode.StdOut:
                        result.StandardOutput?.ForEach( writer.WriteLine );
                        break;

                    case RedirectionMode.StdErr:
                        result.StandardError?.ForEach( writer.WriteLine );
                        break;

                    case RedirectionMode.StdOutAndErr:
                        result.StandardOutput?.ForEach( writer.WriteLine );
                        result.StandardError?.ForEach( writer.WriteLine );
                        break;
                }

                writer.Flush();

                // seek back to the beginning of the stream after we write to allow reading
                if( leaveOpen && stream is MemoryStream )
                    stream.Position = 0;
            }

            return new ShellResult( result.ExitCode, null, result.StandardError );
        }
    }
}
