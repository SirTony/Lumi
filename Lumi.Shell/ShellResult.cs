namespace Lumi.Shell
{
    public readonly ref struct ShellResult
    {
        public int ExitCode { get; }
        public object Value { get; }

        public ShellResult( int exitCode, object value )
        {
            this.ExitCode = exitCode;
            this.Value = value;
        }

        public static implicit operator bool( ShellResult result )
            => result.ExitCode == 0;

        public static ShellResult Ok( object value = null )
            => new ShellResult( 0, value );

        public static ShellResult Error( int code, object value = null )
            => new ShellResult( code, value );
    }
}
