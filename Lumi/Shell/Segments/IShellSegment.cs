using System.Collections.Generic;
using Lumi.Shell.Visitors;

namespace Lumi.Shell.Segments
{
    internal interface IShellSegment
    {
        IShellSegment Parent { get; set; }
        ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false );
        T Accept<T>( ISegmentVisitor<T> visitor );
        void Accept( ISegmentVisitor visitor );
    }
}
