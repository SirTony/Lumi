using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class CommandSegment : IShellSegment
    {
        [JsonProperty]
        public IShellSegment Command { get; }

        [JsonProperty]
        public IReadOnlyList<IShellSegment> Arguments { get; }

        public CommandSegment( IShellSegment command, IReadOnlyList<IShellSegment> args = null )
        {
            Ensure.That( command, nameof( command ) ).IsNotNull();

            this.Command = command;
            this.Arguments = args;
        }

        public override string ToString()
            => this.Arguments is null
                   ? $"{this.Command}"
                   : $"{this.Command} {this.Arguments.Select( x => x.ToString() ).Join( " " )}";

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Command;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var commandSegment = this.Command.Execute( config, captureOutput: true );
            if( commandSegment.ExitCode != 0 || !( commandSegment.Value is string commandName ) )
                throw new InvalidOperationException( "Command name must evaluate to a string" );

            var hasBuiltInCommand = Commands.TryGetCommandByName( commandName, out var command )
                                 && !config.DisableAllCommands
                                 && !config.DisabledCommands.Contains( commandName );

            if( hasBuiltInCommand )
                return command.Execute( input );

            var args = new List<string>();
            if( this.Arguments != null )
            {
                foreach( var item in this.Arguments )
                {
                    var result = item.Execute( config, captureOutput: true );
                    switch( result.Value )
                    {
                        case Exception e when !result:

                            // ReSharper disable once UnthrowableException
                            throw e;

                        case object _ when !result:
                            return result;

                        case StandardStreams std when std.StandardOutput != null:
                            args.AddRange( std.StandardOutput );
                            break;

                        case IEnumerable<string> lines:
                            args.AddRange( lines );
                            break;

                        default:
                            args.Add( result.Value?.ToString() ?? String.Empty );
                            break;
                    }
                }
            }

            var inputLines = GetInputLines();
            var startInfo = new ProcessStartInfo
            {
                FileName = commandName,
                Arguments = Utility.Escape( args ).Join( " " ),
                UseShellExecute = false,
                RedirectStandardInput = inputLines.Count > 0,
                RedirectStandardError = captureOutput,
                RedirectStandardOutput = captureOutput
            };

            try
            {
                var proc = Process.Start( startInfo );

                if( startInfo.RedirectStandardInput )
                {
                    inputLines.ForEach( proc.StandardInput.WriteLine );
                    proc.StandardInput.Flush();
                    proc.StandardInput.Close();
                }


                if( !captureOutput )
                {
                    proc.WaitForExit();
                    return new ShellResult( proc.ExitCode, null );
                }

                var output = new List<string>();
                var error = new List<string>();

                proc.OutputDataReceived += Appender( output );
                proc.ErrorDataReceived += Appender( error );

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();

                output.RemoveAll( x => x is null );
                error.RemoveAll( x => x is null );

                return error.Count == 0 && output.Count == 1
                           ? new ShellResult( proc.ExitCode, output[0] )
                           : new ShellResult( proc.ExitCode, new StandardStreams( output, error ) );
            }
            catch( Win32Exception ex )
            {
                throw new ProgramNotFoundException( commandName, ex );
            }

            DataReceivedEventHandler Appender( List<string> destination ) => ( s, e ) => destination.Add( e.Data );

            IReadOnlyList<string> GetInputLines()
            {
                switch( input )
                {
                    case StandardStreams std when std.StandardOutput != null:
                        return std.StandardOutput;

                    case IEnumerable<string> lines:
                        return lines.ToArray();

                    default:
                        return input is null ? Array.Empty<string>() : new[] { input.ToString() };
                }
            }
        }

        public T Accept<T>( ISegmentVisitor<T> visitor ) => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor ) => visitor.Visit( this );
    }
}
