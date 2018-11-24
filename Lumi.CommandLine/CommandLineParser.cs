using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnsureThat;
using Lumi.CommandLine.Models;
using Lumi.Core;
using Lumi.Shell;

namespace Lumi.CommandLine
{
    public sealed class CommandLineParser
    {
        private static readonly Regex OptionRegex;

        static CommandLineParser()
        {
            CommandLineParser.OptionRegex = new Regex(
                @"^--?(?<Name>.+?)([=:](?<Value>.*?))?$",
                RegexOptions.ExplicitCapture | RegexOptions.Compiled
            );
        }

        private static void ParseArguments( IReadOnlyList<string> args, Type applicationType, Action<CommandLineSyntax> setup )
        {
            if( args is null || args.Count == 0 ) return;
            Ensure.That( setup, nameof( setup ) ).IsNotNull();

            var syntax = new CommandLineSyntax( applicationType );
            setup( syntax );

            var commandProcessed = false;
            var arguments = CommandLineParser.ProcessCommandLine( args ).ToArray();
            var helpRequested = arguments.Any( IsHelpRequest );
            CommandModel currentCommand = null;

            foreach( var arg in arguments )
            {
                if( commandProcessed ) break;

                switch( arg )
                {
                    // process command
                    case PositionalOption positional
                        when positional.Position == 0
                          && syntax.Commands.TryGetValue( FindCommand( arg.Value ), out currentCommand ):
                    {
                        if( helpRequested ) break;

                        foreach( var child in arguments.Skip( 1 ) )
                            ProcessArgument( child, currentCommand );

                        currentCommand.CallbackFunc();
                        commandProcessed = true;
                        break;
                    }

                    default:
                        if( helpRequested ) break;
                        ProcessArgument( arg, syntax );
                        break;
                }

                if( helpRequested ) CommandLineParser.PrintHelpScreen( syntax, currentCommand );
            }

            void ProcessArgument( CommandLineOption arg, CommandLineModel currentModel )
            {
                switch( arg )
                {
                    case PositionalOption positional:
                    {
                        var success = currentModel.PositionalArguments.TryGetValue(
                            x => x.AdjustedPosition == positional.Position,
                            out var model
                        );

                        if( !success )
                            throw new CommandLineException( $"Unexpected positional argument at position {positional.Position}" );
                        
                        model.Assigner( model.Parser( positional.Value ) );
                        break;
                    }

                    case LongNamedOption named:
                    {
                        var success = currentModel.NamedArguments.TryGetValue(
                            x => x.HasLongName
                              && x.LongName.Equals(
                                     named.Name,
                                     StringComparison.OrdinalIgnoreCase
                                 ),
                            out var model
                        );

                        if( !success )
                            throw new CommandLineException( $"Unexpected argument --{named.Name}" );

                        model.Assigner( model.Parser( named.Value ) );
                        break;
                    }

                    case ShortNamedOption named:
                    {
                        var success = currentModel.NamedArguments.TryGetValue(
                            x => x.HasShortName
                              && Char.ToLowerInvariant( x.ShortName )
                              == Char.ToLowerInvariant( named.Name ),
                            out var model
                        );

                        if( !success )
                            throw new CommandLineException(
                                $"Unexpected argument -{named.Name}{( named.WasBundled ? $" as part of bundle -{named.Bundle}" : String.Empty )}"
                            );

                        model.Assigner( model.Parser( named.Value ) );
                        break;
                    }
                }
            }

            bool IsHelpRequest( CommandLineOption option )
            {
                switch( option )
                {
                    case ShortNamedOption name when name.Name == '?' || name.Name == 'h':
                        return true;

                    case LongNamedOption name when name.Name.Equals( "help", StringComparison.OrdinalIgnoreCase ):
                        return true;

                    default:
                        return false;
                }
            }

            Predicate<CommandModel> FindCommand( string name )
            {
                return model
                    => model.Command.Equals( name, StringComparison.OrdinalIgnoreCase )
                    || model.Aliases.TryGetValue( name, out _ );
            }
        }

