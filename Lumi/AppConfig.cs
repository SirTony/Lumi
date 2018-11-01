using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Lumi
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    internal sealed class AppConfig
    {
        private const string FileName = "lumi.json";

        private static readonly string FilePath;

        [JsonProperty( Required = Required.Always )]
        public ColorScheme ColorScheme { get; private set; }

        [JsonProperty( Required = Required.Always )]
        public bool UseTilde { get; private set; }

        [JsonProperty( Required = Required.Default )]
        public string DefaultVariableScope { get; private set; }

        /// <summary>
        ///     Variables that persist across all instances and shell sessions.
        /// </summary>
        [JsonProperty( Required = Required.Always )]
        public Dictionary<string, string> Persistent { get; private set; }

        /// <summary>
        ///     Temporary variables that only exist as long as the process.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> Temporary { get; private set; }

        static AppConfig()
        {
#if DEBUG
            const bool ForceOverwrite = true;
#else
            const bool ForceOverwrite = false;
#endif

            AppConfig.FilePath = Path.Combine( Program.SourceDirectory, AppConfig.FileName );

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if( ForceOverwrite || !File.Exists( AppConfig.FilePath ) )
                AppConfig.SaveDefaultConfig();
        }

        public AppConfig()
            => this.Temporary = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        public static void SaveDefaultConfig()
        {
            var @default = new AppConfig
            {
                ColorScheme = new ColorScheme(),
                UseTilde = true,
                Persistent = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ),
                Temporary = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ),
                DefaultVariableScope = null
            };

            @default.Save();
        }

        public static AppConfig Load()
        {
            var json = File.ReadAllText( AppConfig.FilePath, Encoding.UTF8 );
            var obj = JsonConvert.DeserializeObject<AppConfig>( json );

            // This dictionary must be case-insensitive and the Comparer property is read-only
            obj.Persistent = new Dictionary<string, string>(
                obj.Persistent,
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
