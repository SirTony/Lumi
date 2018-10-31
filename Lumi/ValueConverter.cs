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

        public readonly IDictionary<Type, Delegate> Table;

        public static IReadOnlyList<Type> IntegralTypes { get; }
        public static IReadOnlyList<Type> FloatTypes { get; }
        public static IReadOnlyList<Type> NumericTypes { get; }

        public static ValueConverter Default { get; }

        public Delegate this[ Type type ]
        {
            get => this.Table[type];
            set => this.Table[type] = value;
        }

        static ValueConverter()
        {
            ValueConverter.IntegralTypes = new[]
            {
                typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ),
                typeof( long ), typeof( ulong )
            };

            ValueConverter.FloatTypes = new[] { typeof( float ), typeof( double ), typeof( decimal ) };

            ValueConverter.NumericTypes = ValueConverter.IntegralTypes.Concat( ValueConverter.FloatTypes ).ToArray();

            ValueConverter.Default = new ValueConverter();
        }

        public ValueConverter( IDictionary<Type, Delegate> table = null )
        {
            this.Table = table ?? new Dictionary<Type, Delegate>();

            this.Table[typeof( Color )] = delegate( object value, Type targetType ) {
                if( targetType != typeof( Color ) )
                    return ( false, null );

                try { return ( true, ColorTranslator.FromHtml( value.ToString() ) ); }
                catch { return ( false, null ); }
            };
        }

        public void Add( Type type, Delegate dg )
            => this.Table.Add( type, dg );

        public (bool, object) TryConvert( object value, Type targetType )
        {
            var type = value.GetType();

            if( this.Table.TryGetValue( targetType, out var dg ) )
                return dg( value, targetType );

            if( targetType.IsConstructedGenericType
             && typeof( Nullable<> ).IsAssignableFrom( targetType.GetGenericTypeDefinition() ) )
            {
                var actualTarget = targetType.GenericTypeArguments[0];
                if( value == null || value is string s && String.IsNullOrWhiteSpace( s ) )
                    return ( true, Activator.CreateInstance( targetType ) );

                var (ok, converted) = this.TryConvert( value, actualTarget );
                return ok ? ( true, Activator.CreateInstance( targetType, converted ) ) : ( false, null );
            }

            if( targetType.IsEnum )
            {
                if( ValueConverter.IntegralTypes.Contains( type ) && Enum.IsDefined( targetType, value ) )
                    return ( true, Enum.ToObject( targetType, value ) );

                if( value is string name )
                {
                    try { return ( true, Enum.Parse( targetType, name, true ) ); }
                    catch { }
                }
            }

            if( typeof( IConvertible ).IsAssignableFrom( type ) )
            {
                try { return ( true, Convert.ChangeType( value, targetType, CultureInfo.CurrentCulture ) ); }
                catch { }
            }

            return ( false, null );
        }
    }
}
