using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lumi.CommandLine
{
    internal sealed class PropertyWrapper
    {
        public PropertyInfo Property { get; }
        public bool Handled { get; set; }

        public PropertyWrapper( PropertyInfo info, bool handled )
        {
            this.Property = info;
            this.Handled = handled;
        }

        public override string ToString()
        {
            var named = this.Property.Get<NamedAttribute>();
            if( named != null )
            {
                var flags = new List<string>();
                if( named.HasShortName ) flags.Add( $"-{named.ShortName}" );
                if( named.HasLongName ) flags.Add( $"--{named.LongName}" );

                return flags.Join( "/" );
            }

            var positional = this.Property.Get<PositionalAttribute>();
            if( positional != null ) return $"at position {positional.Position}";

            throw new NotSupportedException(); // shouldnt happen
        }
    }
}
