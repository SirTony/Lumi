using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumi.CommandLine;
using Lumi.Core;
using Lumi.Parsing;
using Lumi.Shell;
using Lumi.Shell.Parsing;
using Lumi.Shell.Segments;
using Newtonsoft.Json;
using Console = Colorful.Console;

namespace Lumi
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    internal sealed class Program
    {
        private const string DefaultTitle = "Lumi";

        public static AppConfig Config { get; }

        static Program()
            => Program.Config = AppConfig.Load();

        private static int Main( string[] args )
        {
            Console.CancelKeyPress += ( s, e ) => {
                e.Cancel = true;
            };

            Console.Title = Program.DefaultTitle;
            Program.Config.ColorScheme.Apply();
            Console.Clear();

            try
            {
                Shell.Commands.LoadCommandsFrom( typeof( Program ).Assembly );
                Shell.Commands.LoadCommands();
            }
            catch( Exception ex ) when( !Debugger.IsAttached )
            {
                ConsoleEx.WriteError( "Unable to load commands" );
                ConsoleEx.WriteError( ex.Message );
            }

            try
            {
                var ( code, _ ) = CommandLineParser.ParseArguments<Program>( args );
                Environment.ExitCode = code;
            }
            catch( HelpRequestedException ) { }
            catch( Exception ex ) when( !Debugger.IsAttached )
            {
                ConsoleEx.WriteError( ex.Message );
            }

            return Environment.ExitCode;
        }

        [Description( "Tokenize command line and print the token stream in JSON format" )]
        [Command( "print-tokens" )]
        public void PrintTokens(
            [Required] [Description( "Command line text to tokenize" )] [Positional( 0 )]
            string command
        )
        {
            Program.ParseCommandLine( command, out var tokens );
            if( tokens is null ) return;
            Console.WriteLine( JsonConvert.SerializeObject( tokens, Formatting.Indented ) );
        }

        [Description( "Parse command line and print the parse tree in JSON format" )]
        [Command( "print-tree" )]
        public void PrintTree(
            [Required] [Description( "Command line text to parse" )] [Positional( 0 )]
            string command
        )
        {
            var segment = Program.ParseCommandLine( command, out var _ );
            if( segment is null ) return;
            Console.WriteLine( JsonConvert.SerializeObject( segment, Formatting.Indented ) );
        }

        [Description( "Execute a command then exit" )]
        [Command( "exec" )]
        public void Exec(
            [Required] [Description( "Command to execute" )] [Positional( 0 )]
            string command
        ) => Program.ExecuteCommand( command );

        private void Main()
        {
            while( true )
            {
                Program.WritePrompt();
                Program.ExecuteCommand( Console.ReadLine() );
                Console.WriteLine();
            }
        }

        private static void ExecuteCommand( string input )
        {
            if( String.IsNullOrWhiteSpace( input ) )
                return;

            try
            {
                Console.Title = $"Lumi - [{input}]";
                var segment = Program.ParseCommandLine( input, out var _ );

                if( segment is null ) return;

                var result = segment.Execute( Program.Config, captureOutput: true );
                Environment.ExitCode = result.ExitCode;

                switch( result.Value )
                {
                    case StandardStreams std:
                        std.StandardOutput?.Reject( x => x is null )?.ForEach( Console.Out.WriteLine );
                        std.StandardError?.Reject( x => x is null )?.ForEach( Console.Error.WriteLine );
                        break;

                    case IEnumerable<string> lines:
                        lines.Reject( x => x is null ).ForEach( Console.Out.WriteLine );
                        break;

                    default:
                        if( result.Value is null ) break;
                        Console.WriteLine( result.Value.ToString() );
                        break;
                }
            }
            catch( ProgramNotFoundException ex ) when( !Debugger.IsAttached )
            {
                ConsoleEx.WriteError( $"'{ex.ProgramName}' is not a known command or executable file" );
            }
            catch( Exception ex ) when( !Debugger.IsAttached )
            {
                Console.WriteLine( ex );
            }
            finally { Console.Title = Program.DefaultTitle; }
        }

        private static void WritePrompt()
        {
            var scheme = Program.Config.ColorScheme;

            switch( Program.Config.PromptStyle )
            {
                case PromptStyle.Default:
                    Console.Write( "$ " );
                    Console.Write( Environment.UserName, scheme.PromptUserNameColor );
                    Console.Write( "@" );
                    Console.Write( ShellUtility.GetCurrentDirectory(), scheme.PromptDirectoryColor );
                    Console.Write( "> " );
                    break;

                case PromptStyle.Unix:
                    Console.Write( Environment.UserName, scheme.PromptUserNameColor );
                    Console.Write( "@" );
                    Console.WriteLine( Environment.MachineName, scheme.PromptUserNameColor );
                    Console.Write( ":" );
                    Console.Write( ShellUtility.GetCurrentDirectory(), scheme.PromptDirectoryColor );
                    Console.Write( "$ " );
                    break;

                case PromptStyle.Windows:
                    Console.Write( ShellUtility.GetCurrentDirectory(), scheme.PromptDirectoryColor );
                    Console.Write( "> " );
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static IShellSegment ParseCommandLine(
            string input, out IEnumerable<SyntaxToken<ShellTokenKind>> tokens
        )
        {
            try
            {
                var lexer = new ShellLexer( input );
                var parser = new ShellParser( tokens = lexer.Tokenize() );
                return parser.ParseAll();
            }
            catch( ShellSyntaxException ex ) when( !Debugger.IsAttached )
            {
                Console.WriteLine();
                ConsoleEx.WriteError( $"{ex.Message} at position {ex.Span.Start.Index}" );
                Console.WriteLine();

                const int PaddingSize = 10;
                const string PrefixEllipsis = "... ";

                var trim = ex.Span.Start.Index > PaddingSize && input.Length > Console.BufferWidth;
                var section = trim ? $"{PrefixEllipsis}{input.Substring( ex.Span.Start.Index - PaddingSize )}" : input;

                if( section.Length > Console.BufferWidth )
                {
                    const string TrailingEllipsis = " ...";
                    section =
                        $"{section.Substring( 0, Console.BufferWidth - TrailingEllipsis.Length )}{TrailingEllipsis}";
                }

                var len = trim ? PaddingSize + PrefixEllipsis.Length : ex.Span.Start.Index;
                var whitespace = new string( ' ', len );
                var line = new string( '─', len );

                Console.WriteLine( section );
                Console.WriteLine( $"{whitespace}^", Program.Config.ColorScheme.ErrorColor );
                Console.WriteLine( $"{line}┘", Program.Config.ColorScheme.ErrorColor );
            }

            tokens = null;
            return null;
        }
    }
}
