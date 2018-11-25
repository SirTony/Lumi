using System;

namespace Lumi.CommandLine
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter
    )]
    public sealed class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public DescriptionAttribute( string description )
            => this.Description = description;
    }
}
