using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Parsing
{
    [JsonObject( MemberSerialization.OptIn )]
    public readonly struct SyntaxToken<TKind> where TKind : Enum
    {
        [JsonProperty]
        public string Text { get; }

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public TKind Kind { get; }

        [JsonProperty]
        public TextSpan Span { get; }

        [JsonProperty]
        public object Data { get; }

        public SyntaxToken( string text, TKind kind, TextSpan span, object data = null )
        {
            this.Text = text;
            this.Kind = kind;
            this.Span = span;
            this.Data = data;
        }
    }
}
