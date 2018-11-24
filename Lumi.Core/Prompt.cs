using System;
using System.Text.RegularExpressions;

namespace Lumi.Core
{
    public static class Prompt
    {
        private static readonly Regex YesNoRegex;

        static Prompt() => Prompt.YesNoRegex = new Regex(
                               "^y|yes?|no?$",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase
                           );

        public static bool YesNo( string message, bool defaultValue )
        {
            var y = defaultValue ? "Y" : "y";
            var n = defaultValue ? "n" : "N";

            Console.Write( $"{message} [{y}/{n}] " );
            var answer = Console.ReadLine()?.Trim()?.ToLowerInvariant() ?? "no";

            return Prompt.YesNoRegex.IsMatch( answer ) && answer[0] == 'y' || defaultValue;
        }
    }
}
