using System;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter )]
    public sealed class RequiredAttribute : Attribute
    {
    }
}
