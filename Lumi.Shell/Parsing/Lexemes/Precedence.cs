namespace Lumi.Shell.Parsing.Lexemes
{
    public enum Precedence
    {
        Invalid,
        Sequence,
        Pipe,
        Redirection,
        Command
    }
}
