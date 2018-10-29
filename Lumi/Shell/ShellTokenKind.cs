namespace Lumi.Shell
{
    internal enum ShellTokenKind
    {
        Literal,

        Dollar,

        Octothorpe,

        Semicolon,

        //Colon,

        LeftParen, RightParen,

        LeftSquare, RightSquare,

        LeftAngle, RightAngle,

        DoubleRightAngle,
        TripleRightAngle,

        Ampersand,

        Pipe,

        EndOfInput,
    }
}
