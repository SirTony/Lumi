namespace Lumi.Shell
{
    internal enum ShellTokenKind
    {
        Literal,

        Dollar,

        Octothorpe,

        Semicolon,

        Colon,

        LeftParen, RightParen,

        LeftAngle, RightAngle,

        DoubleRightAngle,
        TripleRightAngle,

        Ampersand,

        Pipe,

        EndOfInput,
    }
}
