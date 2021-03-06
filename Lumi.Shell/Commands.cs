﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnsureThat;
using Lumi.CommandLine;
using Lumi.Core;

namespace Lumi.Shell
{
    public static class Commands
    {
        private static readonly IDictionary<string, Type> CommandTable;

        static Commands() => Commands.CommandTable = new Dictionary<string, Type>();

        public static bool TryGetCommandByName( string name, out Type type )
            => Commands.CommandTable.TryGetValue( name, out type );

        public static bool TryExecuteCommandByName(
            string name, AppConfig config, string[] args, object input, out ShellResult result
        )
        {
            if( !Commands.TryGetCommandByName( name, out var commandType ) )
                throw new ProgramNotFoundException( name, null );

            try
            {
                var (_, parsed) = CommandLineParser.ParseArguments( commandType, args );

                // justification: ShellResult is a ref struct and can't be nullable
                // ReSharper disable once MergeConditionalExpression
                result = parsed is null ? ShellResult.Ok() : ( (ICommand) parsed ).Execute( config, input );
                return true;
            }
            catch( HelpRequestedException )
            {
                result = ShellResult.Error( 1 );
                return true;
            }
        }

        private static void AddCommand( string name, Type type )
        {
            Ensure.That( name, nameof( name ) ).IsNotNullOrWhiteSpace();
            Ensure.That( type, nameof( type ) ).IsNotNull();

            Commands.CommandTable.Add( name, type );
        }

        public static void LoadCommandsFrom( Assembly assembly )
        {
            var commands = assembly.GetTypes().Where( t => typeof( ICommand ).IsAssignableFrom( t ) );

            foreach( var cmd in commands )
            {
                var tempInstance = (ICommand) Activator.CreateInstance( cmd, true );
                Commands.AddCommand( tempInstance.Name, cmd );

                cmd.GetCustomAttributes<CommandAliasAttribute>()
                   .Select( a => a.Alias )
                   .ForEach(
                        a => Commands.AddCommand( a, cmd )
                    );
            }
        }

        public static void LoadCommands()
        {
            var path = Path.Combine( AppConfig.SourceDirectory, "ext" );
            if( !Directory.Exists( path ) ) return;

            var dir = new DirectoryInfo( path );
            dir.EnumerateFiles( "*.dll", SearchOption.AllDirectories )
               .Select( f => Assembly.LoadFile( f.FullName ) )
               .ForEach(
                    Commands.LoadCommandsFrom
                );
        }
    }
}
