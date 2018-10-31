using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Lumi.Config
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    internal sealed class ConfigManager
    {
        private const string FileName = "lumi.json";

        private static readonly string FilePath;

        public static ConfigManager Instance { get; }
        public static ConfigManager Default { get; }

        [JsonProperty( Required = Required.Always )]
        public ColorScheme ColorScheme { get; private set; }

        [JsonProperty( Required = Required.Always )]
        public bool UseTilde { get; private set; }

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

        static ConfigManager()
        {
#if DEBUG
            const bool ForceOverwrite = true;
#else
            const bool ForceOverwrite = false;
#endif

            ConfigManager.FilePath = Path.Combine( Program.SourceDirectory, ConfigManager.FileName );

            ConfigManager.Default = new ConfigManager
            {
                ColorScheme = new ColorScheme(),
                UseTilde = true,
                Persistent = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ),
                Temporary = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
            };

            if( ForceOverwrite || !File.Exists( ConfigManager.FilePath ) )
                ConfigManager.SaveImpl( ConfigManager.Default );

            var json = File.ReadAllText( ConfigManager.FilePath, Encoding.UTF8 );
            ConfigManager.Instance = JsonConvert.DeserializeObject<ConfigManager>( json );

            // This dictionary must be case-insensitive and the Comparer property is read-only
            ConfigManager.Instance.Persistent = new Dictionary<string, string>(
                ConfigManager.Instance.Persistent,
                StringComparer.OrdinalIgnoreCase
            );

            // Temp variables don't exist in hte JSON file, so set up the dictionary manually
            ConfigManager.Instance.Temporary = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
        }

        public static void Save()
            => ConfigManager.SaveImpl( ConfigManager.Instance );

        private static void SaveImpl( ConfigManager what )
        {
            var json = JsonConvert.SerializeObject( what, Formatting.Indented );
            File.WriteAllText( ConfigManager.FilePath, json, Encoding.UTF8 );
        }
    }
}
