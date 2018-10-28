using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lumi
{
    internal static class Kernel32
    {
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        private static extern uint GetLongPathName( string ShortPath, StringBuilder sb, int buffer );

        [DllImport( "kernel32.dll" )]
        private static extern uint GetShortPathName( string longpath, StringBuilder sb, int buffer );

        public static string GetWindowsPhysicalPath( string path )
        {
            var builder = new StringBuilder( 255 );

            GetShortPathName( path, builder, builder.Capacity );

            path = builder.ToString();

            var result = GetLongPathName( path, builder, builder.Capacity );

            if( result > 0 && result < builder.Capacity )
            {
                builder[0] = Char.ToLower( builder[0] );
                return builder.ToString( 0, (int)result );
            }

            if( result > 0 )
            {
                builder = new StringBuilder( (int)result );
                result = GetLongPathName( path, builder, builder.Capacity );
                builder[0] = Char.ToLower( builder[0] );
                return builder.ToString( 0, (int)result );
            }

            return path;
        }
    }
}
