using System;
using PlanetGeneratorDll.AMath;

namespace PlanetGeneratorDll.Projections
{
    public class MercatorProjection
    {
        private readonly double __Lat;
        private readonly double __Lng;

        private readonly int __Width;
        private readonly int __height;

        public MercatorProjection(double lng, double lat, int w, int h)
        {
            __Lng = lng;
            __Lat = lat;

            __Width = w;
            __height = h;
        }

        public APoint3D ToPoint3D(APoint p)
        {
            if (p.X < 0 || p.Y < 0)
            {
                
            }
            var y = Math.Sin(__Lat);
            y = (1.0 + y) / (1.0 - y);
            y = 0.5 * Math.Log(y);
            var k = (int)(0.5 * y * __Width / Math.PI);

            y = Math.PI * (2.0 * (p.Y - k) - __height) / __Width;
            y = Math.Exp(2 * y);
            y = (y - 1) / (y + 1);
            var cos2 = Math.Sqrt(1.0 - y * y);

            var theta1 = __Lng - 0.5 * Math.PI + Math.PI * (2.0 * p.X - __Width) / __Width;

            return new APoint3D(Math.Cos(theta1) * cos2, y, -Math.Sin(theta1) * cos2);
        }

        public APoint ToPoint(APoint3D p)
        {
            var cos2 = Math.Sqrt(1.0 - p.Y * p.Y);
            var theta1 = -Math.Acos(p.X / cos2);

            if (p.Z > 0)
                theta1 += Math.PI;

            if (p.Z <= 0 && p.X > 0)
                theta1 -= Math.PI * 2;

            var cx = (((theta1 - 0.5 * Math.PI) * __Width) / Math.PI + __Width) / 2.0;

            if (p.Z <= 0 && p.X > 0)
                cx = Math.Abs(cx);

            double ly = 0.0;
            ly = (1.0 + ly) / (1.0 - ly);
            ly = 0.5 * Math.Log(ly);

            var k = (int)(0.5 * ly * __Width / Math.PI);

            ly = Math.Abs((p.Y + 1) / (p.Y - 1));
            ly = Math.Abs(Math.Log(ly) / 2);

            if (p.Y > 0)
                ly *= -1;

            var cy = __Width - (ly * __Width / Math.PI + __height) / 2.0 + k;
            return APoint.Round(Math.Abs(cx), cy);
        }
    }
}
