using Colorful;
using Lumi.Config;

namespace Lumi
{
    internal static class ConsoleEx
    {
        public static void WriteWarning( string message )
        {
            Console.Write( "[" );
            Console.Write( "WRN", ConfigManager.Instance.ColorScheme.WarningColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteError( string message )
        {
            Console.Write( "[" );
            Console.Write( "ERR", ConfigManager.Instance.ColorScheme.ErrorColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteNotice( string message )
        {
            Console.Write( "[" );
            Console.Write( "INF", ConfigManager.Instance.ColorScheme.NoticeColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }
    }
}
