using Lumi.Shell;
using Lumi.Shell.Segments;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
        private static readonly IDictionary<string, ShellFunction> _functions;

        static BuiltInCommands()
        {
            _functions = new Dictionary<string, ShellFunction>
            {
                ["cd"] = ChangeDirectory,
                ["pwd"] = PrintWorkingDirectory,
                ["exit"] = Exit,
            };
        }

        public static bool TryFind( string name, out ShellFunction func )
        {
            foreach( var (k, v) in _functions )
            {
                if( String.Equals( k, name, StringComparison.OrdinalIgnoreCase ) )
                {
                    func = v;
                    return true;
                }
            }

            func = null;
            return false;
        }

        public static ShellResult SetVariable( IReadOnlyList<IShellSegment> args )
        {
            if( args.Count != 2 )
                return ShellResult.Error( -1, $"set: incorrect number of arguments. expecting 2, got {args.Count}" );

            if( args[0] is VariableSegment variable )
            {
                var result = args[1].Execute( capture: true );
                return result.ExitCode != 0 ? result : variable.SetValue( result.StandardOutput.Join( Environment.NewLine ) );
            }

            return ShellResult.Error( -2, $"set: first argument must be a variable" );
        }

        private static ShellResult Exit( IReadOnlyList<IShellSegment> args )
        {
            if( args.Count == 0 )
                Environment.Exit( Environment.ExitCode );

            if( args.Count == 1 && args[0] is TextSegment text )
            {
                var success = Int32.TryParse( text.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code );
                if( success )
                    Environment.Exit( code );

                return ShellResult.Error( Int32.MinValue, "exit: exit code must be an integer" );
            }

            return args.Count > 1
                ? ShellResult.Error( Int32.MinValue + 1, $"exit: incorrect number of arguments. expecting 0 or 1, got {args.Count}" )
                : ShellResult.Error( Int32.MinValue + 2, $"exit: incorrect argument type. expecting number, got {args[0].GetFriendlyName()}" );
        }

        private static ShellResult ChangeDirectory( IReadOnlyList<IShellSegment> args )
        {
            if( args.Count == 0 )
            {
                Directory.SetCurrentDirectory( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) );
                return ShellResult.Ok();
            }

            if( args.Count == 1 && args[0] is TextSegment text )
            {
                var path = Path.GetFullPath( ShellUtil.ProcessTilde( text.Text ) );

                if( File.Exists( path ) )
                    return ShellResult.Error( -3, $"cd: cannot change directory to '{text.Text}' because it is a file" );

                if( !Directory.Exists( path ) )
                    return ShellResult.Error( -4, $"cd: cannot change directory to '{text.Text}' because it does not exist" );

                Directory.SetCurrentDirectory( path );
                return ShellResult.Ok();
            }

            return args.Count > 1
                ? ShellResult.Error( -1, $"cd: incorrect number of arguments. expecting 0 or 1, got {args.Count}" )
                : ShellResult.Error( -2, $"cd: incorrect argument type. expected text, got {args[0].GetFriendlyName()}" );
        }

        private static ShellResult PrintWorkingDirectory( IReadOnlyList<IShellSegment> args )
            => args.Count != 0
             ? ShellResult.Error( -1, "pwd: command does not accept arguments" )
             : ShellResult.Ok( Directory.GetCurrentDirectory() );

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
