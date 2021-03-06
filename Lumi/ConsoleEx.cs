﻿using Colorful;

namespace Lumi
{
    internal static class ConsoleEx
    {
        public static void WriteWarning( string message )
        {
            Console.Write( "[" );
            Console.Write( "WRN", Program.Config.ColorScheme.WarningColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteError( string message )
        {
            Console.Write( "[" );
            Console.Write( "ERR", Program.Config.ColorScheme.ErrorColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }

        public static void WriteNotice( string message )
        {
            Console.Write( "[" );
            Console.Write( "INF", Program.Config.ColorScheme.NoticeColor );
            Console.Write( "] :: " );
            Console.WriteLine( message );
        }
    }
}
