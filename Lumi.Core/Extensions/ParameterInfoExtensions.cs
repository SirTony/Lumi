namespace System.Reflection
{
    public static class ParameterInfoExtensions
    {
        public static bool Has<T>( this ParameterInfo parameter, bool inherit = true ) where T : Attribute
            => parameter.IsDefined( typeof( T ), inherit );

        public static T Get<T>( this ParameterInfo parameter, bool inherit = true ) where T : Attribute
            => parameter.GetCustomAttribute<T>( inherit );
    }
}
