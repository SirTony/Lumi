using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Lumi
{
    public sealed class ValueConverter
    {
        public delegate (bool, object) Delegate( object value, Type targetType );

        public static IReadOnlyList<Type> IntegralTypes { get; }
        public static IReadOnlyList<Type> FloatTypes { get; }
        public static IReadOnlyList<Type> NumericTypes { get; }

        public static ValueConverter Default { get; }

        static ValueConverter()
        {
            IntegralTypes = new[]
            {
                typeof( sbyte ), typeof( byte ),
                typeof( short ), typeof( ushort ),
                typeof( int ), typeof( uint ),
                typeof( long ), typeof( ulong ),
            };

            FloatTypes = new[] { typeof( float ), typeof( double ), typeof( decimal ) };

            NumericTypes = IntegralTypes.Concat( FloatTypes ).ToArray();

            Default = new ValueConverter();
        }

        public readonly IDictionary<Type, Delegate> _table;

        public Delegate this[Type type]
        {
            get => this._table[type];
            set => this._table[type] = value;
        }

        public ValueConverter( IDictionary<Type, Delegate> table = null )
        {
            this._table = table ?? new Dictionary<Type, Delegate>();

            this._table[typeof( Color )] = delegate ( object value, Type targetType ) {
                if( targetType != typeof( Color ) )
                    return (false, null);

                try { return (true, ColorTranslator.FromHtml( value.ToString() )); }
                catch { return (false, null); }
            };
        }

        public void Add( Type type, Delegate dg )
            => this._table.Add( type, dg );

        public (bool, object) TryConvert( object value, Type targetType )
        {
            var type = value.GetType();

            if( this._table.TryGetValue( targetType, out var dg ) )
                return dg( value, targetType );

            if( targetType.IsConstructedGenericType && typeof( Nullable<> ).IsAssignableFrom( targetType.GetGenericTypeDefinition() ) )
            {
                var actualTarget = targetType.GenericTypeArguments[0];
                if( value == null || ( value is string s && String.IsNullOrWhiteSpace( s ) ) )
                    return (true, Activator.CreateInstance( targetType ));

                var (ok, converted) = this.TryConvert( value, actualTarget );
                return ok ? (true, Activator.CreateInstance( targetType, new[] { converted } )) : (false, null);
            }

            if( targetType.IsEnum )
            {
                if( IntegralTypes.Contains( type ) && Enum.IsDefined( targetType, value ) )
                    return (true, Enum.ToObject( targetType, value ));

                if( value is string name )
                {
                    try { return (true, Enum.Parse( targetType, name, true )); }
                    catch { }
                }
            }

            if( typeof( IConvertible ).IsAssignableFrom( type ) )
            {
                try { return (true, Convert.ChangeType( value, targetType, CultureInfo.CurrentCulture )); }
                catch { }
            }

            return (false, null);
        }
    }
}
