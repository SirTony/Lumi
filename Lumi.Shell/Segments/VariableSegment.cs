using System;
using System.Collections.Generic;
using System.Reflection;
using EnsureThat;
using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    public sealed class VariableSegment : IShellSegment
    {
        private static class Scopes
        {
            public const string Invalid = null;
            public const string Configuration = "config";
            public const string System = "system";
            public const string User = "user";
            public const string Persistent = "persistent";
            public const string Temporary = "temporary";
            public const string Process = "process";
        }

        [JsonProperty]
        public string Scope { get; }

        [JsonProperty]
        public string Name { get; }

        public VariableSegment( string name )
            : this( null, name )
        {
        }

        public VariableSegment( string scope, string name )
        {
            Ensure.That( name, nameof( name ) ).IsNotNull();
            Ensure.That( scope, nameof( scope ) ).IsNotEmptyOrWhitespace(); // it can still be null

            this.Scope = scope;
            this.Name = name;
        }

        private object GetValue( AppConfig config, string scope )
        {
            switch( scope )
            {
                case Scopes.Configuration:
                    return config.GetValueFromPropertyPath( this.Name, VariableSegment.IsJsonProperty );

                case Scopes.System:
                    return GetEnvironmentVariable( EnvironmentVariableTarget.Machine );

                case Scopes.User:
                    return GetEnvironmentVariable( EnvironmentVariableTarget.User );

                case Scopes.Persistent:
                    return config.Persistent[this.Name];

                case Scopes.Temporary:
                    return config.Temporary[this.Name];

                case Scopes.Process:
                    return GetEnvironmentVariable( EnvironmentVariableTarget.Process );

                default:
                    throw new NotImplementedException();
            }

            string GetEnvironmentVariable( EnvironmentVariableTarget target )
                => Environment.GetEnvironmentVariable( this.Name, target );
        }

        private void SetValue( AppConfig config, string scope, object value )
        {
            switch( scope )
            {
                case Scopes.Configuration:
                    var ( instance, prop ) = config.GetPropertyFromPath( this.Name, VariableSegment.IsJsonProperty );
                    var setter = prop.GetSetMethod( true );

                    if( setter is null )
                        throw new KeyNotFoundException( $"Unknown config variable '{this.Name}'" );

                    var converted = TypeConverter.Default.Convert( prop.PropertyType, value );
                    setter.Invoke( instance, new[] { converted } );
                    break;

                case Scopes.System:
                    SetEnvironmentVariable( EnvironmentVariableTarget.Machine );
                    break;

                case Scopes.User:
                    SetEnvironmentVariable( EnvironmentVariableTarget.User );
                    break;

                case Scopes.Persistent:
                    config.Persistent[this.Name] = value;
                    break;

                case Scopes.Temporary:
                    config.Temporary[this.Name] = value;
                    break;

                case Scopes.Process:
                    SetEnvironmentVariable( EnvironmentVariableTarget.Process );
                    break;

                default:
                    throw new NotImplementedException();
            }

            void SetEnvironmentVariable( EnvironmentVariableTarget target )
                => Environment.SetEnvironmentVariable( this.Name, TypeConverter.Default.To<string>( value ), target );
        }

        private static bool IsJsonProperty( PropertyInfo prop )
            => prop.GetCustomAttribute<JsonPropertyAttribute>() != null
            && prop.Name != "Persistent"
            && prop.Name != "Temporary";

        public override string ToString() => this.Scope is null ? $"${this.Name}" : $"${this.Scope}:{this.Name}";

        private static string NormalizeScope( string scope, string @default )
        {
            switch( scope )
            {
                case "cfg":
                case "conf":
                case "config":
                    return Scopes.Configuration;

                case "sys":
                case "system":
                    return Scopes.System;

                case "usr":
                case "user":
                    return Scopes.User;

                case "pers":
                case "persist":
                case "persistent":
                    return Scopes.Persistent;

                case "tmp":
                case "temp":
                case "temporary":
                    return Scopes.Temporary;

                case "proc":
                case "process":
                case "env":
                case "environment":
                    return Scopes.Process;

                case null when @default != null:

                    // ReSharper disable once TailRecursiveCall
                    return VariableSegment.NormalizeScope( @default, null );

                default:
                    return Scopes.Invalid;
            }
        }

        [JsonProperty]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public ShellSegmentKind Kind { get; } = ShellSegmentKind.Variable;

        public ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false )
        {
            var normalizedScope = VariableSegment.NormalizeScope(
                this.Scope ?? config.DefaultVariableScope,
                config.DefaultVariableScope
            );

            if( normalizedScope == Scopes.Invalid )
                throw new InvalidOperationException( $"Invalid variable scope '{this.Scope}'" );

            if( input is null )
                return ShellResult.Ok( this.GetValue( config, normalizedScope ) );

            this.SetValue( config, normalizedScope, input );
            return ShellResult.Ok( captureOutput ? input : null );
        }

        public T Accept<T>( ISegmentVisitor<T> visitor ) => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor ) => visitor.Visit( this );
    }
}
