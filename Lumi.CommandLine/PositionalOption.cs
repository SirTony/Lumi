namespace Lumi.CommandLine
{
    internal sealed class PositionalOption : CommandLineOption
    {
        public int Position { get; }

        public PositionalOption( int position, string value ) : base( value )
            => this.Position = position;
    }
}
