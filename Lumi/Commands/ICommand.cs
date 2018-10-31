using Lumi.Shell;

namespace Lumi.Commands
{
    internal interface ICommand
    {
        string Name { get; }

        ShellResult Execute();
    }
}
