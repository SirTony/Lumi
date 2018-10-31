using System;
using System.Collections.Generic;
using Lumi.Shell.Segments;

namespace Lumi.Shell
{
    internal delegate ShellResult ShellFunction( IReadOnlyList<IShellSegment> segments );

    [AttributeUsage( AttributeTargets.Method )]
    internal sealed class ShellFunctionAttribute : Attribute
    {
        public string Name { get; }

        public ShellFunctionAttribute( string name = null ) => this.Name = name;
    }
}
