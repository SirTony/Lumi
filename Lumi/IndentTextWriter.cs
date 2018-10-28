using System.IO;
using System.Text;

namespace Lumi
{
    // Borrowed from https://raw.githubusercontent.com/Rohansi/Mond/master/Mond/IndentTextWriter.cs
    // License: https://raw.githubusercontent.com/Rohansi/Mond/master/LICENSE
    internal sealed class IndentTextWriter : TextWriter
    {
        private readonly TextWriter _writer;
        private readonly string _indentStr;
        private bool _shouldIndent;

        public override Encoding Encoding => this._writer.Encoding;

        public int Indent { get; set; }

        public IndentTextWriter( TextWriter writer, string indentStr = "  " )
        {
            this._writer = writer;
            this._indentStr = indentStr;
            this._shouldIndent = false;
        }

        public override void Write( char value )
        {
            if( this._shouldIndent )
            {
                this._shouldIndent = false; // shouldIndent must be cleared first
                this.WriteIndent();
            }

            this._writer.Write( value );
        }

        public override void WriteLine()
        {
            base.WriteLine();

            this._shouldIndent = true; // defer indenting until the next Write
        }

        public override void WriteLine( string value )
        {
            this.Write( value );
            this.WriteLine();
        }

        private void WriteIndent()
        {
            for( var i = 0; i < this.Indent; i++ )
                this.Write( this._indentStr );
        }
    }
}
