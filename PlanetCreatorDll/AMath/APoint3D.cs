using System;

namespace PlanetGeneratorDll.AMath
{
    public struct APoint3D
    {
        public double X;
        public double Y;
        public double Z;

        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        public double Lng
        {
            get { return Math.Atan(Y / X); }
        }

        public double Lat
        {
            get { return Math.Acos(Z / Length); }
        }

        #region constructors

        public APoint3D(double x, double y, double z)
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
            var len = Math.Sqrt(X * X + Y * Y + Z * Z);
            X /= len;
            Y /= len;
            Z /= len;
        }

        #endregion

        #region Operators

        public static APoint3D operator *(APoint3D p, double len)
        {
            return new APoint3D(p.X * len, p.Y * len, p.Z * len);
        }

        public static APoint3D operator /(APoint3D p, double value)
        {
            return p * (1 / value);
        }

        public static APoint3D operator +(APoint3D p1, APoint3D p2)
        {
            return new APoint3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static APoint3D operator -(APoint3D p1, APoint3D p2)
        {
            return new APoint3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        #endregion

        #region public static methods

        public static APoint3D MiddlePoint(APoint3D p1, APoint3D p2)
        {
            return (p1 + p2) * 0.5f;
        }

        public static APoint3D FromAngles(double lng, double lat)
        {
            var x = Math.Sin(lat) * Math.Cos(lng);
            var y = Math.Sin(lat) * Math.Sin(lng);
            var z = Math.Cos(lat);

            return new APoint3D(x, y, z);
        }

        #endregion

    }
}
