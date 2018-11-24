using System;

namespace Lumi.CommandLine
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter )]
    public sealed class NamedAttribute : Attribute
    {
        public char ShortName { get; }
        public string LongName { get; }

        public NamedAttribute( char shortName )
            : this( shortName, null ) { }

        public NamedAttribute( string longName )
            : this( default, longName ) { }

        public NamedAttribute( char shortName, string longName )
        {
            this.ShortName = shortName;
            this.LongName = longName;
        }
    }
}