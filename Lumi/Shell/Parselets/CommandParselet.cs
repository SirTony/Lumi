using System.Collections.Generic;
using System.Linq;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parselets
{
    internal sealed class CommandParselet : ISegmentParselet
    {
        public IShellSegment Parse( ShellParser parser, IShellSegment parent, ShellToken token )
        {
            if( !parser.HasSegment() )
                return new CommandSegment( parent, token.Text );

            if( parent != null && parent is CommandSegment )
                return new TextSegment( parent, token.Text );

            parser.AddOverride( ShellTokenKind.Literal, new TextParselet() );

            var args = new List<IShellSegment>();
            while( parser.HasSegment() )
                args.Add( parser.Parse() );

            var command = new CommandSegment( parent, token.Text, args.ToArray() );
            command.Arguments.ForEach( x => x.Parent = command );

            parser.ClearSegmentOverrides();

            return command;
        }
    }
}
