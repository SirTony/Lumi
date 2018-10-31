namespace Lumi.Shell
{
    internal sealed class ShellToken
    {
        public ShellTokenKind Kind { get; }
        public string Text { get; }
        public int Index { get; }

        public ShellToken( ShellTokenKind kind, string text, int index )
        {
            this.Kind = kind;
            this.Text = text;
            this.Index = index;
        }

        public string ToString( bool verbose )
            => verbose
                   ? $"Token( Kind = {this.Kind}, Text = {( this.Text == null ? "null" : $"\"{this.Text}\"" )}, Index = {this.Index:#,#0} )"
                   : this.ToString();

        public override string ToString()
        {
            switch( this.Kind )
            {
                case ShellTokenKind.EndOfInput:
                    return "<end-of-input>";

                default:
                    return this.Text;
            }
        }
    }
}
