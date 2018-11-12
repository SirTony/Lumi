using System.Collections.Generic;

namespace Lumi.Shell
{
    public readonly struct StandardStreams
    {
        public IReadOnlyList<string> StandardOutput { get; }
        public IReadOnlyList<string> StandardError { get; }

        public StandardStreams( IReadOnlyList<string> output, IReadOnlyList<string> error )
        {
            this.StandardOutput = output;
            this.StandardError = error;
        }
    }
}
