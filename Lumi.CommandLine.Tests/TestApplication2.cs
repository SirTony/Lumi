namespace Lumi.CommandLine.Tests
{
    internal sealed class TestApplication2
    {
        public string RequiredParameter { get; private set; }
        public string DefaultParameter { get; private set; }

        [Command( "with-code" )]
        public int WithExitCode( [Positional( 0 )] int code )
            => code;

        [Command( "with-required" )]
        public void WithRequiredParameter( [Named( 'r', "required" )] [Required] string value )
            => this.RequiredParameter = value;

        // default when no command is specified
        public int Main()
        {
            this.DefaultParameter = "Hello, World";
            return 100;
        }
    }
}
