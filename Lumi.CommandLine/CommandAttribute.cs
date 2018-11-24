using System;
using EnsureThat;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }

        public CommandAttribute( string name )
        {
            // can still be null
            Ensure.That( name, nameof( name ) ).IsNotEmptyOrWhitespace();
            this.Name = name;
        }
    }
}