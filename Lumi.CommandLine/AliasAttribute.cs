using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true )]
    public sealed class AliasAttribute : Attribute
    {
        public string Name { get; }

        public AliasAttribute( string name )
        {
            Ensure.That( name, nameof(name) ).IsNotNullOrWhiteSpace();
            this.Name = name;
        }
    }
}
