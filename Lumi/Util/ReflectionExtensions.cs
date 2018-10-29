using System.Linq;

namespace System.Reflection
{
    internal static class ReflectionExtensions
    {
        private const BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public static object GetValueFromPropertyPath( this object source, string path, Predicate<PropertyInfo> validator = null )
        {
            var (instance, prop) = source.GetPropertyFromPath( path, validator );
            return instance == null || prop == null ? null : prop.GetGetMethod()?.Invoke( instance, null );
        }

        public static (object, PropertyInfo) GetPropertyFromPath( this object source, string path, Predicate<PropertyInfo> validator = null )
        {
            var type = source.GetType();
            var instance = source;

            if( !path.Contains( '.' ) )
                return Validate( type.GetProperty( path, _flags ) );

            var parts = path.Split( '.' );
            foreach( var (i, name) in parts.Select( ( x, i ) => (i, x) ) )
            {
                if( i == parts.Length - 1 )
                    return Validate( type.GetProperty( name, _flags ) );

                var (_, prop) = Validate( type.GetProperty( name, _flags ) );
                var getter = prop?.GetGetMethod();

                if( prop == null || getter == null )
                    break;

                instance = getter.Invoke( instance, null );
                type = instance.GetType();
            }

            return (null, null);

            (object, PropertyInfo) Validate( PropertyInfo info )
                => instance != null && info != null && validator( info ) ? (instance, info) : (null, null);
        }
    }
}
