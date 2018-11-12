using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Lumi.Core.Converters;

namespace Lumi.Core
{
    public sealed class TypeConverter
    {
        private readonly IList<ITypeConverter> _converters;
        public static TypeConverter Default { get; }
        public IReadOnlyList<ITypeConverter> Converters => this.Converters;

        public static IReadOnlyList<Type> IntegralTypes { get; }
        public static IReadOnlyList<Type> FloatTypes { get; }
        public static IReadOnlyList<Type> NumericTypes { get; }

        static TypeConverter()
        {
            TypeConverter.IntegralTypes = new[]
            {
                typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ),
                typeof( long ), typeof( ulong )
            };

            TypeConverter.FloatTypes = new[] { typeof( float ), typeof( double ), typeof( decimal ) };

            TypeConverter.NumericTypes = TypeConverter.IntegralTypes.Concat( TypeConverter.FloatTypes ).ToArray();

            TypeConverter.Default = new TypeConverter(
                new[] { new BasicConverter() }
            );
        }

        public TypeConverter( IEnumerable<ITypeConverter> converters = null )
            => this._converters = new List<ITypeConverter>( converters ?? Enumerable.Empty<ITypeConverter>() );

        public void Add( ITypeConverter converter )
        {
            Ensure.That( converter, nameof( converter ) ).IsNotNull();
            this._converters.Add( converter );
        }

        public T To<T>( object value, IFormatProvider provider = null )
            => (T) this.Convert( typeof( T ), value, provider );

        public object Convert( Type toType, object value, IFormatProvider provider = null )
        {
            if( value is null )
            {
                return toType.IsValueType
                           ? throw new InvalidOperationException()
                           : System.Convert.ChangeType( null, toType );
            }

            if( !this.CanConvert( value.GetType(), toType ) )
                throw new InvalidCastException( $"No type converter for {toType.FullName}" );

            var converter = this._converters.First( x => x.CanConvert( value.GetType(), toType ) );
            return converter.Convert( toType, value, provider );
        }

        public bool CanConvert( Type fromType, Type toType )
        {
            Ensure.That( fromType, nameof( fromType ) ).IsNotNull();
            Ensure.That( toType, nameof( toType ) ).IsNotNull();

            return this._converters.Any( x => x.CanConvert( fromType, toType ) );
        }
    }
}