        [SuppressMessage( "ReSharper", "PossibleMultipleEnumeration", Justification = "We don't really care about performance here." )]
        private static void PrintHelpScreen( CommandLineSyntax syntax, CommandModel currentCommand )
        {
            var entry = Assembly.GetEntryAssembly();

            Console.Error.WriteLine(
                "{0} - {1}",
                entry.Get<AssemblyTitleAttribute>().Title,
                entry.Get<AssemblyDescriptionAttribute>().Description
            );

            var usage = syntax.ApplicationType.Get<UsageAttribute>()?.Usage
                     ?? Path.GetFileName( AppConfig.ExecutablePath );
            
            Console.Error.Write( "USAGE: {0} ", usage );

            if( syntax.Commands.Count > 0 )
            {
                if( currentCommand is null )
                    Console.Error.Write( "<command> " );
                else
                    Console.Error.Write( "{0} ", currentCommand.Command );
            }

            Console.Error.WriteLine( "[arguments...]" );

            Console.Error.WriteLine();

            if( syntax.Commands.Count > 0 && currentCommand is null )
            {
                Console.Error.WriteLine( "For additional help, type {0} <command> -h", usage );
                Console.Error.WriteLine();
            }

            if( syntax.Commands.Count > 0 && currentCommand is null )
            {
                Console.Error.WriteLine( "  Commands:" );
                Console.Error.WriteLine();

                foreach( var cmd in syntax.Commands )
                {
                    Console.Error.Write( "    {0}", cmd.Command );
                    if( cmd.Aliases.Count > 0 )
                        Console.Error.WriteLine( " (aliases: {0})", cmd.Aliases.Join( ", " ) );
                    else
                        Console.Error.WriteLine();

                    if( String.IsNullOrWhiteSpace( cmd.DescriptionText ) ) continue;

                    Console.Error.WriteLine( "    {0}", new string( '─', cmd.Command.Length ) );
                    Console.Error.WriteLine( "    {0}", ShellUtility.WordWrap( cmd.DescriptionText, 4 ) );
                    Console.Error.WriteLine();
                }
            }

            PrintNamedArguments( currentCommand is null ? syntax.NamedArguments : currentCommand.NamedArguments );
            PrintPositionalArguments(
                currentCommand is null ? syntax.PositionalArguments : currentCommand.PositionalArguments
            );

            Console.Error.WriteLine();
            Console.Error.Flush();
            Environment.Exit( 1 );

            void PrintNamedArguments( IEnumerable<NamedArgumentModel> arguments )
            {
                if( !arguments.Any() )
                    return;

                Console.Error.WriteLine( "  Named arguments:" );
                Console.Error.WriteLine();
                Console.Error.WriteLine( "    -?, -h, --help" );
                Console.Error.WriteLine( "    ──────────────" );
                Console.Error.WriteLine( "    Show this help screen." );
                Console.Error.WriteLine();

                foreach( var item in arguments )
                {
                    var names = new List<string>();

                    if( item.HasShortName ) names.Add( $"-{item.ShortName}" );
                    if( item.HasLongName ) names.Add( $"--{item.LongName}" );

                    var flags = names.Join( ", " );
                    Console.Error.WriteLine(
                        $"    {{0}}{{1,{Console.BufferWidth / 2}}}",
                        flags,
                        $"[{GetArgumentInformation( item )}]"
                    );

                    if( String.IsNullOrWhiteSpace( item.DescriptionText ) ) continue;
                    
                    Console.Error.WriteLine( "    {0}", new string( '─', flags.Length ) );
                    Console.Error.WriteLine( "    {0}", ShellUtility.WordWrap( item.DescriptionText, 4 ) );
                    Console.Error.WriteLine();
                }
            }

            void PrintPositionalArguments( IEnumerable<PositionalArgumentModel> arguments )
            {
                if( !arguments.Any() )
                    return;

                Console.Error.WriteLine( "  Positional arguments:" );
                Console.Error.WriteLine();

                foreach( var item in arguments )
                {
                    Console.Error.WriteLine(
                        $"Position {{0}}{{1,{Console.BufferWidth / 2}}}",
                        item.Position,
                        $"{GetArgumentInformation( item )}"
                    );

                    if( String.IsNullOrWhiteSpace( item.DescriptionText ) ) continue;

                    Console.Error.WriteLine( "    {0}", new string( '─', $"Position {item.Position}".Length ) );
                    Console.Error.WriteLine( "    {0}", ShellUtility.WordWrap( item.DescriptionText, 4 ) );
                    Console.Error.WriteLine();
                }
            }

            string GetArgumentInformation( ArgumentModel model )
            {
                var info = new List<string>();

                if( model.IsRequired )
                    info.Add( "Required" );

                if( !model.ValueType.IsEnum )
                    info.Add( model.ValueType.Name );
                else
                {
                    var enumNames = Enum.GetNames( model.ValueType ).Join( "," );
                    info.Add( $"{model.ValueType.Name}(one of: {enumNames})" );
                }

                if( model.DefaultValue != null )
                    info.Add( $"Default: {model.DefaultValue}" );

                return info.Join( ", " );
            }
        }

