namespace Lumi.Shell
{
    public interface ICommand
    {
        string Name { get; }

        ShellResult Execute( object input );
    }
}
