using System;
using System.Collections.Generic;
using System.Linq;
using Lumi.Parsing;
using Lumi.Shell.Parsing.Lexemes;
using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing
{
    public sealed class ShellParser : ParserBase<ShellTokenKind, IShellSegment>
    {
        private static readonly IReadOnlyDictionary<ShellTokenKind, IPrimaryLexeme> PrimaryLexemes;
        private static readonly IReadOnlyDictionary<ShellTokenKind, IBinaryLexeme> BinaryLexemes;

        public bool CommandParsingDisabled { get; private set; }

        static ShellParser()
        {
            ShellParser.PrimaryLexemes = new Dictionary<ShellTokenKind, IPrimaryLexeme>
            {
                [ShellTokenKind.String] = new StringLexeme(), [ShellTokenKind.Dollar] = new DollarLexeme()
            };

            ShellParser.BinaryLexemes = new Dictionary<ShellTokenKind, IBinaryLexeme>
            {
                [ShellTokenKind.Ampersand] = new SequenceLexeme( true ),
                [ShellTokenKind.Semicolon] = new SequenceLexeme( false ),
                [ShellTokenKind.Pipe] = new PipeLexeme(),
                [ShellTokenKind.LeftAngle] = new RedirectionLexeme( RedirectionMode.StandardInput ),
                [ShellTokenKind.RightAngle] = new RedirectionLexeme( RedirectionMode.StandardOutput ),
                [ShellTokenKind.DoubleRightAngle] = new RedirectionLexeme( RedirectionMode.StandardError ),
                [ShellTokenKind.TripleRightAngle] = new RedirectionLexeme( RedirectionMode.StandardOutputAndError )
            };
        }

        public ShellParser( IEnumerable<SyntaxToken<ShellTokenKind>> tokens ) : base( tokens ) { }

        public override IShellSegment ParseAll()
        {
            this.Index = 0;
            var tree = this.Parse( Precedence.Invalid );
            this.Take( ShellTokenKind.EndOfInput );

            return tree;
        }

        public IShellSegment Parse( Precedence precedence = Precedence.Invalid )
        {
            var token = this.Take();
            if( !ShellParser.PrimaryLexemes.TryGetValue( token.Kind, out var primary ) )
                throw new ShellSyntaxException( $"Expecting shell segment, found {token.Kind}", token.Span );

            var left = primary.Parse( this, token );
            while( precedence < this.GetPrecedence( out var binary ) )
            {
                token = this.Take();
                left = binary.Parse( this, left, token );
            }

            return left;
        }

        public bool HasSegment()
            => ShellParser.PrimaryLexemes.ContainsKey( this.Peek().Kind );

        public T WithoutCommandParsing<T>( Func<T> fn )
        {
            var original = this.CommandParsingDisabled;
            this.CommandParsingDisabled = true;
            var value = fn();
            this.CommandParsingDisabled = original;

            return value;
        }

        public T WithCommandParsing<T>( Func<T> fn )
        {
            var original = this.CommandParsingDisabled;
            this.CommandParsingDisabled = false;
            var value = fn();
            this.CommandParsingDisabled = original;

            return value;
        }

        private IShellSegment ParseSpecificSegment( Precedence precedence, params Type[] acceptedTypes )
        {
            var token = this.Peek();
            var segment = this.Parse( precedence );

            if( acceptedTypes.Any( x => x.IsInstanceOfType( segment ) ) ) return segment;

            if( acceptedTypes.Length == 1 )
            {
                throw new ShellSyntaxException(
                    $"Unexpected {segment.GetType().Name}, expecting {acceptedTypes[0].Name}",
                    token.Span
                );
            }

            var names = acceptedTypes.Select( x => x.Name ).Join( ", " );
            throw new ShellSyntaxException(
                $"Unexpected {segment.GetType().Name}, expecting one of: {names}",
                token.Span
            );
        }

        private Precedence GetPrecedence( out IBinaryLexeme lexeme )
            => ShellParser.BinaryLexemes.TryGetValue( this.Peek().Kind, out lexeme )
                   ? lexeme.Precedence
                   : Precedence.Invalid;

        #region Auto-Generated

        public T1 Parse<T1>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            => (T1) this.ParseSpecificSegment( precedence, typeof( T1 ) );

        public IShellSegment Parse<T1, T2>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            => this.ParseSpecificSegment( precedence, typeof( T1 ), typeof( T2 ) );

        public IShellSegment Parse<T1, T2, T3>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            => this.ParseSpecificSegment( precedence, typeof( T1 ), typeof( T2 ), typeof( T3 ) );

        public IShellSegment Parse<T1, T2, T3, T4>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            => this.ParseSpecificSegment( precedence, typeof( T1 ), typeof( T2 ), typeof( T3 ), typeof( T4 ) );

        public IShellSegment Parse<T1, T2, T3, T4, T5>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9>( Precedence precedence = Precedence.Invalid )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            where T12 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 ),
                typeof( T12 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            where T12 : IShellSegment
            where T13 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 ),
                typeof( T12 ),
                typeof( T13 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            where T12 : IShellSegment
            where T13 : IShellSegment
            where T14 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 ),
                typeof( T12 ),
                typeof( T13 ),
                typeof( T14 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            where T12 : IShellSegment
            where T13 : IShellSegment
            where T14 : IShellSegment
            where T15 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 ),
                typeof( T12 ),
                typeof( T13 ),
                typeof( T14 ),
                typeof( T15 )
            );

        public IShellSegment Parse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Precedence precedence = Precedence.Invalid
        )
            where T1 : IShellSegment
            where T2 : IShellSegment
            where T3 : IShellSegment
            where T4 : IShellSegment
            where T5 : IShellSegment
            where T6 : IShellSegment
            where T7 : IShellSegment
            where T8 : IShellSegment
            where T9 : IShellSegment
            where T10 : IShellSegment
            where T11 : IShellSegment
            where T12 : IShellSegment
            where T13 : IShellSegment
            where T14 : IShellSegment
            where T15 : IShellSegment
            where T16 : IShellSegment
            => this.ParseSpecificSegment(
                precedence,
                typeof( T1 ),
                typeof( T2 ),
                typeof( T3 ),
                typeof( T4 ),
                typeof( T5 ),
                typeof( T6 ),
                typeof( T7 ),
                typeof( T8 ),
                typeof( T9 ),
                typeof( T10 ),
                typeof( T11 ),
                typeof( T12 ),
                typeof( T13 ),
                typeof( T14 ),
                typeof( T15 ),
                typeof( T16 )
            );

        #endregion
    }
}
