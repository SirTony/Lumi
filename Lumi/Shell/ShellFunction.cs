using Lumi.Shell.Segments;
using System;
using System.Collections.Generic;

namespace Lumi.Shell
{
    internal delegate ShellResult ShellFunction( IReadOnlyList<IShellSegment> segments );

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    internal sealed class ShellFunctionAttribute : Attribute
    {
        public string Name { get; }

        public ShellFunctionAttribute( string name = null )
        {
            this.Name = name;
        }
    }
}
