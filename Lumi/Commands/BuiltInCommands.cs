using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lumi.Shell;
using Lumi.Shell.Segments;
using PowerArgs;

namespace Lumi.Commands
{
    /*
        TODO
        ====
        This entire class is a janky rushjob that I threw together to get 'cd' working
        for testing purposes. The intention is to redo this class into a proper
        module/plugin system with automatic argument matching and type conversion
        to make defining new built-ins as simple as declaring a new method without
        having to faff with a collection of segments.
    */
    internal static class BuiltInCommands
    {
        private static readonly IDictionary<string, Type> Commands;

        static BuiltInCommands()
        {
            BuiltInCommands.Commands = new Dictionary<string, Type>( StringComparer.OrdinalIgnoreCase );

            var commandType = typeof( ICommand );
            var types = from x in commandType.Assembly.GetTypes()
                        where commandType.IsAssignableFrom( x )
                        let ctor = x.GetConstructor(
                            BindingFlags.Public | BindingFlags.Instance,
                            null,
                            Type.EmptyTypes,
                            null
                        )
                        where ctor != null
                        let aliases = x.GetCustomAttributes<AliasAttribute>()?.Select( y => y.Alias )
                        select (
                                   ( (ICommand) ctor.Invoke( null ) ).Name,
                                   Type: x,
                                   Aliases: aliases ?? Array.Empty<string>()
                               );

            foreach( var ( name, type, aliases) in types )
            {
                var names = aliases.Prepend( name ).Distinct( StringComparer.OrdinalIgnoreCase );
                foreach( var x in names )
                    BuiltInCommands.Commands[x] = type;
            }
        }

        public static bool TryExecute(
            string name, IReadOnlyList<IShellSegment> args, IReadOnlyList<string> input, out ShellResult result
        )
        {
            if( !BuiltInCommands.Commands.TryGetValue( name, out var type ) )
            {
                // we don't actually use result when this method returns false
                // so this is really just for clarity.
                result = ShellResult.Error( -1, $"command '{name}' not found" );
                return false;
            }

            var argv = new List<string>();
            foreach( var item in args )
            {
                result = item.Execute( capture: true );
                if( result.ExitCode != 0 )
                    return true;

                argv.Add( result.StandardOutput.Join( " " ) );
            }

            var command = (ICommand) Args.Parse( type, argv.ToArray() );
            if( command == null )
            {
                result = ShellResult.Ok();
                return true;
            }

            result = command.Execute( input );
            return true;
        }

        public static ShellResult SetVariable( IReadOnlyList<IShellSegment> args )
        {
            if( args.Count != 2 )
                return ShellResult.Error( -1, $"set: incorrect number of arguments. expecting 2, got {args.Count}" );

            if( args[0] is VariableSegment variable )
            {
                var result = args[1].Execute( capture: true );
                return result.ExitCode != 0
                           ? result
                           : variable.SetValue( result.StandardOutput.Join( Environment.NewLine ) );
            }

            return ShellResult.Error( -2, "set: first argument must be a variable" );
        }

        //public static void LoadExtensions( string dir )
        //{
        //    if( !Directory.Exists( dir ) )
        //        return;

        //    var di = new DirectoryInfo( dir );
        //    var types = di.EnumerateFiles( "*.dll", SearchOption.AllDirectories )
        //                  .Select( f => Assembly.LoadFile( f.FullName ) )
        //                  .SelectMany( a => a.ExportedTypes );

        //    foreach( var type in types )
        //    {
        //        var methods = from m in type.GetMethods( BindingFlags.Public | BindingFlags.Static )
        //                      let attr = m.GetCustomAttribute<ShellFunctionAttribute>()
        //                      where attr != null
        //                      let para = m.GetParameters()
        //                      where para.Length == 1
        //                      where para[0].ParameterType == typeof( IReadOnlyList<IShellSegment> )
        //                      where m.ReturnType == typeof( ShellResult )
        //                      select (String.IsNullOrWhiteSpace( attr.Name ) ? m.Name : attr.Name, m);

        //        foreach( var (name, method) in methods )
        //        {
        //            Console.WriteLine(
        //                "Register function {0}.{1} from {2}",
        //                type.FullName,
        //                name,
        //                Path.GetFileName( new Uri( type.Assembly.CodeBase ).LocalPath )
        //            );

        //            var dg = (ShellFunction)Delegate.CreateDelegate( typeof( ShellFunction ), method );
        //            _functions.Add( name.ToLowerInvariant(), dg );
        //        }
        //    }
        //}
    }
}
