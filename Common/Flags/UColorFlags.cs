using System;

namespace Common.Flags
{
    [Flags]
    public enum UColorFlags
    {
        A = 8,
        R = 4,
        G = 2,
        B = 1,

        Argb = A | R | G | B
    }
}
