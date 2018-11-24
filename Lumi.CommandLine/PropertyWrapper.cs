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
    }
}