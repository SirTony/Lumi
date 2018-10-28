using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lumi
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    internal sealed class ColorScheme
    {
        [JsonProperty, JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleColor? Foreground { get; set; }

        [JsonProperty, JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleColor? Background { get; set; }

        [JsonProperty, JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleColor? Error { get; set; }

        [JsonProperty, JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleColor? PromptUserNameColor { get; set; }

        [JsonProperty, JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleColor? PromptDirectoryColor { get; set; }

        public override string ToString()
        {
            var value = new StringBuilder();
            var self = typeof( ColorScheme );
            var props = from x in self.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                        where x.GetCustomAttribute<JsonPropertyAttribute>() != null
                        let y = x.GetGetMethod( false ).Invoke( this, null )
                        select (x.Name, (ConsoleColor?)y);

            value.AppendLine( "{" );

            foreach( var (name, x) in props )
            {
                if( !x.HasValue )
                    continue;

                value.AppendLine( $"    {name} = {Enum.GetName( typeof( ConsoleColor ), x.Value )}" );
            }

            return value.AppendLine( "}" ).ToString();
        }
    }
}
