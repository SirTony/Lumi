using System;
using EnsureThat;

namespace Lumi.CommandLine.Models
{
    internal abstract class ArgumentModel
    {
        public bool IsRequired { get; private set; }
        public Type ValueType { get; private set; }
        public object DefaultValue { get; private set; }
        public bool HasDefaultValue { get; private set; }
        public Func<string, object> Parser { get; private set; }
        public Action<object> Assigner { get; private set; }
        public string DescriptionText { get; private set; }

        public ArgumentModel Required( bool required = false )
        {
            this.IsRequired = required;
            return this;
        }

        public ArgumentModel As( Type type )
        {
            Ensure.That( type, nameof( type ) ).IsNotNull();
            this.ValueType = type;
            return this;
        }

        public ArgumentModel Default( object defaultValue )
        {
            this.HasDefaultValue = true;
            this.DefaultValue = defaultValue;
            return this;
        }

        public ArgumentModel ValueParser( Func<string, object> parser )
        {
            this.Parser = parser ?? DefaultParser;
            return this;

            object DefaultParser( string x )
            {
                if( String.IsNullOrWhiteSpace( x ) && this.ValueType == typeof( bool ) )
                    return true;

                if( String.IsNullOrWhiteSpace( x ) && !this.HasDefaultValue )
                    throw new CommandLineException( $"Missing required value for argument {this}" );

                if( this.ValueType == typeof( string ) )
                    return String.IsNullOrWhiteSpace( x ) ? this.DefaultValue : x;

                return this.ValueType.IsEnum
                           ? String.IsNullOrWhiteSpace( x )
                                 ? this.DefaultValue
                                 : Enum.Parse( this.ValueType, x, true )
                           : String.IsNullOrWhiteSpace( x )
                               ? this.DefaultValue
                               : Convert.ChangeType( x, this.ValueType );
            }
        }

        internal ArgumentModel ValueAssigner( Action<object> assigner )
        {
            this.Assigner = assigner;
            return this;
        }

        public ArgumentModel Description( string text )
        {
            this.DescriptionText = text;
            return this;
        }
    }
}
