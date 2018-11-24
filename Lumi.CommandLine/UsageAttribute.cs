using System;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class UsageAttribute : Attribute
    {
        public string Usage { get; }

        public UsageAttribute( string usage )
            => this.Usage = usage;
    }
}