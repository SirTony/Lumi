using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lumi.Core;
using Lumi.Shell;
using PowerArgs;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    public sealed class Theme : ICommand
    {
        [StructLayout( LayoutKind.Sequential )]
        private struct DWMCOLORIZATIONCOLORS
        {
            public uint ColorizationColor;
            public uint ColorizationAfterglow;
            public uint ColorizationColorBalance;
            public uint ColorizationAfterglowBalance;
            public uint ColorizationBlurBalance;
            public uint ColorizationGlassReflectionIntensity;
            public uint ColorizationOpaqueBlend;
        }

        private static Color SystemWindowColor
        {
            get
            {
                var colors = new DWMCOLORIZATIONCOLORS();
                Theme.DwmGetColorizationParameters( ref colors );

                return Color.FromArgb(
                    255,
                    (byte)( colors.ColorizationColor >> 16 ),
                    (byte)( colors.ColorizationColor >> 8 ),
                    (byte)colors.ColorizationColor
                );
            }
        }

        [DllImport( "dwmapi.dll", EntryPoint = "#127" )]
        private static extern void DwmGetColorizationParameters( ref DWMCOLORIZATIONCOLORS colors );

        private delegate void ApplyThemeDelegate( AppConfig config );
        
        private static readonly IReadOnlyDictionary<string, ApplyThemeDelegate> BuiltInThemes;

        static Theme()
        {
            Theme.BuiltInThemes = new Dictionary<string, ApplyThemeDelegate>( StringComparer.OrdinalIgnoreCase )
            {
                ["default"] = config => {
                    config.PromptStyle = PromptStyle.Default;
                    config.ColorScheme = new ColorScheme();
                    config.ColorScheme.Apply();
                    Console.Clear();
                },

                ["windows"] = Theme.ApplyWindowsTheme
            };
        }

        [CustomHelpHook]
        [ArgShortcut( "?" )]
        [ArgShortcut( "h" )]
        [ArgDescription( "Show this help screen." )]
        public bool Help { get; private set; }

        [ArgPosition( 0 ), ArgRequired, ArgShortcut( "t" )]
        public string ThemeName { get; private set; }

        [ArgIgnore]
        public string Name { get; } = "theme";

        // TODO: need to implement user-defined themes that can be saved and loaded to/from a JSON file
        public ShellResult Execute( AppConfig config, object input )
        {
            if( Theme.BuiltInThemes.TryGetValue( this.ThemeName, out var applicator ) )
            {
                applicator( config );
                config.Save();
                return ShellResult.Ok();
            }

            return ShellResult.Ok();
        }

        // uses the Windows 10 window border colour to make the entire console window one solid colour.
        private static void ApplyWindowsTheme( AppConfig config )
        {
            var version = Environment.OSVersion.Version;
            if( !( version.Major >= 6 && version.Minor >= 2 ) )
            {
                ConsoleEx.WriteError( "Windows theme only available on Windows 10" );
                return;
            }

            config.PromptStyle = PromptStyle.Windows;

            var wndColor = Theme.SystemWindowColor;
            var useLightColours = wndColor.CalculateContrastRatio( Color.WhiteSmoke ) <= 0.6;
            var foreground = useLightColours ? Color.WhiteSmoke : Color.Black;

            config.ColorScheme = new ColorScheme
            {
                Background = wndColor,
                Foreground = foreground,
                NoticeColor = useLightColours ? Color.Aqua : Color.DarkCyan,
                WarningColor = useLightColours ? Color.Gold : Color.DarkGoldenrod,
                ErrorColor = useLightColours ? Color : Color.Firebrick,
                PromptUserNameColor = foreground,
                PromptDirectoryColor = foreground
            };

            config.ColorScheme.Apply();
            Console.Clear();
        }
    }
}
