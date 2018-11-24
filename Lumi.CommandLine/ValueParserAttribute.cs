using System;
using EnsureThat;

namespace Lumi.CommandLine
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter
    )]
    public sealed class ValueParserAttribute : Attribute
    {
        public Type ValueParserType { get; }

        public ValueParserAttribute( Type parserType )
        {
            Ensure.That( parserType, nameof( parserType ) ).IsNotNull();

            if( typeof( IValueParser ).IsAssignableFrom( parserType ) )
                throw new ArgumentException( "Given type is not a value parser", nameof( parserType ) );

            this.ValueParserType = parserType;
        }
    }
}