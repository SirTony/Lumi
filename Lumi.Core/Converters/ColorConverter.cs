using System;
using System.Drawing;

namespace Lumi.Core.Converters
{
    internal sealed class ColorConverter : ITypeConverter
    {
        public bool CanConvert( Type fromType, Type toType )
            => toType == typeof( Color )
            && ( fromType == typeof( string ) || typeof( int ).IsAssignableFrom( fromType ) );

        public object Convert( Type toType, object value, IFormatProvider provider )
            => value is string s
                   ? ColorTranslator.FromHtml( s )
                   : ColorTranslator.FromWin32( (int) System.Convert.ChangeType( value, TypeCode.Int32 ) );
    }
}
