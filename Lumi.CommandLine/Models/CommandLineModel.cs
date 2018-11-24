using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace Lumi.CommandLine.Models
{
    internal abstract class CommandLineModel
    {
        public HashSet<PositionalArgumentModel> PositionalArguments { get; }
        public HashSet<NamedArgumentModel> NamedArguments { get; }
        private readonly int _positionAdjust;

        protected internal CommandLineModel( int positionAdjust = 0 )
        {
            this.PositionalArguments = new HashSet<PositionalArgumentModel>();
            this.NamedArguments = new HashSet<NamedArgumentModel>();
            this._positionAdjust = positionAdjust;
        }

        public ArgumentModel Positional( int position )
        {
            Ensure.That( position ).IsGte( 0 );
            var builder = new PositionalArgumentModel( position, this._positionAdjust );

            if( !this.PositionalArguments.Add( builder ) )
                throw new InvalidOperationException( $"Duplicate positional argument at position {position}" );

            return builder;
        }

        public ArgumentModel Named( char shortName )
        {
            CommandLineModel.ValidateShortName( shortName, false );
            return this.Named( shortName, null );
        }

        public ArgumentModel Named( string longName )
        {
            CommandLineModel.ValidateLongName( longName, false );
            return this.Named( default, longName );
        }

        public ArgumentModel Named( char shortName, string longName )
        {
            CommandLineModel.ValidateShortName( shortName, true );
            CommandLineModel.ValidateLongName( longName, true );
            var builder = new NamedArgumentModel( shortName, longName );

            if( !this.NamedArguments.Add( builder ) )
                throw new InvalidOperationException( $"Duplicate named argument {builder}" );

            return builder;
        }

        private static void ValidateShortName( char shortName, bool allowDefault )
        {
            if( !allowDefault )
                Ensure.That( shortName, nameof( shortName ) ).IsNotDefault();

            Ensure.That( shortName, nameof( shortName ) ).IsNot( '-' );
            if( Char.IsControl( shortName ) )
                throw new ArgumentException( "Short name must be a printable character", nameof( shortName ) );
        }

        private static void ValidateLongName( string longName, bool allowNull )
        {
            if( allowNull )
                Ensure.That( longName, nameof( longName ) ).IsNotEmptyOrWhitespace();
            else
                Ensure.That( longName, nameof( longName ) ).IsNotNullOrWhiteSpace();

            if( longName.All( x => x == '-' ) )
                throw new ArgumentException( "Long name cannot contain '-'", nameof( longName ) );

            if( longName.Any( Char.IsControl ) )
                throw new ArgumentException( "Long name must contain only printable characters", nameof( longName ) );
        }
    }
}
