namespace Lumi.CommandLine
{
    internal sealed class LongNamedOption : CommandLineOption
    {
        public string Name { get; }

        public LongNamedOption( string name, string value ) : base( value )
            => this.Name = name;
    }
}