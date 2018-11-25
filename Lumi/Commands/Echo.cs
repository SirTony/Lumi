namespace Lumi.Commands
{
    // TODO: fix this and update it for the new command line parser
    //[SuppressMessage(
    //    "ReSharper",
    //    "UnusedAutoPropertyAccessor.Local",
    //    Justification = "Private setters are needed by PowerArgs"
    //)]
    //internal sealed class Echo : ICommand
    //{
    //    [ArgPosition( 0 )]
    //    public List<string> Text { get; private set; }

    //    [ArgIgnore]
    //    public string Name { get; } = "echo";

    //    public ShellResult Execute( AppConfig config, object input )
    //    {
    //        if( this.Text is null ) this.Text = new List<string>();

    //        switch( input )
    //        {
    //            case string s:
    //                this.Text.Add( s );
    //                break;

    //            case StandardStreams std when std.StandardOutput != null:
    //                this.Text.AddRange( std.StandardOutput );
    //                break;

    //            case IEnumerable<string> strings:
    //                this.Text.AddRange( strings );
    //                break;

    //            default:
    //                this.Text.Add( input.ToString() );
    //                break;
    //        }

    //        return ShellResult.Ok( this.Text.Join( " " ) );
    //    }
    //}
}
