using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lumi.Config;
using Lumi.Shell.Visitors;
using Newtonsoft.Json;

namespace Lumi.Shell.Segments
{
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

        private static readonly IDictionary<string, IList<EventHandler<VariableValueChangedEventArgs>>> Watchers;

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

        public string Name { get; }

        static VariableSegment()
            => VariableSegment.Watchers =
                   new Dictionary<string, IList<EventHandler<VariableValueChangedEventArgs>>>(
                       StringComparer.OrdinalIgnoreCase
                   );

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
            ShellResult result;
            var oldValue = this.Execute().StandardOutput.Join( Environment.NewLine );

            switch( this.Scope )
            {
                case Scopes.Configuration:
                {
                    var (instance, prop) =
                        ConfigManager.Instance.GetPropertyFromPath( this.Name, VariableSegment.IsJsonProperty );
                    var setter = prop?.GetSetMethod( true );

                    if( instance == null || prop == null || setter == null )
                    {
                        result = this.Error();
                        break;
                    }

                    var (ok, converted) = ValueConverter.Default.TryConvert( value, prop.PropertyType );

                    if( !ok )
                    {
                        result = ShellResult.Error( -3, $"set: cannot convert value to type {prop.PropertyType.Name}" );
                        break;
                    }

                    try
                    {
                        setter.Invoke( instance, new[] { converted } );
                        ConfigManager.Save();
                        result = ShellResult.Ok( converted.ToString() );
                    }
                    catch( Exception ex )
                    {
                        var msgs = new[] { "set: could not set value. reason:", ex.Message };

                        result = ShellResult.Error( -4, msgs );
                    }

                    break;
                }

                case Scopes.System:
                    result = SetVariable( EnvironmentVariableTarget.Machine );
                    break;

                case Scopes.User:
                    result = SetVariable( EnvironmentVariableTarget.User );
                    break;

                case Scopes.Persistent:
                    ConfigManager.Instance.Persistent[this.Name] = value;
                    ConfigManager.Save();
                    result = ShellResult.Ok( value );
                    break;

                case Scopes.Temporary:
                    ConfigManager.Instance.Temporary[this.Name] = value;
                    result = ShellResult.Ok( value );
                    break;

                case Scopes.Process:
                    result = SetVariable( EnvironmentVariableTarget.Process );
                    break;

                case Scopes.Invalid:
                default:
                    return ShellResult.Error( -2, $"{this}: invalid variable scope: {this._scope}" );
            }

            if( !suppressEvent && VariableSegment.Watchers.TryGetValue( this.Name, out var handlers ) )
            {
                // make sure we dont needlessly reset the variable
                // if multiple handlers trigger a revert.
                var hasReverted = false;

                foreach( var handler in handlers )
                {
                    var args = new VariableValueChangedEventArgs( value, oldValue );
                    handler( this, args );

                    if( args.Revert && !hasReverted )
                    {
                        this.SetValueImpl( oldValue, true );
                        result = ShellResult.Ok( oldValue );
                        hasReverted = true;
                    }
                }
            }

            return result;

            ShellResult SetVariable( EnvironmentVariableTarget target )
            {
                Environment.SetEnvironmentVariable( this.Name, value, target );
                return ShellResult.Ok( value );
            }
        }

        public void WatchForChange( EventHandler<VariableValueChangedEventArgs> handler )
            => VariableSegment.WatchForChange( this.Name, handler );

        private ShellResult Error() => ShellResult.Error( -1, $"no such variable '{this.Name}'" );

        public static void WatchForChange( string name, EventHandler<VariableValueChangedEventArgs> handler )
        {
            if( VariableSegment.Watchers.TryGetValue( name, out var list ) )
            {
                list.Add( handler );
                return;
            }

            VariableSegment.Watchers[name] = new List<EventHandler<VariableValueChangedEventArgs>> { handler };
        }

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
            => this._scope == null ? $"${this.Name}" : $"$[{this._scope}]{this.Name}";

        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var data = new List<string>();

            switch( this.Scope )
            {
                case Scopes.Configuration:
                {
                    var value = ConfigManager.Instance.GetValueFromPropertyPath(
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
                    return ConfigManager.Instance.Persistent.TryGetValue( this.Name, out var result )
                               ? ShellResult.Ok( result )
                               : this.Error();
                }

                case Scopes.Temporary:
                {
                    return ConfigManager.Instance.Temporary.TryGetValue( this.Name, out var result )
                               ? ShellResult.Ok( result )
                               : this.Error();
                }

                case Scopes.Process:
                    return GetVariable( EnvironmentVariableTarget.Process );

                case Scopes.Invalid:
                default:
                    return ShellResult.Error( -2, $"{this}: invalid variable scope: {this._scope}" );
            }

            ShellResult GetVariable( EnvironmentVariableTarget target )
            {
                var value = Environment.GetEnvironmentVariable( this.Name, target );
                return value != null ? ShellResult.Ok( value ) : this.Error();
            }
        }
    }
}
