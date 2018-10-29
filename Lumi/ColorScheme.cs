﻿using Newtonsoft.Json;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Console = Colorful.Console;

namespace Lumi
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    internal sealed class ColorScheme
    {
        [JsonProperty( nameof( Foreground ) )]
        private Color? _foreground;

        [JsonProperty( nameof( Background ) )]
        private Color? _background;

        [JsonProperty( nameof( ErrorColor ) )]
        private Color? _errorColor;

        [JsonProperty( nameof( WarningColor ) )]
        private Color? _warningColor;

        [JsonProperty( nameof( NoticeColor ) )]
        private Color? _noticeColor;

        [JsonProperty( nameof( PromptUserNameColor ) )]
        private Color? _promptUserNameColor;

        [JsonProperty( nameof( PromptDirectoryColor ) )]
        private Color? _promptDirectoryColor;

        [JsonIgnore]
        public Color Foreground
        {
            get => this._foreground ?? Console.ForegroundColor;
            set => this._foreground = value;
        }

        [JsonIgnore]
        public Color Background
        {
            get => this._background ?? Console.BackgroundColor;
            set => this._background = value;
        }

        [JsonIgnore]
        public Color ErrorColor
        {
            get => this._errorColor ?? this.Foreground;
            set => this._errorColor = value;
        }

        [JsonIgnore]
        public Color WarningColor
        {
            get => this._warningColor ?? this.Foreground;
            set => this._warningColor = value;
        }

        [JsonIgnore]
        public Color NoticeColor
        {
            get => this._noticeColor ?? this.Foreground;
            set => this._noticeColor = value;
        }

        [JsonIgnore]
        public Color PromptUserNameColor
        {
            get => this._promptUserNameColor ?? this.Foreground;
            set => this._promptUserNameColor = value;
        }

        [JsonIgnore]
        public Color PromptDirectoryColor
        {
            get => this._promptDirectoryColor ?? this.Foreground;
            set => this._promptDirectoryColor = value;
        }

        public ColorScheme()
        {
            this._foreground = Color.FromArgb( 191, 191, 191 );
            this._background = Color.FromArgb( 37, 4, 64 );
            this._errorColor = Color.FromArgb( 255, 67, 131 );
            this._warningColor = Color.FromArgb( 249, 184, 22 );
            this._noticeColor = Color.FromArgb( 29, 136, 241 );
            this._promptUserNameColor = Color.FromArgb( 80, 177, 255 );
            this._promptDirectoryColor = Color.FromArgb( 248, 176, 104 );
        }

        public void Apply()
        {
            if( this._foreground.HasValue )
                Console.ForegroundColor = this.Foreground;

            if( this._background.HasValue )
                Console.BackgroundColor = this.Background;
        }

        public override string ToString()
        {
            var value = new StringBuilder();
            var self = typeof( ColorScheme );
            var fields = from x in self.GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                         let attr = x.GetCustomAttribute<JsonPropertyAttribute>()
                         where attr != null
                         select (attr.PropertyName, (Color?)x.GetValue( this ));

            value.AppendLine( "{" );

            foreach( var (name, x) in fields )
            {
                if( !x.HasValue )
                    continue;

                value.AppendLine( $"    {name} = {ColorTranslator.ToHtml( x.Value )}" );
            }

            return value.Append( "}" ).ToString();
        }
    }
}
