namespace Lumi.CommandLine.Tests
{
    internal sealed class EmptyApplication
    {
        public int TestVariable { get; }

        public EmptyApplication() : this( 5 ) { }

        public EmptyApplication( int value )
            => this.TestVariable = value;
    }
}
