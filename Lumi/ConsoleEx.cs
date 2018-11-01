using Colorful;

namespace Lumi
{
    internal static class ConsoleEx
    {
        public static void WriteWarning( string message )
        {
            Console.Write( "[" );
            Console.Write( "WRN", Program.AppConfig.ColorScheme.WarningColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteError( string message )
        {
            Console.Write( "[" );
            Console.Write( "ERR", Program.AppConfig.ColorScheme.ErrorColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteNotice( string message )
        {
            Console.Write( "[" );
            Console.Write( "INF", Program.AppConfig.ColorScheme.NoticeColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }
    }
}
