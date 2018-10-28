using Lumi.Config;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lumi.Shell.Segments
{
    internal sealed class VariableSegment : IShellSegment
    {
        public string Scope { get; }
        public string Name { get; }
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public VariableSegment( IShellSegment parent, string name )
            : this( parent, null, name ) { }

        public VariableSegment( IShellSegment parent, string scope, string name )
        {
            this.Parent = parent;
            this.Scope = scope;
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

            return (T)value;
        }

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var data = new List<string>();

            switch( this.Scope )
            {
                case "cfg":
                case "conf":
                case "config":
                {
                    var value = ConfigManager.Instance.GetValueFromPropertyPath( this.Name, IsJsonProperty );
                    return value == null
                         ? this.Error()
                         : ShellResult.Ok( value.ToString() );
                }

                case "sys":
                case "system":
                    return GetVariable( EnvironmentVariableTarget.Machine );

                case "usr":
                case "user":
                    return GetVariable( EnvironmentVariableTarget.User );

                case "pers":
                case "persist":
                case "persistent":
                {
                    return ConfigManager.Instance.Persistent.TryGetValue( this.Name, out var result )
                         ? ShellResult.Ok( result )
                         : this.Error();
                }

                case "tmp":
                case "temp":
                case "temporary":
                case null:
                {
                    return ConfigManager.Instance.Temporary.TryGetValue( this.Name, out var result )
                         ? ShellResult.Ok( result )
                         : this.Error();
                }

                case "proc":
                case "process":
                case "env":
                case "environment":
                    return GetVariable( EnvironmentVariableTarget.Process );

                default:
                    return ShellResult.Error( -2, $"{this.ToString()}: invalid variable scope: {this.Scope}" );
            }

            ShellResult GetVariable( EnvironmentVariableTarget target )
            {
                var value = Environment.GetEnvironmentVariable( this.Name, target );
                return value != null ? ShellResult.Ok( value ) : this.Error();
            }
        }

        public ShellResult SetValue( string value )
        {
            switch( this.Scope )
            {
                case "cfg":
                case "conf":
                case "config":
                {
                    var (instance, prop) = ConfigManager.Instance.GetPropertyFromPath( this.Name, IsJsonProperty );
                    var setter = prop?.GetSetMethod( true );

                    if( instance == null || prop == null || setter == null )
                        return this.Error();

                    var (ok, converted) = ValueConverter.Default.TryConvert( value, prop.PropertyType );

                    if( !ok )
                        return ShellResult.Error( -3, $"set: cannot convert value to type {prop.PropertyType.Name}" );

                    try
                    {
                        setter.Invoke( instance, new[] { converted } );
                        ConfigManager.Save();
                        return ShellResult.Ok( converted.ToString() );
                    }
                    catch( Exception ex )
                    {
                        var msgs = new[]
                        {
                            "set: could not set value. reason:",
                            ex.Message
                        };

                        return ShellResult.Error( -4, msgs );
                    }
                }

                case "sys":
                case "system":
                    return SetVariable( EnvironmentVariableTarget.Machine );

                case "usr":
                case "user":
                    return SetVariable( EnvironmentVariableTarget.User );

                case "pers":
                case "persist":
                case "persistent":
                    ConfigManager.Instance.Persistent[this.Name] = value.ToString();
                    ConfigManager.Save();
                    return ShellResult.Ok( value.ToString() );

                case "tmp":
                case "temp":
                case "temporary":
                case null:
                    ConfigManager.Instance.Temporary[this.Name] = value.ToString();
                    return ShellResult.Ok( value.ToString() );

                case "proc":
                case "process":
                case "env":
                case "environment":
                    return SetVariable( EnvironmentVariableTarget.Process );

                default:
                    return ShellResult.Error( -2, $"{this.ToString()}: invalid variable scope: {this.Scope}" );
            }

            ShellResult SetVariable( EnvironmentVariableTarget target )
            {
                Environment.SetEnvironmentVariable( this.Name, value.ToString(), target );
                return ShellResult.Ok( value.ToString() );
            }
        }

        private ShellResult Error() => ShellResult.Error( -1, $"no such variable '{this.Name}'" );

        private static bool IsJsonProperty( PropertyInfo info )
            => info?.GetCustomAttribute<JsonPropertyAttribute>() != null;

        public override string ToString()
            => this.Scope == null ? $"${this.Name}" : $"${this.Scope}:{this.Name}";
    }
}
