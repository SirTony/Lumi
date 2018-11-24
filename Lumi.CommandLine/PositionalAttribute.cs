using System;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter )]
    public sealed class PositionalAttribute : Attribute
    {
        public int Position { get; }

        public PositionalAttribute( int position )
            => this.Position = position;
    }
}