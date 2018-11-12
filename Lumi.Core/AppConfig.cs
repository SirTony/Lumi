using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Lumi.Core
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    public sealed class AppConfig
    {
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public static Assembly Assembly { get; }
        public static string ExecutablePath { get; }
        public static string SourceDirectory { get; }

        private const string FileName = "lumi.json";

        private static readonly string FilePath;

        [JsonProperty( Required = Required.Always )]
        public ColorScheme ColorScheme { get; private set; }

        [JsonProperty( Required = Required.Always )]
        public bool UseTilde { get; private set; }

        [JsonProperty( Required = Required.Default )]
        public string DefaultVariableScope { get; private set; }

        /// <summary>
        ///     Disable certain built-in commands.
        /// </summary>
        [JsonProperty( Required = Required.DisallowNull )]
        public HashSet<string> DisabledCommands { get; private set; }

        /// <summary>
        ///     Disable all built-in commands.
        /// </summary>
        [JsonProperty( Required = Required.Default )]
        public bool DisableAllCommands { get; private set; }

        /// <summary>
        ///     Variables that persist across all instances and shell sessions.
        /// </summary>
        [JsonProperty( Required = Required.Always )]
        public Dictionary<string, object> Persistent { get; private set; }

        /// <summary>
        ///     Temporary variables that only exist as long as the process.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> Temporary { get; private set; }

        static AppConfig()
        {
            AppConfig.Assembly = Assembly.GetEntryAssembly();
            var uri = new Uri( AppConfig.Assembly.CodeBase );

            AppConfig.ExecutablePath = uri.LocalPath;
            AppConfig.SourceDirectory = Path.GetDirectoryName( uri.LocalPath );

            AppConfig.FilePath = Path.Combine( AppConfig.SourceDirectory, AppConfig.FileName );

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if( AppConfig.IsDebug || !File.Exists( AppConfig.FilePath ) )
                AppConfig.SaveDefaultConfig();
        }

        public AppConfig()
            => this.Temporary = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

        public static void SaveDefaultConfig()
        {
            var @default = new AppConfig
            {
                ColorScheme = new ColorScheme(),
                UseTilde = true,
                Persistent = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase ),
                Temporary = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase ),
                DefaultVariableScope = null,
                DisabledCommands = new HashSet<string>( StringComparer.OrdinalIgnoreCase ),
                DisableAllCommands = false
            };

            @default.Save();
        }

        public static AppConfig Load()
        {
            var json = File.ReadAllText( AppConfig.FilePath, Encoding.UTF8 );
            var obj = JsonConvert.DeserializeObject<AppConfig>( json );

            // This dictionary must be case-insensitive and the Comparer property is read-only
            obj.Persistent = new Dictionary<string, object>(
                obj.Persistent,
                StringComparer.OrdinalIgnoreCase
            );

            obj.DisabledCommands = new HashSet<string>(
                obj.DisabledCommands,
                StringComparer.OrdinalIgnoreCase
            );

            return obj;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject( this, Formatting.Indented );
            File.WriteAllText( AppConfig.FilePath, json, Encoding.UTF8 );
        }
    }
}
