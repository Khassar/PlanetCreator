using System;
using System.Globalization;

namespace PlanetGeneratorDll.AMath
{
    /// <summary>
    /// Point - Defaults to 0,0
    /// </summary>
    public struct APoint
    {
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public int X;
        public int Y;

        public APoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region Public Methods

        public static APoint operator +(APoint point, APoint vector)
        {
            return new APoint(point.X + vector.X, point.Y + vector.Y);
        }

        public static bool operator ==(APoint p1, APoint p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(APoint p1, APoint p2)
        {
            return !(p1 == p2);
        }

        public static APoint operator -(APoint point, APoint vector)
        {
            return new APoint(point.X - vector.X, point.Y - vector.Y);
        }

        public static APoint Round(double x, double y)
        {
            return new APoint((int)Math.Round(x), (int)Math.Round(y));
        }

        public bool Equals(APoint p)
        {
            return X == p.X && Y == p.Y;
        }

        public static implicit operator APointF(APoint p)
        {
            return new APointF(p.X, p.Y);
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
        }

        #endregion Public Methods
    }
}
