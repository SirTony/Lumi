using System;
using System.IO;
using System.Linq;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Visitors
{
    internal sealed class DebugPrintVisitor : ISegmentVisitor
    {
        private readonly IndentTextWriter _writer;

        public DebugPrintVisitor( TextWriter writer, int indentSize = 4 )
            => this._writer = new IndentTextWriter( writer, new string( ' ', indentSize ) );

        private void WriteParent( IShellSegment segment )
            => this._writer.WriteLine(
                segment.Parent == null
                    ? "Parent = <none>"
                    : $"Parent = {segment.Parent.GetType().Name.Replace( "Segment", "" )}"
            );

        public void Visit( CommandSegment segment )
        {
            this._writer.WriteLine( "Command" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            this._writer.WriteLine( $"Command = {ShellUtil.Escape( segment.Command )}" );

            if( segment.Arguments.Count == 0 )
                this._writer.WriteLine( "Arguments = <none>" );
            else
            {
                this._writer.WriteLine( "Arguments =" );
                this._writer.WriteLine( "[" );
                ++this._writer.Indent;
                segment.Arguments.ForEach( x => x.Accept( this ) );
                --this._writer.Indent;
                this._writer.WriteLine( "]" );
            }

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }

        public void Visit( InterpolationSegment segment )
        {
            this._writer.WriteLine( "Interpolation" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            segment.Segment.Accept( this );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }

        public void Visit( PipeSegment segment )
        {
            this._writer.WriteLine( "Pipe" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );

            this._writer.Write( "Left = " );
            segment.Left.Accept( this );

            this._writer.Write( "Right = " );
            segment.Right.Accept( this );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }

        public void Visit( RedirectionSegment segment )
        {
            this._writer.WriteLine( "Redirection" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            this._writer.WriteLine( $"Redirection = {GetRedirectionName()}" );
            this._writer.WriteLine(
                $"{( segment.Mode == RedirectionSegment.RedirectionMode.StdIn ? "Source" : "Destination" )} = {segment.Redirection}"
            );
            this._writer.Write( "Left = " );
            segment.Left.Accept( this );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );

            string GetRedirectionName()
            {
                switch( segment.Mode )
                {
                    case RedirectionSegment.RedirectionMode.StdIn:
                        return "input";

                    case RedirectionSegment.RedirectionMode.StdOut:
                        return "output";

                    case RedirectionSegment.RedirectionMode.StdErr:
                        return "error";

                    case RedirectionSegment.RedirectionMode.StdOutAndErr:
                        return "output, error";

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void Visit( SequenceSegment segment )
        {
            this._writer.WriteLine( "Sequence" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            this._writer.Write( "Left = " );
            segment.Left.Accept( this );
            this._writer.Write( "Right = " );
            segment.Right.Accept( this );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }

        public void Visit( TextSegment segment )
        {
            this._writer.WriteLine( "Text" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            this._writer.WriteLine( $"Value = {ShellUtil.Escape( segment.Text )}" );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }

        public void Visit( VariableSegment segment )
        {
            this._writer.WriteLine( "Variable" );
            this._writer.WriteLine( "{" );
            ++this._writer.Indent;

            this.WriteParent( segment );
            this._writer.WriteLine( $"Scope = {segment.Scope ?? "<none>"}" );
            this._writer.WriteLine( $"Name = {segment.Name}" );

            --this._writer.Indent;
            this._writer.WriteLine( "}" );
        }
    }
}
