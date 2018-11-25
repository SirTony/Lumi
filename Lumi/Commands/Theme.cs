using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Lumi.CommandLine;
using Lumi.Core;
using Lumi.Shell;

namespace Lumi.Commands
{
    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Local",
        Justification = "Private setters are needed by PowerArgs"
    )]
    public sealed class Theme : ICommand
    {
        private delegate void ApplyThemeDelegate( AppConfig config );

        private static readonly IReadOnlyDictionary<string, ApplyThemeDelegate> BuiltInThemes;

        [Positional( 0 )]
        [Required]
        public string ThemeName { get; private set; }

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

            var wndColor = ThemeManager.SystemWindowColor;
            var useLightColours = wndColor.CalculateContrastRatio( Color.WhiteSmoke ) <= 0.6;
            var foreground = useLightColours ? Color.WhiteSmoke : Color.Black;

            config.ColorScheme = new ColorScheme
            {
                Background = wndColor,
                Foreground = foreground,
                NoticeColor = useLightColours ? Color.Aqua : Color.DarkCyan,
                WarningColor = useLightColours ? Color.Gold : Color.DarkGoldenrod,
                ErrorColor = useLightColours ? Color.Red : Color.Firebrick,
                PromptUserNameColor = foreground,
                PromptDirectoryColor = foreground
            };

            config.ColorScheme.Apply();
            Console.Clear();
        }

        public string Name { get; } = "theme";

        public ShellResult Execute( AppConfig config, object input )
        {
            ConsoleEx.WriteWarning(
                "Due to a limitation within Windows, the console is limited to 16 distinct colours. "
              + "If any of the new colours from the selected theme fail to show or colours are "
              + "repeated/incorrect, you have exceeded the 16 colour limit. "
              + "Simply restart the application to fix it."
            );

            if( !Prompt.YesNo( "Continue?", true ) ) return ShellResult.Ok();

            if( Theme.BuiltInThemes.TryGetValue( this.ThemeName, out var applicator ) )
                applicator( config );
            else
            {
                var manager = ThemeManager.Load();
                if( !manager.Themes.TryGetValue( this.ThemeName, out var theme ) )
                    throw new KeyNotFoundException( $"Unknown theme '{this.ThemeName}'" );

                config.ColorScheme = theme;
                config.ColorScheme.Apply();
            }

            Console.Clear();
            config.Save();
            return ShellResult.Ok();
        }
    }
}
