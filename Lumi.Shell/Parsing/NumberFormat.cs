using System;

namespace Lumi.Shell.Parsing
{
    [Flags]
    internal enum NumberFormat
    {
        None = 0,
        Int8 = 1 << 0,
        Int16 = 1 << 1,
        Int32 = 1 << 2,
        Int64 = 1 << 3,
        Float = 1 << 4,
        Double = 1 << 5,
        Decimal = 1 << 6,
        Binary = 1 << 7,
        Octal = 1 << 8,
        Hexadecimal = 1 << 9,

        // no 'Signed' because that's the default.
        Unsigned = 1 << 10
    }
}
