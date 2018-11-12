using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Lumi.Core;
using Lumi.Shell;
using Lumi.Shell.Parsing;
using Args = PowerArgs.Args;
using Console = Colorful.Console;

// To silence the catch clauses that use when( !IsDebug )
#pragma warning disable CS8359 // Filter expression is a constant 'false'
#pragma warning disable CS7095 // Filter expression is a constant 'true'

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

    internal static class Program
    {
        private const string DefaultTitle = "Lumi";

        public static AppConfig Config { get; private set; }

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
            catch( ProgramNotFoundException ex ) when( !Debugger.IsAttached )
            {
                ConsoleEx.WriteError( $"'{ex.ProgramName}' is not a known command or executable file" );
            }
            catch( Exception ex ) when( !Debugger.IsAttached )
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
            var scheme = Program.Config.ColorScheme;

            Console.Write( "$ " );
            Console.Write( Environment.UserName, scheme.PromptUserNameColor );
            Console.Write( "@" );
            Console.Write( Program.GetCurrentDirectory(), scheme.PromptDirectoryColor );
            Console.Write( "> " );
        }

        public static void ReloadConfig()
        {
            Program.Config = AppConfig.Load();
            Program.Config.ColorScheme.Apply();
            Console.Clear();
        }

        public static string GetCurrentDirectory()
        {
            var home = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ).ToLowerInvariant();
            var current = ShellUtil.GetProperDirectoryCapitalization( Directory.GetCurrentDirectory() );

            return current.ToLowerInvariant().StartsWith( home ) && Program.Config.UseTilde
                       ? $"~{current.Substring( home.Length )}"
                       : current;
        }
    }
}
