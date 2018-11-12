using System;

namespace Lumi.Shell
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true )]
    internal sealed class CommandAliasAttribute : Attribute
    {
        public string Alias { get; }

        public CommandAliasAttribute( string alias )
            => this.Alias = alias;
    }
}
