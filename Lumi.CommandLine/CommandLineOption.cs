using System;

namespace Lumi.CommandLine
{
    internal abstract class CommandLineOption
    {
        public string Value { get; }

        public CommandLineOption( string value )
            => this.Value = String.IsNullOrWhiteSpace( value ) ? null : value;
    }
}