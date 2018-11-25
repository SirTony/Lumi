namespace System.Drawing
{
    public static class ColorExtensions
    {
        public static double CalculateLuminance( this Color c )
            => Math.Sqrt( Math.Pow( 0.299 * c.R, 2 ) + Math.Pow( 0.587 * c.G, 2 ) + Math.Pow( 0.114 * c.B, 2 ) );

        public static double CalculateContrastRatio( this Color a, Color b )
            => ( a.CalculateLuminance() + 0.05 ) / ( b.CalculateLuminance() + 0.05 );
    }
}
