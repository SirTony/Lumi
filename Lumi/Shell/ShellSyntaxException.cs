using System;

namespace Lumi.Shell
{
    internal sealed class ShellSyntaxException : Exception
    {
        public int Start { get; }
        public int End { get; }

        public ShellSyntaxException( string message, int start, int end )
            : base( message )
        {
            this.Start = start;
            this.End = end;
        }
    }
}
