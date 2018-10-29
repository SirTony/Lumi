using CommandLine;
using Lumi.Commands;
using Lumi.Config;
using Lumi.Shell;
using Lumi.Shell.Segments;
using Lumi.Shell.Visitors;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Console = Colorful.Console;

// To silence the catch clauses that use when( !IsDebug )
#pragma warning disable CS8359 // Filter expression is a constant 'false'
#pragma warning disable CS7095 // Filter expression is a constant 'true'

namespace Lumi
{
    internal static class Program
    {
        private const string _defaultTitlte = "Lumi";

#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public static string ExecutablePath { get; }
        public static string SourceDirectory { get; }

        private static readonly VariableSegment _noExecute;
        private static readonly VariableSegment _printTokens;
        private static readonly VariableSegment _printTree;

        static Program()
        {
            var asm = typeof( Program ).Assembly;
            var uri = new Uri( asm.CodeBase );

            ExecutablePath = uri.LocalPath;
            SourceDirectory = Path.GetDirectoryName( uri.LocalPath );

            _noExecute = new VariableSegment( null, "temp", "__NO_EXECUTE" );
            _printTokens = new VariableSegment( null, "temp", "__PRINT_TOKENS" );
            _printTree = new VariableSegment( null, "temp", "__PRINT_TREE" );
        }

        private static void Main( string[] args )
        {
            Console.CancelKeyPress += ( s, e ) => {
                e.Cancel = true;
            };

            // These need to be set ahead of time because they're temporary
            // so they only live as long as the process, and if we try
            // reading the value before it's been set the variable
            // doesn't actually exist and we'll get an exception.
            _noExecute.SetValue( "false" );
            _printTokens.SetValue( "false" );
            _printTree.SetValue( "false" );

            _noExecute.WatchForChange( delegate ( object sender, VariableValueChangedEventArgs e ) {
                if( !( sender is VariableSegment variable ) || variable.Scope != VariableSegment.Scopes.Temporary || !variable.Is<bool>() )
                    return;

                ConsoleEx.WriteWarning(
                    "Disabling command execution will prevent usage of the set command " +
                    "preventing execution from being re-enabled FOR THIS PROCESS ONLY." +
                    "Other Lumi shells will be unaffected."
                );

                Console.WriteLine();
                e.Revert = Prompt.YesNo( "Would you like to revert?", true );
            } );

            VariableSegment.WatchForChange( "ColorScheme.Background", delegate ( object sender, VariableValueChangedEventArgs e ) {
                if( !( sender is VariableSegment variable ) || variable.Scope != VariableSegment.Scopes.Configuration || !variable.Is<Color>() )
                    return;

                ConsoleEx.WriteWarning(
                    "Changing the console window's background color will cause all text to be cleared."
                );

                Console.WriteLine();
                e.Revert = Prompt.YesNo( "Would you like to revert?", true );

                if( !e.Revert )
                {
                    ConfigManager.Instance.ColorScheme.Apply();
                    Console.Clear();
                }
            } );

            VariableSegment.WatchForChange( "ColorScheme.Foreground", delegate ( object sender, VariableValueChangedEventArgs e ) {
                if( !( sender is VariableSegment variable ) || variable.Scope != VariableSegment.Scopes.Configuration || !variable.Is<Color>() || e.Revert )
                    return;

                ConfigManager.Instance.ColorScheme.Apply();
            } );

            Parser.Default
                  .ParseArguments<CommandLineArguments>( args )
                  .WithParsed( Program.Run );
        }

        private static void Run( CommandLineArguments args )
        {
            if( !String.IsNullOrWhiteSpace( args.EvaluateCommand ) )
            {
                ExecuteCommand( args.EvaluateCommand, args );
                return;
            }

            Console.Title = _defaultTitlte;

            ConfigManager.Instance.ColorScheme.Apply();
            Console.Clear();

            while( true )
            {
                WritePrompt();
                ExecuteCommand( Console.ReadLine(), args );
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

                if( args.PrintTokens || _printTokens.As<bool>() )
                    lexer.Tokenize().ForEach( x => Console.WriteLine( x.ToString( true ) ) );

                if( args.PrintTree || _printTree.As<bool>() )
                {
                    var printer = new DebugPrintVisitor( Console.Out );
                    segment.Accept( printer );
                }

                if( args.NoExecute || _noExecute.As<bool>() )
                    return;

                // The built-in "set" command has to be processed up-front because it depends on
                // the second argument being a VariableSegment which has to remain as-is
                // because we use the syntax 'set $cfg:Foo "some value"'
                // and if we execute the segment the variable will be expanded to it's actual value
                // which we don't want
                var result = segment is CommandSegment cmd && cmd.Command.Equals( "set", StringComparison.OrdinalIgnoreCase )
                    ? BuiltInCommands.SetVariable( cmd.Arguments )
                    : segment.Execute();

                Environment.ExitCode = result.ExitCode;

                result.StandardOutput?.ForEach( Console.Out.WriteLine );
                result.StandardError?.ForEach( Console.Error.WriteLine );
            }
            catch( ShellSyntaxException ex ) when( !IsDebug )
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
                    section = $"{section.Substring( 0, Console.BufferWidth - TrailingEllipsis.Length )}{TrailingEllipsis}";
                }

                var len = trim ? PaddingSize + PrefixEllipsis.Length : ex.Start;
                var whitespace = new string( ' ', len );
                var line = new string( '─', len );

                Console.WriteLine( section );
                Console.WriteLine( $"{whitespace}^", ConfigManager.Instance.ColorScheme.ErrorColor );
                Console.WriteLine( $"{line}┘", ConfigManager.Instance.ColorScheme.ErrorColor );
            }
            catch( ProgramNotFoundException ex ) when( !IsDebug )
            {
                ConsoleEx.WriteError( $"'{ex.ProgramName}' is not a known command or executable file" );
            }
            catch( Exception ex ) when( !IsDebug )
            {
                Console.WriteLine( ex );
            }
            finally
            {
                if( String.IsNullOrWhiteSpace( args.EvaluateCommand ) )
                    Console.Title = _defaultTitlte;
            }
        }

        private static void WritePrompt()
        {
            var scheme = ConfigManager.Instance.ColorScheme;

            Console.Write( "$ " );
            Console.Write( Environment.UserName, scheme.PromptUserNameColor );
            Console.Write( "@" );
            Console.Write( GetCurrentDirectory(), scheme.PromptDirectoryColor );
            Console.Write( "> " );
        }

        public static string GetCurrentDirectory()
        {
            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ).ToLowerInvariant();
            var current = Directory.GetCurrentDirectory();

            return current.ToLowerInvariant().StartsWith( home ) && ConfigManager.Instance.UseTilde
                 ? $"~{current.Substring( home.Length )}"
                 : current;
        }
    }
}
