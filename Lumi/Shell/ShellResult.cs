using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lumi.Shell
{
    internal ref struct ShellResult
    {
        public int ExitCode { get; }
        public IReadOnlyList<string> StandardOutput { get; }
        public IReadOnlyList<string> StandardError { get; }

        public ShellResult( int exitCode, IReadOnlyList<string> standardOutput, IReadOnlyList<string> standardError )
        {
            this.ExitCode = exitCode;
            this.StandardOutput = standardOutput ?? new string[0];
            this.StandardError = standardError ?? new string[0];
        }

        public static ShellResult Ok( string message )
            => new ShellResult( 0, new[] { message }, null );

        public static ShellResult Ok( string[] output = null )
            => new ShellResult( 0, output, null );

        public static ShellResult Error( int code, string message )
            => new ShellResult( code, null, new[] { message } );

        public static ShellResult Error( int code, string[] error = null )
            => new ShellResult( code, null, error );

        public static ShellResult FromProcess( ProcessStartInfo info )
            => ShellResult.FromProcess( Process.Start( info ) );

        public static ShellResult FromProcess( Process proc )
        {
            proc.WaitForExit();

            return new ShellResult(
                proc.ExitCode,
                proc.StartInfo.RedirectStandardOutput ? ReadAllLines( proc.StandardOutput ) : null,
                proc.StartInfo.RedirectStandardError ? ReadAllLines( proc.StandardError ) : null
            );

            IReadOnlyList<string> ReadAllLines( StreamReader reader )
            {
                string line;
                var lines = new List<string>();

                using( reader )
                {
                    while( ( line = reader.ReadLine() ) != null )
                        lines.Add( line );
                }

                return lines;
            }
        }
    }
}
