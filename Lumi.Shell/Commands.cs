using System.Collections.Generic;
using EnsureThat;

namespace Lumi.Shell
{
    public static class Commands
    {
        private static readonly IDictionary<string, ICommand> CommandTable;

        static Commands() => Commands.CommandTable = new Dictionary<string, ICommand>();

        public static bool TryGetCommandByName( string name, out ICommand command )
            => Commands.CommandTable.TryGetValue( name, out command );

        public static ICommand GetCommandByName( string name )
            => Commands.CommandTable[name];

        public static bool TryExecuteCommandByName( string name, object input, out ShellResult result )
        {
            if( !Commands.TryGetCommandByName( name, out var command ) )
            {
                result = ShellResult.Error( -1, new ProgramNotFoundException( name, null ) );
                return false;
            }

            result = command.Execute( input );
            return true;
        }

        public static ShellResult ExecuteCommandByName( string name, object input = null )
            => Commands.GetCommandByName( name ).Execute( input );

        public static void AddCommand( string name, ICommand command )
        {
            Ensure.That( name, nameof( name ) ).IsNotNullOrWhiteSpace();
            Ensure.That( command, nameof( command ) ).IsNotNull();
            Commands.CommandTable.Add( name, command );
        }
    }
}
