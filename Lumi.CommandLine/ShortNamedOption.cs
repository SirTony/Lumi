namespace Lumi.CommandLine
{
    internal sealed class ShortNamedOption : CommandLineOption
    {
        public char Name { get; }
        public string Bundle { get; }
        public bool WasBundled => this.Bundle != null;

        public ShortNamedOption( char name, string bundle, string value ) : base( value )
        {
            this.Name = name;
            this.Bundle = bundle;
        }
    }
}
