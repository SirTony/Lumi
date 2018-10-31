using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PowerArgs;

namespace Lumi
{
    internal class CustomHelpHook : ArgHook
    {
        public string CommandName { get; }
        public Version Version { get; }
        public bool SuppressBanner { get; }

        public CustomHelpHook( string commandName = null, bool suppressBanner = false )
        {
            this.SuppressBanner = suppressBanner;
            this.AfterCancelPriority = 0;
            this.CommandName = String.IsNullOrWhiteSpace( commandName )
                                   ? Path.GetFileName( Program.ExecutablePath )
                                   : commandName;

            var versionStr = Program.Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                          ?? Program.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

            this.Version = versionStr is null ? new Version() : Version.Parse( versionStr );
        }

        public override void AfterCancel( HookContext context )
        {
            base.AfterCancel( context );

            var sb = new StringBuilder();

            this.BuildBanner( sb );
            this.BuildUsageLine( sb, context );
            CustomHelpHook.BuildArgumentsList( sb, context );

            Console.Error.WriteLine( sb.ToString() );
        }

        private void BuildBanner( StringBuilder sb )
        {
            if( this.SuppressBanner )
                return;

            sb.AppendFormat(
                "{0} {1} - {2}",
                Program.Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title,
                $"{this.Version.Major}.{this.Version.Minor}",
                Program.Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description
            );

            sb.AppendLine()
              .AppendLine(
                   Program.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright.Replace( "©", "(c)" )
               )
              .AppendLine();
        }

        private void BuildUsageLine( StringBuilder sb, HookContext context )
        {
            const int ChunkSize = 3;

            var usagePrefix = $"USAGE: {this.CommandName} ";
            sb.Append( usagePrefix );

            if( context.Definition.HasActions )
                sb.AppendLine( "<action> [options...]" );
            else if( context.Definition.HasGlobalUsageArguments )
            {
                var line = context.Definition.Arguments.Select( GetArgumentUsage ).ToArray();

                if( line.Length <= ChunkSize )
                    sb.AppendLine( line.Join( " " ) );
                else
                {
                    var indent = new string( ' ', usagePrefix.Length );
                    var chunks = line.InChunksOf( ChunkSize ).ToArray();
                    var first = chunks.First().Join( " " );
                    var rest = chunks.Skip( 1 ).Select( x => $"{indent}{x.Join( " " )}" ).Join( Environment.NewLine );

                    sb.AppendLine( first ).AppendLine( rest );
                }
            }

            sb.AppendLine();

            string GetArgumentUsage( CommandLineArgument arg )
            {
                var flag = $"-{arg.DefaultAlias}";
                var defaultStringValue = arg.HasDefaultValue && arg.DefaultValue is string s
                                             ? String.IsNullOrWhiteSpace( s )
                                                   ? "<null>"
                                                   : s
                                             : arg.DefaultValue?.ToString() ?? "<null>";

                var def = arg.HasDefaultValue ? $"={defaultStringValue}" : String.Empty;

                return arg.IsRequired ? $"{flag}{def}" : $"[{flag}{def}]";
            }
        }

        private static void BuildArgumentsList( StringBuilder sb, HookContext context )
        {
            const string DescriptionSeparator = " :: ";

            if( !context.Definition.HasGlobalUsageArguments )
                return;

            sb.AppendLine( $"  {( context.Definition.HasActions ? "Global " : String.Empty )}Arguments" );

            var longestName = context.Definition
                                     .Arguments
                                     .Where( x => x.IncludeInUsage && x.DefaultAlias != null )
                                     .Max( x => x.DefaultAlias.Length );

            foreach( var arg in context.Definition.Arguments )
            {
                var firstAlias = arg.Aliases?.FirstOrDefault( x => x.Length == 1 );

                var temp = new StringBuilder()
                          .Append( "    " )
                          .Append( firstAlias is null ? "    " : $"-{firstAlias}, " )
                          .AppendFormat(
                               $"{{0,-{longestName + 1}}}",
                               arg.DefaultAlias is null ? new string( ' ', longestName + 1 ) : $"-{arg.DefaultAlias}"
                           );

                if( !String.IsNullOrWhiteSpace( arg.Description ) )
                {
                    var desc = WordWrap(
                        $"[{arg.ArgumentType.Name}{( arg.IsRequired ? ", Required" : String.Empty )}] {arg.Description}",
                        temp.Length + DescriptionSeparator.Length + 4
                    );

                    temp.Append( desc );
                }

                sb.AppendLine( temp.ToString() );
            }

            string WordWrap( string text, int startIndex )
            {
                var width = Console.BufferWidth - startIndex;
                if( text.Length < width )
                    return $"{DescriptionSeparator}{text}";

                var lines = text.InChunksOf( width ).Select( x => x.Trim() );
                var indent = new string( ' ', startIndex - 4 );

                return $"{DescriptionSeparator}{lines.First()}{Environment.NewLine}"
                     + $"{lines.Skip( 1 ).Select( x => $"{indent}{x}" ).Join( Environment.NewLine )}";
            }
        }

        public override void AfterPopulateProperty( HookContext context )
        {
            base.AfterPopulateProperty( context );
            if( !( context.CurrentArgument.RevivedValue is bool x && x ) )
                return;

            context.CancelAllProcessing();
        }

        public override void BeforePopulateProperty( HookContext context )
        {
            base.BeforePopulateProperty( context );
            if( context.CurrentArgument.ArgumentType != typeof( bool ) )
            {
                throw new InvalidArgDefinitionException(
                    typeof( CustomHelpHook ).Name + " attributes can only be used with bool properties or parameters"
                );
            }
        }
    }
}