        public static T ParseArguments<T>( IReadOnlyList<string> args, params object[] ctorArgs )
            => (T) CommandLineParser.ParseArguments( typeof( T ), args, ctorArgs );

        public static object ParseArguments( Type type, IReadOnlyList<string> args, params object[] ctorArgs )
        {
            const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var instance = Activator.CreateInstance( type, ctorArgs );
            var commands = type.GetMethods( MemberFlags ).Where( x => x.Has<CommandAttribute>() ).ToList().AsReadOnly();
            var options = type.GetProperties( MemberFlags )
                              .Where( IsCommandLineOption )
                              .Select( x => new PropertyWrapper( x, false ) )
                              .ToList()
                              .AsReadOnly();

            if( commands.Count > 0 && options.Any( x => x.Property.Has<PositionalAttribute>() ) )
                throw new InvalidOperationException( "Cannot mix global positional arguments and commands" );

            CommandLineParser.ParseArguments( args, type, SyntaxSetup );
            
            foreach( var option in options )
            {
                if( !option.Property.Has<RequiredAttribute>() ) continue;
                if( !option.Handled ) throw new CommandLineException( $"Missing required argument {option}" );
            }

            return instance;

            void SyntaxSetup( CommandLineSyntax syntax )
            {
                foreach( var command in commands )
                {
                    var cmdName = command.GetCustomAttribute<CommandAttribute>()?.Name ?? command.Name;
                    if( command.ReturnType != typeof( void ) )
                        throw new InvalidOperationException(
                            $"Method for command {cmdName} must return void"
                        );

                    var aliases = command.GetCustomAttributes<AliasAttribute>()
                                         .Select( x => x.Name )
                                         .Reject( String.IsNullOrWhiteSpace );

                    var cmd = syntax.Command( cmdName );
                    foreach( var alias in aliases )
                        cmd.Alias( alias );

                    cmd.Description( command.GetCustomAttribute<DescriptionAttribute>()?.Description );

                    var parameters = command.GetParameters();
                    if( parameters.Any( x => x.IsOut || x.IsRetval || x.IsLcid ) )
                        throw new InvalidOperationException( "out, retval, and lcid parameters are not supported" );

                    var values = new object[parameters.Length];
                    cmd.Callback( () => command.Invoke( instance, values ) );

                    foreach( var param in parameters )
                    {
                        var named = param.GetCustomAttribute<NamedAttribute>();
                        var positional = param.GetCustomAttribute<PositionalAttribute>();

                        if( named != null && positional != null )
                            throw new InvalidOperationException(
                                $"Parameter {param.Name} for command {cmdName} cannot be both positional and named"
                            );

                        if( named is null && positional is null )
                            values[param.Position] = param.IsOptional && param.HasDefaultValue
                                                         ? param.DefaultValue
                                                         : Activator.CreateInstance( param.ParameterType );

                        ArgumentModel model = null;

                        if( named != null ) model = cmd.Named( named.ShortName, named.LongName );
                        if( positional != null ) model = cmd.Positional( positional.Position );

                        // shouldn't happen
                        if( model is null ) throw new NotSupportedException();

                        model.As( param.ParameterType )
                               .Description( param.Get<DescriptionAttribute>()?.Description )
                               .Required( param.Has<RequiredAttribute>() )
                               .Default( param.HasDefaultValue ? param.DefaultValue : null )
                               .ValueAssigner( x => values[param.Position] = x );

                        if( !param.Has<ValueParserAttribute>() )
                        {
                            model.ValueParser( null );
                            continue;
                        }

                        var parser = (IValueParser) Activator.CreateInstance(
                            param.Get<ValueParserAttribute>().ValueParserType
                        );

                        model.ValueParser( parser.Parse );
                    }
                }

                foreach( var option in options )
                {
                    var named = option.Property.Get<NamedAttribute>();
                    var positional = option.Property.Get<PositionalAttribute>();

                    ArgumentModel model = null;

                    if( named != null ) model = syntax.Named( named.ShortName, named.LongName );
                    if( positional != null ) model = syntax.Positional( positional.Position );

                    // shouldn't happen
                    if( model is null ) throw new NotSupportedException();

                    model.As( option.Property.PropertyType )
                         .Description( option.Property.Get<DescriptionAttribute>()?.Description )
                         .Required( option.Property.Has<RequiredAttribute>() )
                         .Default( option.Property.GetGetMethod( true )?.Invoke( instance, null ) )
                         .ValueAssigner(
                              x => {
                                  var setter = option.Property.GetSetMethod( true );
                                  if( setter is null ) return;

                                  setter.Invoke( instance, new[] { x } );
                                  option.Handled = true;
                              }
                          );

                    if( !option.Property.Has<ValueParserAttribute>() )
                    {
                        model.ValueParser( null );
                        continue;
                    }

                    var parser = (IValueParser) Activator.CreateInstance(
                        option.Property.Get<ValueParserAttribute>().ValueParserType
                    );

                    model.ValueParser( parser.Parse );
                }
            }

