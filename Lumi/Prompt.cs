using System.Text.RegularExpressions;
using Console = Colorful.Console;

namespace Lumi
{
    internal static class Prompt
    {
        private static readonly Regex _yesNoRegex;

        static Prompt()
        {
            _yesNoRegex = new Regex( "^y|yes?|no?$", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        }

        public static bool YesNo( string message, bool defaultValue )
        {
            var y = defaultValue ? "Y" : "y";
            var n = defaultValue ? "n" : "N";

            Console.Write( $"{message} [{y}/{n}] " );
            var answer = Console.ReadLine().Trim().ToLowerInvariant();

            return _yesNoRegex.IsMatch( answer ) && answer[0] == 'y' ? true : defaultValue;
        }
    }
}
