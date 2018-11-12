using System;

namespace Lumi.Parsing
{
    public class SyntaxException : Exception
    {
        public TextSpan Span { get; }

        public SyntaxException( string message, TextSpan span, Exception innerException = null )
            : base( message, innerException )
            => this.Span = span;
    }
}
