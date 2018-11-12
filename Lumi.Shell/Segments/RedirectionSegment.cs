using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    public enum RedirectionMode
    {
        /// <summary>
        ///     Reads all available lines from the device and writes them to a process' STDIN.
        /// </summary>
        StandardInput,

        /// <summary>
        ///     Writes a process' STDOUT to the specified device.
        /// </summary>
        StandardOutput,

        /// <summary>
        ///     Writes a process' STDERR to the specified device.
        /// </summary>
        StandardError,

        /// <summary>
        ///     Writes both a process' STDOUT and STDERR to the specified device.
        /// </summary>
        StandardOutputAndError
    }

    [JsonObject( MemberSerialization.OptIn )]
    public sealed class RedirectionSegment : IShellSegment
    {
        // fucking DOS legacy shit
        public static readonly IReadOnlyList<string> ReservedDeviceNames;

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public RedirectionMode Mode { get; }

        [JsonProperty]
        public IShellSegment Left { get; }

        [JsonProperty]
        public IShellSegment Device { get; }

        static RedirectionSegment()
        {
            /*
             * Reserved device names
             * =====================
             *
             * CON, PRN, AUX, NUL,
             * COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9,
             * LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9
            */

            var devices = new[] { "CON", "PRN", "AUX", "NUL" };
            var com = Enumerable.Range( 1, 9 ).Select( x => $"COM{x}" );
            var lpt = Enumerable.Range( 1, 9 ).Select( x => $"LPT{x}" );

            RedirectionSegment.ReservedDeviceNames = devices.Concat( com ).Concat( lpt ).ToArray();
        }

        public RedirectionSegment( IShellSegment segment, IShellSegment device, RedirectionMode mode )
        {
            Ensure.That( segment, nameof( segment ) ).IsNotNull();
            Ensure.That( device, nameof( device ) ).IsNotNull();

            this.Left = segment;
            this.Device = device;
            this.Mode = mode;
        }

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        private static extern IntPtr CreateFile(
            [MarshalAs( UnmanagedType.LPTStr )] string filename,
            [MarshalAs( UnmanagedType.U4 )] FileAccess access,
            [MarshalAs( UnmanagedType.U4 )] FileShare share,
            IntPtr securityAttributes,
            [MarshalAs( UnmanagedType.U4 )] FileMode creationDisposition,
            [MarshalAs( UnmanagedType.U4 )] FileAttributes flagsAndAttributes,
            IntPtr templateFile
        );

        private IReadOnlyList<string> ReadFromDevice( string device )
        {
            using( var stream = this.OpenStream( device, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            using( var reader = new StreamReader( stream, Encoding.UTF8, true ) )
            {
                string line;
                var lines = new List<string>();

                while( ( line = reader.ReadLine() ) != null )
                    lines.Add( line );

                return lines;
            }
        }

        private void WriteToDevice( object value, string device )
        {
            var redirectStdOut = this.Mode == RedirectionMode.StandardOutput
                              || this.Mode == RedirectionMode.StandardOutputAndError;

            var redirectStdErr = this.Mode == RedirectionMode.StandardError
                              || this.Mode == RedirectionMode.StandardOutputAndError;

            using( var stream = this.OpenStream( device, FileMode.Create, FileAccess.Write, FileShare.None ) )
            using( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
            {
                switch( value )
                {
                    case StandardStreams std when std.StandardOutput != null && redirectStdOut:
                        std.StandardOutput.ForEach( writer.WriteLine );
                        break;

                    case StandardStreams std when std.StandardError != null && redirectStdErr:
                        std.StandardError.ForEach( writer.WriteLine );
                        break;

                    case IEnumerable<string> lines:
                        lines.ForEach( writer.WriteLine );
                        break;

                    default:
                        writer.WriteLine( value?.ToString() );
                        break;
                }

                writer.Flush();
            }
        }

        private FileStream OpenStream( string device, FileMode mode, FileAccess access, FileShare share )
        {
            var isSpecialDevice = RedirectionSegment.ReservedDeviceNames.Any(
                x => device.IndexOf( x, StringComparison.OrdinalIgnoreCase )
                  >= 0
            );

            if( isSpecialDevice && device.IndexOf( "NUL", StringComparison.OrdinalIgnoreCase ) < 0 )
            {
                throw new InvalidOperationException(
                    $"Opening special device '{device}' is not supported, only NUL is allowed"
                );
            }

            if( !isSpecialDevice )
                return File.Open( device, mode, access, share );

            var ptr = RedirectionSegment.CreateFile(
                device,
                access,
                share,
                IntPtr.Zero,
                mode,
                FileAttributes.Normal,
                IntPtr.Zero
            );
            if( ptr == IntPtr.Zero )
                throw new FileNotFoundException( $"Unable to open device '{device}'" );

            var safeHandle = new SafeFileHandle( ptr, true );
            return new FileStream( safeHandle, access );
        }

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Redirection;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var device = this.Device.Execute( config, null, true );
            if( !( device.Value is string dev ) )
                throw new InvalidOperationException( "Redirection device must be a string" );

            switch( this.Mode )
            {
                case RedirectionMode.StandardOutput:
                case RedirectionMode.StandardError:
                case RedirectionMode.StandardOutputAndError:
                    var left = this.Left.Execute( config, input, true );
                    if( !left ) return left;

                    this.WriteToDevice( left.Value, dev );
                    return ShellResult.Ok();

                case RedirectionMode.StandardInput:
                    var lines = this.ReadFromDevice( dev );
                    return this.Left.Execute( config, lines );

                default:
                    throw new NotImplementedException();
            }
        }

        public T Accept<T>( ISegmentVisitor<T> visitor ) => throw new NotImplementedException();

        public void Accept( ISegmentVisitor visitor ) { throw new NotImplementedException(); }
    }
}
