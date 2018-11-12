namespace Lumi.Shell.Parsing
{
    public enum ShellTokenKind
    {
        Invalid,
        String,
        StringInterpolation,
        Dollar,
        Semicolon,
        Ampersand,
        Pipe,
        LeftAngle,
        RightAngle,
        DoubleRightAngle,
        TripleRightAngle,
        LeftParen,
        RightParen,
        EndOfInput
    }
}
