using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lumi.Shell.Visitors;

namespace Lumi.Shell.Segments
{
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

        public RedirectionMode Mode { get; }
        public IShellSegment Left { get; }
        public string Redirection { get; }

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

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            capture = this.Mode == RedirectionMode.StdOut
                   || this.Mode == RedirectionMode.StdErr
                   || this.Mode == RedirectionMode.StdOutAndErr;

            inputs = this.Mode == RedirectionMode.StdIn
                         ? new List<string>( File.ReadAllLines( this.Redirection, Encoding.UTF8 ) )
                         : null;

            var result = this.Left.Execute( inputs, capture );

            if( !capture )
                return result;

            using( var stream = File.Open( this.Redirection, FileMode.Create, FileAccess.Write, FileShare.None ) )
            using( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
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
            }

            return result;
        }
    }
}
