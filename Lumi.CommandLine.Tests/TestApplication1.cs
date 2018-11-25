namespace Lumi.CommandLine.Tests
{
    internal sealed class TestApplication1
    {
        [Named( 'o', "optional" )]
        public int OptionalValue { get; } = 5;

        [Named( 'r', "required" )]
        [Required]
        public string RequiredValue { get; private set; }
    }
}
