namespace Lumi.Shell
{
    internal enum ShellTokenKind
    {
        Literal,
        Dollar,
        Hash,
        Semicolon,

        //Colon,

        LeftParen,
        RightParen,

        //LeftSquare,
        //RightSquare,
        LeftAngle,
        RightAngle,
        DoubleRightAngle,
        TripleRightAngle,
        Ampersand,
        Pipe,
        EndOfInput
    }
}
