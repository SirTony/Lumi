using System.Collections.Generic;
using System.Linq;
using Lumi.Shell.Parselets;
using Lumi.Shell.Segments;

namespace Lumi.Shell
{
    internal sealed class ShellParser
    {
        private static readonly IReadOnlyDictionary<ShellTokenKind, ISegmentParselet> Segments;
        private static readonly IReadOnlyDictionary<ShellTokenKind, IInfixParselet> Infix;
        private static readonly IDictionary<ShellTokenKind, ISegmentParselet> SegmentOverrides;
        private static readonly IDictionary<ShellTokenKind, IInfixParselet> InfixOverrides;
        private readonly int _length;

        private readonly IReadOnlyList<ShellToken> _tokens;
        private int _index;

        private bool EndOfInput => this._index >= this._length;

        static ShellParser()
        {
            ShellParser.Segments = new Dictionary<ShellTokenKind, ISegmentParselet>
            {
                [ShellTokenKind.Literal] = new CommandParselet(),
                [ShellTokenKind.Octothorpe] = new InterpolationParselet(),
                [ShellTokenKind.Dollar] = new VariableParselet()
            };

            ShellParser.Infix = new Dictionary<ShellTokenKind, IInfixParselet>
            {
                [ShellTokenKind.RightAngle] = new RedirectionParselet( RedirectionSegment.RedirectionMode.StdOut ),
                [ShellTokenKind.DoubleRightAngle] =
                    new RedirectionParselet( RedirectionSegment.RedirectionMode.StdErr ),
                [ShellTokenKind.TripleRightAngle] =
                    new RedirectionParselet( RedirectionSegment.RedirectionMode.StdOutAndErr ),
                [ShellTokenKind.LeftAngle] = new RedirectionParselet( RedirectionSegment.RedirectionMode.StdIn ),
                [ShellTokenKind.Ampersand] = new SequenceParselet( true ),
                [ShellTokenKind.Semicolon] = new SequenceParselet( false ),
                [ShellTokenKind.Pipe] = new PipeParselet()
            };

            ShellParser.SegmentOverrides = new Dictionary<ShellTokenKind, ISegmentParselet>();
            ShellParser.InfixOverrides = new Dictionary<ShellTokenKind, IInfixParselet>();
        }

        public ShellParser( IEnumerable<ShellToken> tokens )
        {
            this._tokens = tokens.ToList();
            this._length = this._tokens.Count;
        }

        public bool HasSegment()
            => ShellParser.Segments.TryGetValue( this.Peek().Kind, out var _ );

        public IShellSegment ParseAll()
        {
            var token = this.Peek();
            if( token.Kind != ShellTokenKind.Literal && token.Kind != ShellTokenKind.Dollar )
            {
                throw new ShellSyntaxException(
                    "command line must start with a literal or variable",
                    token.Index,
                    token.Index
                );
            }

            var segment = this.Parse();

            //if( !this.IsNext( ShellTokenKind.EndOfInput ) )
            //{
            //    token = this.Peek();

            //    switch( token.Kind )
            //    {
            //        // special error message for unescaped URLs
            //        case ShellTokenKind.Colon:
            //            throw new ShellSyntaxException(
            //                $"unexpected colon. If it is part of a URL, please escape with quotes (e.g. \"http://example.com\")",
            //                token.Index,
            //                token.Index
            //            );
            //    }
            //}

            this.Take( ShellTokenKind.EndOfInput, token );
            return segment;
        }

        public void AddOverride( ShellTokenKind kind, ISegmentParselet parselet )
            => ShellParser.SegmentOverrides[kind] = parselet;

        public void AddOverride( ShellTokenKind kind, IInfixParselet parselet )
            => ShellParser.InfixOverrides[kind] = parselet;

        public void ClearSegmentOverrides()
            => ShellParser.SegmentOverrides.Clear();

        public void ClearInfixOverrides()
            => ShellParser.InfixOverrides.Clear();

        public void ClearAllOverrides()
        {
            this.ClearSegmentOverrides();
            this.ClearInfixOverrides();
        }

        public IShellSegment Parse( Precedence precedence = Precedence.Invalid )
        {
            var token = this.Take();
            if( !this.TryGetSegmentParselet( token.Kind, out var parselet ) )
            {
                throw new ShellSyntaxException(
                    $"unexpected '{token}' at position {token.Index}, expecting command",
                    token.Index,
                    token.Index
                );
            }

            var left = parselet.Parse( this, null, token );

            while( precedence < this.GetPrecedence( out var infix ) )
            {
                token = this.Take();
                left = infix.Parse( this, null, left, token );
            }

            return left;
        }

        public ShellToken Peek( int distance = 0 )
        {
            var newIndex = this._index + distance;
            return newIndex >= this._length || newIndex < 0 ? default : this._tokens[newIndex];
        }

        public bool IsNext( ShellTokenKind kind )
            => this.Peek().Kind == kind;

        public ShellToken Take()
        {
            if( this.EndOfInput )
                return this._tokens[this._length - 1];

            var token = this.Peek();
            ++this._index;
            return token;
        }

        // TODO: Supplying the start token solely for the exception is a total hack.
        //       There has to be a better way to do this, but for right now it works.
        public ShellToken Take( ShellTokenKind kind, ShellToken startToken )
        {
            var token = this.Take();
            if( token.Kind != kind )
            {
                throw new ShellSyntaxException(
                    $"unexpected '{token}', expecting {kind} at position {token.Index}",
                    startToken.Index,
                    token.Index
                );
            }

            return token;
        }

        private bool TryGetSegmentParselet( ShellTokenKind kind, out ISegmentParselet parselet )
            => ShellParser.SegmentOverrides.TryGetValue( kind, out parselet )
            || ShellParser.Segments.TryGetValue( kind, out parselet );

        private bool TryGetInfixParselet( ShellTokenKind kind, out IInfixParselet parselet )
            => ShellParser.InfixOverrides.TryGetValue( kind, out parselet )
            || ShellParser.Infix.TryGetValue( kind, out parselet );

        private Precedence GetPrecedence( out IInfixParselet infix )
            => this.TryGetInfixParselet( this.Peek().Kind, out infix ) ? infix.Precedence : 0;
    }
}
