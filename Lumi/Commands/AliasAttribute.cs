using System;

namespace Lumi.Commands
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true )]
    internal sealed class AliasAttribute : Attribute
    {
        public string Alias { get; }

        public AliasAttribute( string alias )
            => this.Alias = alias;
    }
}
