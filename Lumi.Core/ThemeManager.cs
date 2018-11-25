using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace Lumi.Core
{
    public sealed class ThemeManager
    {
        [StructLayout( LayoutKind.Sequential )]
        private struct DWMCOLORIZATIONCOLORS
        {
            public readonly uint ColorizationColor;
            public readonly uint ColorizationAfterglow;
            public readonly uint ColorizationColorBalance;
            public readonly uint ColorizationAfterglowBalance;
            public readonly uint ColorizationBlurBalance;
            public readonly uint ColorizationGlassReflectionIntensity;
            public readonly uint ColorizationOpaqueBlend;
        }

        private const string FileName = "theme.json";

        private readonly Dictionary<string, ColorScheme> _themes;

        public static Color SystemWindowColor
        {
            get
            {
                var colors = new DWMCOLORIZATIONCOLORS();
                ThemeManager.DwmGetColorizationParameters( ref colors );

                return Color.FromArgb(
                    255,
                    (byte) ( colors.ColorizationColor >> 16 ),
                    (byte) ( colors.ColorizationColor >> 8 ),
                    (byte) colors.ColorizationColor
                );
            }
        }

        public IReadOnlyDictionary<string, ColorScheme> Themes => this._themes;

        private ThemeManager( Dictionary<string, ColorScheme> themes )
            => this._themes = themes;

        [DllImport( "dwmapi.dll", EntryPoint = "#127" )]
        private static extern void DwmGetColorizationParameters( ref DWMCOLORIZATIONCOLORS colors );

        public static ThemeManager Load()
        {
            string json;
            var path = Path.Combine( AppConfig.SourceDirectory, ThemeManager.FileName );
            var dict = new Dictionary<string, ColorScheme>( StringComparer.OrdinalIgnoreCase )
            {
                ["solarized-dark"] =
                    new ColorScheme
                    {
                        Background = Color.FromArgb( 0, 43, 54 ),
                        Foreground = Color.FromArgb( 253, 246, 227 ),
                        ErrorColor = Color.FromArgb( 220, 50, 47 ),
                        WarningColor = Color.FromArgb( 181, 137, 0 ),
                        NoticeColor = Color.FromArgb( 38, 139, 210 ),
                        PromptUserNameColor = Color.FromArgb( 211, 54, 130 ),
                        PromptMachineNameColor = Color.FromArgb( 108, 113, 196 ),
                        PromptDirectoryColor = Color.FromArgb( 133, 153, 0 )
                    },
                ["solarized-light"] = new ColorScheme
                {
                    Background = Color.FromArgb( 253, 246, 227 ),
                    Foreground = Color.FromArgb( 0, 43, 54 ),
                    ErrorColor = Color.FromArgb( 220, 50, 47 ),
                    WarningColor = Color.FromArgb( 181, 137, 0 ),
                    NoticeColor = Color.FromArgb( 38, 139, 210 ),
                    PromptUserNameColor = Color.FromArgb( 211, 54, 130 ),
                    PromptMachineNameColor = Color.FromArgb( 108, 113, 196 ),
                    PromptDirectoryColor = Color.FromArgb( 133, 153, 0 )
                }
            };

            if( !File.Exists( path ) )
            {
                json = JsonConvert.SerializeObject( dict, Formatting.Indented );
                File.WriteAllText( path, json, Encoding.UTF8 );
                return new ThemeManager( dict );
            }

            json = File.ReadAllText( path, Encoding.UTF8 );
            dict = JsonConvert.DeserializeObject<Dictionary<string, ColorScheme>>( json );
            return new ThemeManager( new Dictionary<string, ColorScheme>( dict, StringComparer.OrdinalIgnoreCase ) );
        }
    }
}
