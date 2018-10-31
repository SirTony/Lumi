using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lumi.Commands;
using Lumi.Shell.Visitors;

namespace Lumi.Shell.Segments
{
    internal sealed class CommandSegment : IShellSegment
    {
        public string Command { get; }
        public IReadOnlyList<IShellSegment> Arguments { get; }

        public CommandSegment( IShellSegment parent, string command, params IShellSegment[] args )
        {
            this.Parent = parent;
            this.Command = command;
            this.Arguments = args ?? new IShellSegment[0];
        }

        private void Proc_OutputDataReceived( object sender, DataReceivedEventArgs e )
            => throw new NotImplementedException();

        public override string ToString()
            => this.Arguments.Count == 0
                   ? ShellUtil.Escape( this.Command )
                   : $"{ShellUtil.Escape( this.Command )} {this.Arguments.Select( x => x.ToString() ).Join( " " )}";

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            if( BuiltInCommands.TryExecute( this.Command, this.Arguments, out var result ) )
                return result;

            var args = new List<string>();
            foreach( var segment in this.Arguments )
            {
                switch( segment )
                {
                    case CommandSegment literal:

                        // Arguments should never contain a CommandSegment
                        // but if it does just ignore the sub-arguments (if there are any)
                        args.Add( literal.Command );
                        break;

                    case TextSegment text:
                        args.Add( text.Text );
                        break;

                    default:
                        result = segment.Execute( capture: true );
                        if( result.ExitCode != 0 )
                            return result;

                        if( result.StandardOutput != null )
                            args.AddRange( result.StandardOutput );

                        break;
                }
            }

            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = ShellUtil.Escape( this.Command ),
                    Arguments = ShellUtil.Escape( args ).Join( " " ),
                    UseShellExecute = false,
                    RedirectStandardInput = inputs?.Count > 0,
                    RedirectStandardOutput = capture,
                    RedirectStandardError = capture
                };

                var proc = Process.Start( info );

                if( inputs?.Count > 0 )
                {
                    inputs.ForEach( proc.StandardInput.WriteLine );
                    proc.StandardInput.Flush();
                    proc.StandardInput.Close();
                }

                if( !capture )
                    return ShellResult.FromProcess( proc );

                var output = new List<string>();
                var error = new List<string>();

                proc.OutputDataReceived += Appender( output );
                proc.ErrorDataReceived += Appender( error );

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                return new ShellResult( proc.ExitCode, output, error );
            }
            catch( Win32Exception ex )
            {
                throw new ProgramNotFoundException( ShellUtil.Escape( this.Command ), ex );
            }

            DataReceivedEventHandler Appender( List<string> target ) { return ( _, e ) => target.Add( e.Data ); }
        }
    }
}
