using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Lumi.Commands;
using Lumi.Config;
using Lumi.Shell;
using Lumi.Shell.Segments;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;
using Args = PowerArgs.Args;
using Console = Colorful.Console;

// To silence the catch clauses that use when( !IsDebug )
#pragma warning disable CS8359 // Filter expression is a constant 'false'
#pragma warning disable CS7095 // Filter expression is a constant 'true'

namespace Lumi
{
    internal static class Program
    {
        private const string DefaultTitle = "Lumi";

#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public static Assembly Assembly { get; }
        public static string ExecutablePath { get; }
        public static string SourceDirectory { get; }

        static Program()
        {
            Program.Assembly = typeof( Program ).Assembly;
            var uri = new Uri( Program.Assembly.CodeBase );

            Program.ExecutablePath = uri.LocalPath;
            Program.SourceDirectory = Path.GetDirectoryName( uri.LocalPath );
        }

        private static void Main( string[] args )
        {
            Console.CancelKeyPress += ( s, e ) => {
                e.Cancel = true;
            };

            Console.Title = Program.DefaultTitle;
            ConfigManager.Instance.ColorScheme.Apply();
            Console.Clear();

            var parsed = Args.Parse<CommandLineArguments>( args );
            if( parsed == null )
                return;

            Program.Run( parsed );
        }

        private static void Run( CommandLineArguments args )
        {
            if( !String.IsNullOrWhiteSpace( args.EvaluateCommand ) )
            {
                Program.ExecuteCommand( args.EvaluateCommand, args );
                return;
            }

            while( true )
            {
                Program.WritePrompt();
                Program.ExecuteCommand( Console.ReadLine(), args );
                Console.WriteLine();
            }
        }

        private static void ExecuteCommand( string input, CommandLineArguments args )
        {
            if( String.IsNullOrWhiteSpace( input ) )
                return;

            try
            {
                Console.Title = $"Lumi - [{input}]";
                var lexer = new ShellLexer( input );
                var parser = new ShellParser( lexer.Tokenize() );
                var segment = parser.ParseAll();

                if( args.PrintTokens )
                    lexer.Tokenize().ForEach( x => Console.WriteLine( x.ToString( true ) ) );

                switch( args.PrintTree )
                {
                    case CommandLineArguments.ParseTreeFormat.None:
                        break;

                    case CommandLineArguments.ParseTreeFormat.Default:
                        var printer = new DebugPrintVisitor( Console.Out );
                        segment.Accept( printer );
                        break;

                    case CommandLineArguments.ParseTreeFormat.Json:
                        var json = JsonConvert.SerializeObject( segment, Formatting.Indented );
                        Console.WriteLine( json );
                        break;

                    default:
                        throw new NotImplementedException();
                }

                if( args.NoExecute )
                    return;

                // The built-in "set" command has to be processed up-front because it depends on
                // the second argument being a VariableSegment which has to remain as-is
                // because we use the syntax 'set $cfg:Foo "some value"'
                // and if we execute the segment the variable will be expanded to it's actual value
                // which we don't want
                var result = segment is CommandSegment cmd
                          && cmd.Command.Equals( "set", StringComparison.OrdinalIgnoreCase )
                                 ? BuiltInCommands.SetVariable( cmd.Arguments )
                                 : segment.Execute();

                Environment.ExitCode = result.ExitCode;

                result.StandardOutput?.ForEach( Console.Out.WriteLine );
                result.StandardError?.ForEach( Console.Error.WriteLine );
            }
            catch( ShellSyntaxException ex ) when( !Program.IsDebug )
            {
                Console.WriteLine();
                ConsoleEx.WriteError( ex.Message );
                Console.WriteLine();

                const int PaddingSize = 10;
                const string PrefixEllipsis = "... ";

                var trim = ex.Start > PaddingSize && input.Length > Console.BufferWidth;
                var section = trim ? $"{PrefixEllipsis}{input.Substring( ex.Start - PaddingSize )}" : input;

                if( section.Length > Console.BufferWidth )
                {
                    const string TrailingEllipsis = " ...";
                    section =
                        $"{section.Substring( 0, Console.BufferWidth - TrailingEllipsis.Length )}{TrailingEllipsis}";
                }

                var len = trim ? PaddingSize + PrefixEllipsis.Length : ex.Start;
                var whitespace = new string( ' ', len );
                var line = new string( '─', len );

                Console.WriteLine( section );
                Console.WriteLine( $"{whitespace}^", ConfigManager.Instance.ColorScheme.ErrorColor );
                Console.WriteLine( $"{line}┘", ConfigManager.Instance.ColorScheme.ErrorColor );
            }
            catch( ProgramNotFoundException ex ) when( !Program.IsDebug )
            {
                ConsoleEx.WriteError( $"'{ex.ProgramName}' is not a known command or executable file" );
            }
            catch( Exception ex ) when( !Program.IsDebug )
            {
                Console.WriteLine( ex );
            }
            finally
            {
                if( String.IsNullOrWhiteSpace( args.EvaluateCommand ) )
                    Console.Title = Program.DefaultTitle;
            }
        }

        private static void WritePrompt()
        {
            var scheme = ConfigManager.Instance.ColorScheme;

            Console.Write( "$ " );
            Console.Write( Environment.UserName, scheme.PromptUserNameColor );
            Console.Write( "@" );
            Console.Write( Program.GetCurrentDirectory(), scheme.PromptDirectoryColor );
            Console.Write( "> " );
        }

        public static string GetCurrentDirectory()
        {
            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ).ToLowerInvariant();
            var current = ShellUtil.GetProperDirectoryCapitalization( Directory.GetCurrentDirectory() );

            return current.ToLowerInvariant().StartsWith( home ) && ConfigManager.Instance.UseTilde
                       ? $"~{current.Substring( home.Length )}"
                       : current;
        }
    }
}
