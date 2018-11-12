using System;

namespace Lumi.Core
{
    public interface ITypeConverter
    {
        bool CanConvert( Type fromType, Type toType );
        object Convert( Type toType, object value, IFormatProvider provider );
    }
}
