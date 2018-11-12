using System;
using Lumi.Parsing;

namespace Lumi.Shell.Parsing
{
    public sealed class ShellSyntaxException : SyntaxException
    {
        public ShellSyntaxException( string message, TextSpan span, Exception innerException = null )
            : base( message, span, innerException )
        {
        }
    }
}
