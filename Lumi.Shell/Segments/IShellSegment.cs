using Lumi.Core;
using Lumi.Shell.Parsing.Visitors;

namespace Lumi.Shell.Segments
{
    public enum ShellSegmentKind
    {
        Unknown,
        Command,
        CommandInterpolation,
        Pipe,
        Redirection,
        Sequence,
        StringInterpolation,
        Text,
        Variable
    }

    public interface IShellSegment
    {
        ShellSegmentKind Kind { get; }

        ShellResult Execute( AppConfig config, object input = null, bool captureOutput = false );

        T Accept<T>( ISegmentVisitor<T> visitor );
        void Accept( ISegmentVisitor visitor );
    }
}