            bool IsCommandLineOption( MemberInfo info )
                => info.Has<NamedAttribute>() || info.Has<PositionalAttribute>();
        }

        private static IEnumerable<CommandLineOption> ProcessCommandLine( IReadOnlyList<string> args )
        {
            using( var enumerator = args.Select( ( x, i ) => ( Index: i, Value: x.Trim() ) ).GetEnumerator() )
            {
                while( enumerator.MoveNext() )
                {
                    var (i, item) = enumerator.Current;

                    // stop processing
                    if( item == "--" ) yield break;

                    var match = CommandLineParser.OptionRegex.Match( item );
                    var value = String.IsNullOrWhiteSpace( match.Groups["Value"]?.Value )
                                    ? GetValueFromEnumerator( i )
                                    : match.Groups["Name"].Value;

                    if( !match.Success )
                    {
                        yield return new PositionalOption( i, item );
                        continue;
                    }

                    var name = match.Groups["Name"].Value;
                    if( item.StartsWith( "--" ) )
                    {
                        yield return new LongNamedOption( name, value );
                        continue;
                    }

                    // it's not bundled
                    if( name.Length == 1 )
                    {
                        yield return new ShortNamedOption( name[0], null, value );
                        continue;
                    }

                    foreach( var opt in name )
                        yield return new ShortNamedOption( opt, name, value );
                }

                string GetValueFromEnumerator( int currentPosition )
                {
                    if( currentPosition >= args.Count - 1 )
                        return null;

                    var hasValue = !CommandLineParser.OptionRegex.IsMatch( args[currentPosition + 1] )
                                && enumerator.MoveNext();

                    return hasValue ? enumerator.Current.Value : null;
                }
            }
        }
    }
}
