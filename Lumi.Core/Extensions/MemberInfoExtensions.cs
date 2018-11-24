using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class MemberInfoExtensions
    {
        public static bool Has<T>( this MemberInfo memberInfo, bool inherit = true )
            where T : Attribute
            => memberInfo.IsDefined( typeof( T ), inherit );

        public static T Get<T>( this MemberInfo memberInfo, bool inherit = true )
            where T : Attribute
            => memberInfo.GetCustomAttribute<T>( inherit );
    }
}
