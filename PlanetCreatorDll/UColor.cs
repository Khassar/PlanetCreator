using System;
using System.Runtime.InteropServices;
using PlanetGeneratorDll.Flags;
using PlanetGeneratorDll.Helpers;

namespace PlanetGeneratorDll
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UColor
    {
        [FieldOffset(3)]
        public byte A;
        [FieldOffset(2)]
        public byte R;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(0)]
        public byte B;

        [FieldOffset(0)]
        public uint ARGB;

        #region HSV

        // range [0-1)
        public double HSV_H
        {
            get
            {
                var min = GetMin();
                var max = GetMax();

                if (min == max)
                    return 0;

                var d = max - min;

                const double c6 = 1 / 6.0;

                if (max == R)
                {
                    if (G >= B)
                        return (G - B) * c6 / d;

                    return (G - B) * c6 / d + 1;
                }

                if (max == G)
                    return (B - R) * c6 / d + 1 / 3.0;

                //max == B
                return (R - G) * c6 / d + 2 / 3.0;
            }

            set
            {
                var h = value;

                if (h < 0)
                    h = Math.Abs(1 - h);

                if (h >= 1)
                    h -= (int)h;

                var hI = (byte)(h * 6);

                const double c6 = 1 / 6.0;

                var v = HSV_V;
                var vMin = (byte)(Math.Round((1 - HSV_S) * v));
                var a = (v - vMin) * (h % c6) / c6;

                var vInc = Math.Round(vMin + a);
                var vDec = Math.Round(v - a);
                switch (hI)
                {
                    case 0:
                        R = (byte)v;
                        G = (byte)vInc;
                        B = vMin;
                        break;
                    case 1:
                        R = (byte)vDec;
                        G = (byte)v;
                        B = vMin;
                        break;
                    case 2:
                        R = vMin;
                        G = (byte)v;
                        B = (byte)vInc;
                        break;
                    case 3:
                        R = vMin;
                        G = (byte)vDec;
                        B = (byte)v;
                        break;
                    case 4:
                        R = (byte)vInc;
                        G = vMin;
                        B = (byte)v;
                        break;
                    case 5:
                        R = (byte)v;
                        G = vMin;
                        B = (byte)vDec;
                        break;
                }
            }
        }

        // range 0 - 1
        public double HSV_S
        {
            get { return 1 - (double)GetMin() / GetMax(); }
        }

        // range 0 - 255
        public double HSV_V
        {
            get { return GetMax(); }
        }

        #endregion

        #region colors

        public static UColor White = new UColor(0xFFFFFFFF);
        public static UColor Black = new UColor(0xFF000000);
        public static UColor Transparent = new UColor(0);

        public static UColor Red = new UColor(0xFFFF0000);
        public static UColor Green = new UColor(0xFF00FF00);
        public static UColor Blue = new UColor(0xFF0000FF);

        #endregion

        #region Constructors

        public UColor(byte r, byte g, byte b)
            : this()
        {
            R = r;
            G = g;
            B = b;

            A = 0xFF;
        }

        public UColor(byte a, byte r, byte g, byte b)
            : this()
        {
            R = r;
            G = g;
            B = b;

            A = a;
        }

        public UColor(uint argb)
            : this()
        {
            ARGB = argb;
        }

        #endregion

        public float Rf
        {
            get { return R / 255f; }
        }

        public float Gf
        {
            get { return G / 255f; }
        }

        public float Bf
        {
            get { return B / 255f; }
        }

        #region UI

        public string A_UI
        {
            get { return A.ToString("X2"); }
        }

        public string R_UI
        {
            get { return R.ToString("X2"); }
        }

        public string G_UI
        {
            get { return R.ToString("X2"); }
        }

        public string B_UI
        {
            get { return B.ToString("X2"); }
        }

        #endregion


        public static UColor RandomRgb()
        {
            var i = (uint)RandomHelper.NextMaxInt();

            var c = new UColor(i) { A = 0xff };
            return c;
        }

        public static UColor Merge(UColor c1, UColor c2, int p1, int p2)
        {
            var d = p1 + p2;
            var r = (c1.R * p1 + c2.R * p2) / d;
            var g = (c1.G * p1 + c2.G * p2) / d;
            var b = (c1.B * p1 + c2.B * p2) / d;
            var a = (c1.A * p1 + c2.A * p2) / d;

            return new UColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public static UColor Merge(UColor c1, UColor c2, float minAlt, float maxAlt, float alt)
        {
            return Merge(c1, c2, (alt - minAlt) / (maxAlt - minAlt));
        }

        public static UColor Merge(UColor c1, UColor c2, float percent)
        {
            var r = c1.R * percent + c2.R * (1 - percent);
            var g = c1.G * percent + c2.G * (1 - percent);
            var b = c1.B * percent + c2.B * (1 - percent);
            var a = c1.A * percent + c2.A * (1 - percent);

            return new UColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public UColor Invert()
        {
            return new UColor(0xFFFFFFFF - ARGB);
        }

        public static UColor Merge(UColor c1, UColor c2, int p1, int p2, UColorFlags flags)
        {
            var d = p1 + p2;

            var r = (flags & UColorFlags.R) == UColorFlags.R ? (c1.R * p1 + c2.R * p2) / d : c1.R;
            var g = (flags & UColorFlags.G) == UColorFlags.G ? (c1.G * p1 + c2.G * p2) / d : c1.G;
            var b = (flags & UColorFlags.B) == UColorFlags.B ? (c1.B * p1 + c2.B * p2) / d : c1.B;
            var a = (flags & UColorFlags.A) == UColorFlags.A ? (c1.A * p1 + c2.A * p2) / d : c1.A;

            return new UColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public bool Equals(UColor color)
        {
            return ARGB == color.ARGB;
        }

        public override string ToString()
        {
            return "A:" + A.ToString("X2") + " RGB:" + R + "." + G + "." + B;
        }

        public static bool operator ==(UColor c1, UColor c2)
        {
            return c1.ARGB == c2.ARGB;
        }

        public static bool operator !=(UColor c1, UColor c2)
        {
            return c1.ARGB != c2.ARGB;
        }

        private byte GetMax()
        {
            if (R >= B && R >= G)
                return R;
            if (G >= B && G >= R)
                return G;
            if (B >= R && B >= G)
                return B;

            return R;
        }

        private byte GetMin()
        {
            if (R <= B && R <= G)
                return R;
            if (G <= B && G <= R)
                return G;
            if (B <= R && B <= G)
                return B;

            return R;
        }
    }
}
