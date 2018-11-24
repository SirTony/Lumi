using System;

namespace Lumi.CommandLine
{
    public sealed class CommandLineException : Exception
    {
        public CommandLineException( string message ) : base( message ) { }
    }
}