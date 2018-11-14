using Lumi.Core;

namespace Lumi.Shell
{
    public interface ICommand
    {
        string Name { get; }

        ShellResult Execute( AppConfig config, object input );
    }
}
