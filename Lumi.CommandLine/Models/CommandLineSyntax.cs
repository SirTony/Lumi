using System;
using System.Collections.Generic;
using EnsureThat;

namespace Lumi.CommandLine.Models
{
    internal sealed class CommandLineSyntax : CommandLineModel
    {
        public HashSet<CommandModel> Commands { get; }
        public Type ApplicationType { get; }

        public CommandLineSyntax( Type applicationType )
        {
            this.Commands = new HashSet<CommandModel>();
            this.ApplicationType = applicationType;
        }

        public CommandModel Command( string text )
        {
            Ensure.That( text, nameof( text ) ).IsNotNullOrWhiteSpace();
            var builder = new CommandModel( text, 1 );

            if( !this.Commands.Add( builder ) )
                throw new InvalidOperationException( $"Duplicate command '{text}'" );

            return builder;
        }
    }
}
