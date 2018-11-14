using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Lumi.Core;
using Lumi.Parsing;
using Lumi.Shell;
using Lumi.Shell.Parsing;
using Lumi.Shell.Segments;
using Newtonsoft.Json;
using Args = PowerArgs.Args;
using Console = Colorful.Console;

namespace Lumi
{
    internal static class NativeMethods
    {
        public static Color SystemWindowColor
        {
            get
            {
                var colors = new DWMCOLORIZATIONCOLORS();
                NativeMethods.DwmGetColorizationParameters( ref colors );

                return Color.FromArgb(
                    255,
                    (byte) ( colors.ColorizationColor >> 16 ),
                    (byte) ( colors.ColorizationColor >> 8 ),
                    (byte) colors.ColorizationColor
                );
            }
        }

        [DllImport( "dwmapi.dll", EntryPoint = "#127" )]
        private static extern void DwmGetColorizationParameters( ref DWMCOLORIZATIONCOLORS colors );

        public static double CalculateLuminance( this Color c )
            => Math.Sqrt( Math.Pow( 0.299 * c.R, 2 ) + Math.Pow( 0.587 * c.G, 2 ) + Math.Pow( 0.114 * c.B, 2 ) );

        public static double CalculateContrastRatio( this Color a, Color b )
            => ( a.CalculateLuminance() + 0.05 ) / ( b.CalculateLuminance() + 0.05 );
    }

    public struct DWMCOLORIZATIONCOLORS
    {
        public uint ColorizationColor,
                    ColorizationAfterglow,
                    ColorizationColorBalance,
                    ColorizationAfterglowBalance,
                    ColorizationBlurBalance,
                    ColorizationGlassReflectionIntensity,
                    ColorizationOpaqueBlend;
    }

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

        [CustomHelpHook]
        [PowerArgs.ArgShortcut( "?" )]
        [PowerArgs.ArgShortcut( "h" )]
        [PowerArgs.ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        static Program()
            => Program.Config = AppConfig.Load();

        private static void Main( string[] args )
        {
            Console.CancelKeyPress += ( s, e ) => {
                e.Cancel = true;
            };

            //var wndColor = NativeMethods.SystemWindowColor;
            //var useDark = wndColor.CalculateContrastRatio( Color.White ) >= 0.6;

            //var windowsScheme = new ColorScheme
            //{
            //    Background = wndColor,
            //    Foreground = SystemColors.ActiveCaptionText,
            //    ErrorColor = !useDark ? Color.DarkRed : Color.Firebrick,
            //    NoticeColor = !useDark ? Color.Teal : Color.Cyan,
            //    WarningColor = !useDark ? Color.DarkGoldenrod : Color.Khaki,
            //    PromptDirectoryColor = SystemColors.ActiveCaptionText,
            //    PromptUserNameColor = SystemColors.ActiveCaptionText,
            //};

            //windowsScheme.Apply();
            //Console.Clear();
            //windowsScheme.DisplayTest();

            //return;

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

            Args.InvokeAction<Program>( args );
        }

        [PowerArgs.ArgDescription( "Tokenize command line and print the token stream in JSON format" )]
        [PowerArgs.ArgActionMethod]
        public void PrintTokens(
            [PowerArgs.ArgRequired] [PowerArgs.ArgDescription( "Command line text to tokenize" )]
            string command
        )
        {
            Program.ParseCommandLine( command, out var tokens );
            if( tokens is null ) return;
            Console.WriteLine( JsonConvert.SerializeObject( tokens, Formatting.Indented ) );
        }

        [PowerArgs.ArgDescription( "Parse command line and print the parse tree in JSON format" )]
        [PowerArgs.ArgActionMethod]
        public void PrintTree(
            [PowerArgs.ArgRequired] [PowerArgs.ArgDescription( "Command line text to parse" )]
            string command
        )
        {
            var segment = Program.ParseCommandLine( command, out var _ );
            if( segment is null ) return;
            Console.WriteLine( JsonConvert.SerializeObject( segment, Formatting.Indented ) );
        }

        [PowerArgs.ArgDescription( "Execute a command then exit" )]
        [PowerArgs.ArgActionMethod]
        public void Exec(
            [PowerArgs.ArgDescription( "Command to execute" )]
            string command
        )
        {
            if( String.IsNullOrWhiteSpace( command ) )
                this.Main();
            else
                Program.ExecuteCommand( command );
        }

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

            Console.Write( "$ " );
            Console.Write( Environment.UserName, scheme.PromptUserNameColor );
            Console.Write( "@" );
            Console.Write( ShellUtility.GetCurrentDirectory(), scheme.PromptDirectoryColor );
            Console.Write( "> " );
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
