using System;
using System.Linq;

namespace Lumi.Core.Converters
{
    public sealed class BasicConverter : ITypeConverter
    {
        public bool CanConvert( Type fromType, Type toType )
        {
            if( toType.IsEnum && ( fromType == typeof( string ) || TypeConverter.IntegralTypes.Contains( fromType ) ) )
                return true;

            var convertible = typeof( IConvertible );
            return fromType.IsInstanceOfType( convertible ) && toType.IsInstanceOfType( convertible );
        }

        public object Convert( Type toType, object value, IFormatProvider provider )
        {
            if( toType.IsEnum )
            {
                return value is string name
                           ? Enum.Parse( toType, name, true )
                           : Enum.ToObject( toType, value );
            }

            return System.Convert.ChangeType( value, toType, provider );
        }
    }
}
