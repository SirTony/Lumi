using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
    [JsonObject( MemberSerialization.OptIn )]
    internal sealed class VariableSegment : IShellSegment
    {
        public static class Scopes
        {
            public const string Invalid = null;
            public const string Configuration = "config";
            public const string System = "system";
            public const string User = "user";
            public const string Persistent = "persistent";
            public const string Temporary = "temporary";
            public const string Process = "process";
        }

        [JsonProperty( nameof( VariableSegment.Scope ) )]
        private readonly string _scope;

        public string Scope
        {
            get
            {
                switch( this._scope )
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
                    case null:
                        return Scopes.Temporary;

                    case "proc":
                    case "process":
                    case "env":
                    case "environment":
                        return Scopes.Process;

                    default:
                        return Scopes.Invalid;
                }
            }
        }

        [JsonProperty]
        public string Name { get; }

        public VariableSegment( IShellSegment parent, string name )
            : this( parent, null, name )
        {
        }

        public VariableSegment( IShellSegment parent, string scope, string name )
        {
            this.Parent = parent;
            this._scope = scope;
            this.Name = name;
        }

        public T As<T>()
        {
            var result = this.Execute();
            if( result.ExitCode != 0 )
                throw new InvalidOperationException( result.StandardError.Join( Environment.NewLine ) );

            var (ok, value) = ValueConverter.Default.TryConvert( result.StandardOutput[0], typeof( T ) );
            if( !ok )
                throw new InvalidCastException( $"cannot convert value to type {typeof( T ).Name}" );

            return (T) value;
        }

        public bool Is<T>()
        {
            try
            {
                this.As<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ShellResult SetValue( string value )
            => this.SetValueImpl( value, false );

        private ShellResult SetValueImpl( string value, bool suppressEvent )
        {
            switch( this.Scope )
            {
                case Scopes.Configuration:
                {
                    var (instance, prop) = Program.Config.GetPropertyFromPath(
                        this.Name,
                        VariableSegment.IsJsonProperty
                    );

                    var setter = prop?.GetSetMethod( true );

                    if( instance == null || prop == null || setter == null )
                        return this.Error();

                    var (ok, converted) = ValueConverter.Default.TryConvert( value, prop.PropertyType );

                    if( !ok )
                        return ShellResult.Error( -3, $"set: cannot convert value to type {prop.PropertyType.Name}" );

                    try
                    {
                        setter.Invoke( instance, new[] { converted } );
                        Program.Config.Save();
                        return ShellResult.Ok( converted.ToString() );
                    }
                    catch( Exception ex )
                    {
                        var msgs = new[] { "set: could not set value. reason:", ex.Message };
                        return ShellResult.Error( -4, msgs );
                    }
                }

                case Scopes.System:
                    return SetVariable( EnvironmentVariableTarget.Machine );

                case Scopes.User:
                    return SetVariable( EnvironmentVariableTarget.User );

                case Scopes.Persistent:
                    Program.Config.Persistent[this.Name] = value;
                    Program.Config.Save();
                    return ShellResult.Ok( value );

                case Scopes.Temporary:
                    Program.Config.Temporary[this.Name] = value;
                    return ShellResult.Ok( value );

                case Scopes.Process:
                    return SetVariable( EnvironmentVariableTarget.Process );

                default:
                    return ShellResult.Error( -2, $"{this}: invalid variable scope: {this._scope}" );
            }

            ShellResult SetVariable( EnvironmentVariableTarget target )
            {
                Environment.SetEnvironmentVariable( this.Name, value, target );
                return ShellResult.Ok( value );
            }
        }

        private ShellResult Error() => ShellResult.Error( -1, $"no such variable '{this.Name}'" );

#pragma warning disable IDE0046 // Convert to conditional expression
        private static bool IsJsonProperty( PropertyInfo info )
        {
            if( info is null )
                return false;

            if( info.DeclaringType == typeof( ColorScheme ) )
                return info.GetCustomAttribute<JsonIgnoreAttribute>() != null;

            return info.GetCustomAttribute<JsonPropertyAttribute>() != null;
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        public override string ToString()
            => this._scope == null ? $"${this.Name}" : $"${this._scope}:{this.Name}";

        private ShellResult GetValue()
        {
            switch( this.Scope )
            {
                case Scopes.Configuration:
                {
                    var value = Program.Config.GetValueFromPropertyPath(
                        this.Name,
                        VariableSegment.IsJsonProperty
                    );
                    return value == null
                               ? this.Error()
                               : ShellResult.Ok( value.ToString() );
                }

                case Scopes.System:
                    return GetVariable( EnvironmentVariableTarget.Machine );

                case Scopes.User:
                    return GetVariable( EnvironmentVariableTarget.User );

                case Scopes.Persistent:
                {
                    return Program.Config.Persistent.TryGetValue( this.Name, out var result )
                               ? ShellResult.Ok( result )
                               : this.Error();
                }

                case Scopes.Temporary:
                {
                    return Program.Config.Temporary.TryGetValue( this.Name, out var result )
                               ? ShellResult.Ok( result )
                               : this.Error();
                }

                case Scopes.Process:
                    return GetVariable( EnvironmentVariableTarget.Process );

                default:
                    return ShellResult.Error( -2, $"{this}: invalid variable scope: {this._scope}" );
            }

            ShellResult GetVariable( EnvironmentVariableTarget target )
            {
                var value = Environment.GetEnvironmentVariable( this.Name, target );
                return value != null ? ShellResult.Ok( value ) : this.Error();
            }
        }

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
            => inputs?.Count >= 1
                   ? this.SetValue(
                       inputs.Reject( String.IsNullOrWhiteSpace )
                             .Select( x => x.Trim() )
                             .Join( Environment.NewLine )
                   )
                   : this.GetValue();
    }
}
