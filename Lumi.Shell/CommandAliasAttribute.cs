using System;
using EnsureThat;

namespace Lumi.Shell
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true )]
    public sealed class CommandAliasAttribute : Attribute
    {
        public string Alias { get; }

        public CommandAliasAttribute( string alias )
        {
            Ensure.That( alias, nameof( alias ) ).IsNotNullOrWhiteSpace();
            this.Alias = alias;
        }
    }
}
