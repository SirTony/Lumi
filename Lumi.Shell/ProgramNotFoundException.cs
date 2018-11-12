using System;

namespace Lumi.Shell
{
    public sealed class ProgramNotFoundException : Exception
    {
        public string ProgramName { get; }

        public ProgramNotFoundException( string programName, Exception inner )
            : base( "Program not found", inner ) => this.ProgramName = programName;
    }
}
