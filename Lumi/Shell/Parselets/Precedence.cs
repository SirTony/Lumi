namespace Lumi.Shell.Parselets
{
    internal enum Precedence
    {
        Invalid,

        Sequence,
        Pipe,
        Redirection,
    }
}
