using System;

namespace PlanetGeneratorDll.AMath
{
    public struct APoint3F
    {
        public float X;
        public float Y;
        public float Z;

        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        public float Lng
        {
            get { return (float)Math.Atan(Y / X); }
        }

        public float Lat
        {
            get { return (float)Math.Acos(Z / Length); }
        }

        #region constructors

        public APoint3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region public methods

        public override string ToString()
        {
            return X + " : " + Y + " : " + Z;
        }

        public void Normalize()
        {
            var len = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            X /= len;
            Y /= len;
            Z /= len;
        }

        #endregion

        #region public static methods

        public static APoint3F MiddlePoint(APoint3F p1, APoint3F p2)
        {
            return (p1 + p2) * 0.5f;
        }

        public static APoint3F FromAngles(double lng, double lat)
        {
            var x = Math.Sin(lat) * Math.Cos(lng);
            var y = Math.Sin(lat) * Math.Sin(lng);
            var z = Math.Cos(lat);

            return new APoint3F((float)x, (float)y, (float)z);
        }

        #endregion

        #region Operators

        public static APoint3F operator *(APoint3F p, float len)
        {
            return new APoint3F(p.X * len, p.Y * len, p.Z * len);
        }

        public static APoint3F operator /(APoint3F p, float value)
        {
            return p * (1 / value);
        }

        public static APoint3F operator +(APoint3F p1, APoint3F p2)
        {
            return new APoint3F(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static APoint3F operator -(APoint3F p1, APoint3F p2)
        {
            return new APoint3F(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        #endregion

    }
}
