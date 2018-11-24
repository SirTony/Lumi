using System;
using System.Collections.Generic;
using System.Reflection;
using EnsureThat;

namespace Lumi.CommandLine.Models
{
    internal sealed class CommandModel : CommandLineModel, IEquatable<CommandModel>
    {
        public string Command { get; }
        public HashSet<string> Aliases { get; }
        public string DescriptionText { get; private set; }
        public Action CallbackFunc { get; private set; }

        public CommandModel( string command, int positionAdjust )
            : base( positionAdjust )
        {
            this.Command = command;
            this.Aliases = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
        }

        public CommandModel Alias( string alias )
        {
            Ensure.That( alias, nameof( alias ) ).IsNotNullOrWhiteSpace();

            if( !this.Aliases.Add( alias ) )
                throw new InvalidOperationException( $"Duplicate alias '{alias}'" );

            return this;
        }

        public CommandModel Description( string text )
        {
            this.DescriptionText = text;
            return this;
        }

        public CommandModel Callback( Action action )
        {
            this.CallbackFunc = action;
            return this;
        }

        public bool Equals( CommandModel other )
        {
            if( Object.ReferenceEquals( null, other ) ) return false;
            if( Object.ReferenceEquals( this, other ) ) return true;
            return String.Equals( this.Command, other.Command, StringComparison.OrdinalIgnoreCase )
                && this.Aliases.SetEquals( other.Aliases );
        }

        public override bool Equals( object obj )
        {
            if( Object.ReferenceEquals( null, obj ) ) return false;
            if( Object.ReferenceEquals( this, obj ) ) return true;
            return obj is CommandModel other && this.Equals( other );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ( this.Command != null ? this.Command.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( this.Aliases != null ? this.Aliases.GetHashCode() : 0 );
                return hashCode;
            }
        }
    }
}