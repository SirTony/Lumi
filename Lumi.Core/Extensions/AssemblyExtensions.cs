using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        public static bool Has<T>( this Assembly assembly )
            where T : Attribute
            => assembly.IsDefined( typeof( T ) );

        public static T Get<T>( this Assembly assembly )
            where T : Attribute
            => assembly.GetCustomAttribute<T>();
    }
}
