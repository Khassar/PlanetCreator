using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using Common.AMath;
using Common.Flags;
using PlanetGeneratorDll.Algorithms;
using PlanetGeneratorDll.Enums;
using PlanetGeneratorDll.Models;

namespace PlanetGeneratorDll
{
    internal class PlanetGeneratorNative
    {
        public Action<float> OnProgressChange;
        public Action<UColor[,]> OnFinish;

        #region private fields

        private int __LayersCount;
        private int __CurrentLayerIndex;

        private int __GenerationWidth;
        private int __GenerationHeigth;

        private CancellationToken __CancellationToken;

        private AlgorithmType __AlgorithmType;
        private IPlanetAlgorithm __PlanetAlgorithm;
        private ShadeType __ShadeType;

        private ColorContainer __ColorContainer;

        #region base

        private const double PI = Math.PI;
        private const double DEG2_RAD = Math.PI / 180.0;

        private double __Lng;
        private double __Lat;

        int Width = 800, Height = 600;

        private byte[,] __Shades; /* shade array */

        double __CLat, __SLat, __CLng, __SLng;

        private APoint __StartGenPos;
        private APoint __EndGenPos;

        private UColor[,] __TmpImage;

        #endregion

        #endregion

        #region constructors

        public PlanetGeneratorNative(AlgorithmType algorithmType)
        {
            __AlgorithmType = algorithmType;
        }

        #endregion

        #region public methods

        public UBitmap Generate(PlanetContainer settings, CancellationToken token)
        {
            var pObject = settings.Container2D;

            return Generate(
                settings.Shema,
                settings.Algorithm,
                token,
                0,
                0,
                pObject.Width,
                pObject.Height,
                pObject.Width,
                pObject.Height,
                settings.Seeds,
                pObject.Projection,
                pObject.SeaLevel,
                pObject.Lng,
                pObject.Lat
                );
        }

        public UBitmap Generate(PlanetContainer settings, CancellationToken token, int x, int y, int w, int h)
        {
            var pObject = settings.Container2D;

            return Generate(
                settings.Shema,
                settings.Algorithm,
                token,
                x,
                y,
                w,
                h,
                pObject.Width,
                pObject.Height,
                settings.Seeds,
                pObject.Projection,
                pObject.SeaLevel,
                pObject.Lng,
                pObject.Lat);
        }

        #endregion

        #region private methods

        private UBitmap Generate(
            Shema shema,
            AlgorithmType algorithmType,
            CancellationToken token,
            int x,
            int y,
            int w,
            int h,
            int width,
            int height,
            List<double> seeds,
            ProjectionType projection,
            double seaLevel = 0,
            double lng = 0,
            double lat = 0
            )
        {
            __AlgorithmType = algorithmType;

            UColor[,] resultBitmap = null;

            shema.SortByLevel();

            __LayersCount = shema.Layers.Count(l => l.IsEnable);
            __CurrentLayerIndex = 0;

            if (token.IsCancellationRequested)
                return null;

            for (int index = 0; index < shema.Layers.Count; index++)
            {
                if (token.IsCancellationRequested)
                    return null;

                var layer = shema.Layers[index];
                if (!layer.IsEnable)
                    continue;

                double seed = 0;

                if (__CurrentLayerIndex < seeds.Count)
                    seed = seeds[__CurrentLayerIndex];
                else
                {
                    seed = seeds[seeds.Count - 1];

                    for (int i = seeds.Count; i <= __CurrentLayerIndex; i++)
                        seed = rand2(seed, seed);
                }

                __ShadeType = layer.Shade;
                __PlanetAlgorithm = GetAlgorithm(__AlgorithmType, seed, __ShadeType);
                var range = __PlanetAlgorithm.GetAltRange();
                __ColorContainer = new ColorContainer(layer, range);

                var layerBmp = MainGen(token, x, y, w, h, width, height, seed, projection, seaLevel, lng, lat);

                if (token.IsCancellationRequested)
                    return null;

                if (resultBitmap == null)
                    resultBitmap = layerBmp;
                else
                {
                    for (int _x = 0; _x < w; _x++)
                    {
                        for (int _y = 0; _y < h; _y++)
                        {
                            var nc = layerBmp[_x, _y];
                            resultBitmap[_x, _y] = UColor.Merge(resultBitmap[_x, _y], nc, 255 - nc.A, nc.A, UColorFlags.R | UColorFlags.G | UColorFlags.B);
                        }
                    }
                }

                __CurrentLayerIndex++;
            }

            return new UBitmap(resultBitmap);
        }

        private IPlanetAlgorithm GetAlgorithm(AlgorithmType type, double seed, ShadeType shadeType)
        {
            IPlanetAlgorithm planetAlgorithm = null;
            switch (type)
            {
                case AlgorithmType.Classic:
                    planetAlgorithm = new GClassic();
                    planetAlgorithm.Initialize(seed, shadeType);
                    break;
                default:
                    throw new NotSupportedException(__AlgorithmType.ToString());
            }

            return planetAlgorithm;
        }

        private UColor[,] MainGen(
            CancellationToken token, // token for canceling
            int x, int y, int w, int h, // local generation
            int width, int height, // Generation original size
            double seed,
            ProjectionType projection,
            double seaLevel = 0,
            double lng = 0, double lat = 0
            )
        {
            __TmpImage = new UColor[w, h];

            __StartGenPos = new APoint(x, y);
            __EndGenPos = new APoint(x + w, y + h);

            __CancellationToken = token;

            __GenerationWidth = w;
            __GenerationHeigth = h;

            __Shades = new byte[w, h];

            Width = width;
            Height = height;

            __Lng = lng;
            __Lat = lat;

            if (__Lng > 180)
                __Lng -= 360;
            __Lng = __Lng * DEG2_RAD;
            __Lat = __Lat * DEG2_RAD;

            __SLat = Math.Sin(__Lat);
            __CLat = Math.Cos(__Lat);
            __SLng = Math.Sin(__Lng);
            __CLng = Math.Cos(__Lng);

            switch (projection)
            {
                case ProjectionType.Mercator:
                    Mercator();
                    break;
                case ProjectionType.Peter:
                    Peter();
                    break;
                case ProjectionType.Square:
                    Squarep();
                    break;
                //case ProjectionType.Mollweide:
                //    mollweide();
                //    break;
                //case ProjectionType.Sinusoid:
                //    Sinusoid();
                //    break;
                case ProjectionType.Stereo:
                    Stereo();
                    break;
                case ProjectionType.Orthographic:
                    Orthographic();
                    break;
                //case ProjectionType.Gnomonic:
                //    Gnomonic();
                //    break;
                //case ProjectionType.Icosahedral:
                //    Icosahedral();
                //    break;
                //case ProjectionType.Azimuth:
                //    Azimuth();
                //    break;
                //case ProjectionType.Conical:
                //    Conical();
                //    break;
                //case ProjectionType.Heightfield:
                //    Heightfield();
                //    break;
                default:
                    throw new NotImplementedException(projection.ToString());
            }

            if (__CancellationToken.IsCancellationRequested)
                return null;

            if (__ShadeType != ShadeType.None)
            {
                SmoothShades();

                for (int _y = 0; _y < __GenerationHeigth; _y++)
                {
                    for (int _x = 0; _x < __GenerationWidth; _x++)
                    {
                        var s = __Shades[_x, _y];
                        var uc = __TmpImage[_x, _y];
                        if (s != 255)
                        {
                            __TmpImage[_x, _y] = ReturnFuckngColor(
                                uc.A,
                                (s * uc.R / 150),
                                (s * uc.G / 150),
                                (s * uc.B / 150)
                                );
                        }
                    }
                }
            }

            return __TmpImage;
        }

        private UColor ReturnFuckngColor(int a, int r, int g, int b)
        {
            if (a > 255)
                a = 255;
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            return new UColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        private void CallProgress(int iter)
        {
            var maxIter = __EndGenPos.Y - __StartGenPos.Y;
            var cur = iter - __StartGenPos.Y;

            if (OnProgressChange == null)
                return;

            float progress = cur * 100f;
            progress /= maxIter;
            progress /= __LayersCount;

            progress += (100.0f / __LayersCount) * __CurrentLayerIndex;

            OnProgressChange(progress);
        }

        private void SmoothShades()
        {
            var newShade = new byte[__GenerationWidth, __GenerationHeigth];

            for (int _x = 0; _x < __GenerationWidth; _x++)
                for (int _y = 0; _y < __GenerationHeigth; _y++)
                {
                    int d = 0;
                    int s = 0;

                    s += __Shades[_x, _y] * 4;
                    d += 4;

                    if (_x > 0)
                    {
                        s += __Shades[_x - 1, _y] * 2;
                        d += 2;
                    }

                    if (_x < __GenerationWidth - 1)
                    {
                        s += __Shades[_x + 1, _y] * 2;
                        d += 2;
                    }

                    if (_y > 0)
                    {
                        s += __Shades[_x, _y - 1] * 2;
                        d += 2;
                    }

                    if (_y < __GenerationHeigth - 1)
                    {
                        s += __Shades[_x, _y + 1] * 2;
                        d += 2;
                    }

                    newShade[_x, _y] = (byte)(s / d);
                }

            __Shades = newShade;
        }

        #region math

        private double log_2(double x)
        {
            return Math.Log(x) / Math.Log(2);
        }

        private double rand2(double p, double q)
        {
            double r = (p + 3.14159265) * (q + 3.14159265);
            return (2.0 * (r - (int)r) - 1.0);
        }

        #endregion

        #region projection

        private void SetNone(int x, int y)
        {
            __TmpImage[x - __StartGenPos.X, y - __StartGenPos.Y] = new UColor(0);
            if (__ShadeType != ShadeType.None)
                __Shades[x - __StartGenPos.X, y - __StartGenPos.Y] = 255;
        }

        private void Mercator()
        {
            var y = Math.Sin(__Lat);
            y = (1.0 + y) / (1.0 - y);
            y = 0.5 * Math.Log(y);
            var k = (int)(0.5 * y * Width / PI);
            int depth;
            for (int _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
            {
                if (__CancellationToken.IsCancellationRequested)
                    return;

                CallProgress(_y);

                y = PI * (2.0 * (_y - k) - Height) / Width;
                y = Math.Exp(2 * y);
                y = (y - 1) / (y + 1);
                double scale1 = Width / (double)Height / Math.Sqrt(1.0 - y * y) / PI;
                double cos2 = Math.Sqrt(1.0 - y * y);

                depth = 3 * ((int)(log_2(scale1 * Height))) + 3;

                for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                {
                    double theta1 = __Lng - 0.5 * PI + PI * (2.0 * _x - Width) / Width;
                    planet0(Math.Cos(theta1) * cos2, y, -Math.Sin(theta1) * cos2, _x, _y, depth);
                }
            }
        }

        void Peter()
        {
            var y = 2.0 * Math.Sin(__Lat);
            var k = (int)(0.5 * y * Width * PI);
            for (int _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
            {
                if (__CancellationToken.IsCancellationRequested)
                    return;

                CallProgress(_y);

                y = 0.5 * PI * (2.0 * (_y - k) - Height) / Width;
                if (Math.Abs(y) > 1.0)
                    for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                        SetNone(_x, _y);
                else
                {
                    double cos2 = Math.Sqrt(1.0 - y * y);
                    if (cos2 > 0.0)
                    {
                        double scale1 = Width / (double)Height / cos2 / PI;
                        var Depth = 3 * ((int)(log_2(scale1 * Height))) + 3;
                        for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                        {
                            double theta1 = __Lng - 0.5 * PI + PI * (2.0 * _x - Width) / Width;
                            planet0(Math.Cos(theta1) * cos2, y, -Math.Sin(theta1) * cos2, _x, _y, Depth);
                        }
                    }
                }
            }
        }

        void Squarep()
        {
            var k = (int)(0.5 * __Lat * Width / PI);
            for (int _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
            {
                CallProgress(_y);

                double y = (2.0 * (_y - k) - Height) / Width * PI;
                if (Math.Abs(y) >= 0.5 * PI)
                    for (int _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                        SetNone(_x, _y);
                else
                {
                    double cos2 = Math.Cos(y);
                    if (cos2 > 0.0)
                    {
                        double scale1 = Width / (double)Height / cos2 / PI;
                        var Depth = 3 * ((int)(log_2(scale1 * Height))) + 3;
                        for (int _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                        {
                            double theta1 = __Lng - 0.5 * PI + PI * (2.0 * _x - Width) / Width;
                            planet0(Math.Cos(theta1) * cos2, Math.Sin(y), -Math.Sin(theta1) * cos2, _x, _y, Depth);
                        }
                    }
                }
            }
        }

        //void mollweide()
        //{
        //    double y, y1, zz, scale1, cos2, theta1, theta2;
        //    int i, j, i1 = 1, k;

        //    for (j = 0; j < Height; j++)
        //    {
        //        //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //        y1 = 2 * (2.0 * j - Height) / Width;
        //        if (Math.Abs(y1) >= 1.0) for (i = 0; i < Width; i++)
        //            {
        //                col[i, j] = BACK;
        //                if (doshade > 0) __Shades[i, j] = 255;
        //            }
        //        else
        //        {
        //            zz = Math.Sqrt(1.0 - y1 * y1);
        //            y = 2.0 / PI * (y1 * zz + Math.Asin(y1));
        //            cos2 = Math.Sqrt(1.0 - y * y);
        //            if (cos2 > 0.0)
        //            {
        //                scale1 = Width / Height / cos2 / PI;
        //                Depth = 3 * ((int)(log_2(scale1 * Height))) + 3;
        //                for (i = 0; i < Width; i++)
        //                {
        //                    theta1 = PI / zz * (2.0 * i - Width) / Width;
        //                    if (Math.Abs(theta1) > PI)
        //                    {
        //                        col[i, j] = BACK;
        //                        if (doshade > 0) __Shades[i, j] = 255;
        //                    }
        //                    else
        //                    {
        //                        double x2, y2, z2, x3, y3, z3;
        //                        theta1 += -0.5 * PI;
        //                        x2 = Math.Cos(theta1) * cos2;
        //                        y2 = y;
        //                        z2 = -Math.Sin(theta1) * cos2;
        //                        x3 = cLng * x2 + sLng * sLat * y2 + sLng * cLat * z2;
        //                        y3 = cLat * y2 - sLat * z2;
        //                        z3 = -sLng * x2 + cLng * sLat * y2 + cLng * cLat * z2;

        //                        planet0(x3, y3, z3, i, j);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //void Sinusoid()
        //{
        //    double y, theta1, theta2, cos2, l1, i1, scale1;
        //    int k, i, j, l, c;

        //    k = (int)(__Lat * Width / PI);
        //    for (j = 0; j < Height; j++)
        //    {
        //        //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //        y = (2.0 * (j - k) - Height) / Width * PI;
        //        if (Math.Abs(y) >= 0.5 * PI) for (i = 0; i < Width; i++)
        //            {
        //                col[i, j] = BACK;
        //                if (doshade > 0) __Shades[i, j] = 255;
        //            }
        //        else
        //        {
        //            cos2 = Math.Cos(y);
        //            if (cos2 > 0.0)
        //            {
        //                scale1 = Width / Height / cos2 / PI;
        //                Depth = 3 * ((int)(log_2(scale1 * Height))) + 3;
        //                for (i = 0; i < Width; i++)
        //                {
        //                    l = i * 12 / Width;
        //                    l1 = l * Width / 12.0;
        //                    i1 = i - l1;
        //                    theta2 = __Lng - 0.5 * PI + PI * (2.0 * l1 - Width) / Width;
        //                    theta1 = (PI * (2.0 * i1 - Width / 12) / Width) / cos2;
        //                    if (Math.Abs(theta1) > PI / 12.0)
        //                    {
        //                        col[i, j] = BACK;
        //                        if (doshade > 0) __Shades[i, j] = 255;
        //                    }
        //                    else
        //                    {
        //                        planet0(Math.Cos(theta1 + theta2) * cos2, Math.Sin(y), -Math.Sin(theta1 + theta2) * cos2,
        //                          i, j);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        void Stereo()
        {
            for (var _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
            {
                for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                {
                    var x = (2.0 * _x - Width) / Height;
                    var y = (2.0 * _y - Height) / Height;
                    var z = x * x + y * y;
                    var zz = 0.25 * (4.0 + z);
                    x = x / zz;
                    y = y / zz;
                    z = (1.0 - 0.25 * z) / zz;

                    var x1 = __CLng * x + __SLng * __SLat * y + __SLng * __CLat * z;
                    var y1 = __CLat * y - __SLat * z;
                    var z1 = -__SLng * x + __CLng * __SLat * y + __CLng * __CLat * z;

                    var Depth = 3 * ((int)(log_2(Height) / (1.0 + x1 * x1 + y1 * y1))) + 6;
                    planet0(x1, y1, z1, _x, _y, Depth);
                }
            }
        }

        private void Orthographic()
        {
            double ymin = -2.0;
            double ymax = 2.0;

            var depth = 3 * ((int)(log_2(Height))) + 6;

            for (var _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
            {
                if (__CancellationToken.IsCancellationRequested)
                    return;

                CallProgress(_y);

                for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
                {
                    double x = (2.0 * _x - Width) / Height;
                    double y = (2.0 * _y - Height) / Height;
                    if (x * x + y * y > 1.0)
                    {
                        __TmpImage[_x - __StartGenPos.X, _y - __StartGenPos.Y] = new UColor(0);
                        if (__ShadeType != ShadeType.None)
                            __Shades[_x - __StartGenPos.X, _y - __StartGenPos.Y] = 255;
                    }
                    else
                    {
                        double z = Math.Sqrt(1.0 - x * x - y * y);
                        double x1 = __CLng * x + __SLng * __SLat * y + __SLng * __CLat * z;
                        double y1 = __CLat * y - __SLat * z;
                        double z1 = -__SLng * x + __CLng * __SLat * y + __CLng * __CLat * z;
                        if (y1 < ymin)
                            ymin = y1;
                        if (y1 > ymax)
                            ymax = y1;

                        planet0(x1, y1, z1, _x, _y, depth);
                    }
                }
            }
        }

        //void Icosahedral() /* modified version of gnomonic */
        //{
        //    double x, y, z, x1, y1, z1, zz, theta1, theta2, ymin, ymax;
        //    int i, j;
        //    double lat1, longi1, sla, cla, slo, clo, x0, y0, sq3_4, sq3;
        //    double L1, L2, S;

        //    ymin = 2.0;
        //    ymax = -2.0;
        //    sq3 = Math.Sqrt(3.0);
        //    L1 = 10.812317;
        //    L2 = -52.622632;
        //    S = 55.6;
        //    for (j = 0; j < Height; j++)
        //    {
        //        //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //        for (i = 0; i < Width; i++)
        //        {

        //            x0 = 198.0 * (2.0 * i - Width) / Width - 36;
        //            y0 = 198.0 * (2.0 * j - Height) / Width - __Lat / DEG2_RAD;

        //            longi1 = 0.0;
        //            lat1 = 500.0;
        //            if (y0 / sq3 <= 18.0 && y0 / sq3 >= -18.0)
        //            { /* middle row of triangles */
        //                /* upward triangles */
        //                if (x0 - y0 / sq3 < 144.0 && x0 + y0 / sq3 >= 108.0)
        //                {
        //                    lat1 = -L1;
        //                    longi1 = 126.0;
        //                }
        //                else if (x0 - y0 / sq3 < 72.0 && x0 + y0 / sq3 >= 36.0)
        //                {
        //                    lat1 = -L1;
        //                    longi1 = 54.0;
        //                }
        //                else if (x0 - y0 / sq3 < 0.0 && x0 + y0 / sq3 >= -36.0)
        //                {
        //                    lat1 = -L1;
        //                    longi1 = -18.0;
        //                }
        //                else if (x0 - y0 / sq3 < -72.0 && x0 + y0 / sq3 >= -108.0)
        //                {
        //                    lat1 = -L1;
        //                    longi1 = -90.0;
        //                }
        //                else if (x0 - y0 / sq3 < -144.0 && x0 + y0 / sq3 >= -180.0)
        //                {
        //                    lat1 = -L1;
        //                    longi1 = -162.0;
        //                }

        //                /* downward triangles */
        //                else if (x0 + y0 / sq3 < 108.0 && x0 - y0 / sq3 >= 72.0)
        //                {
        //                    lat1 = L1;
        //                    longi1 = 90.0;
        //                }
        //                else if (x0 + y0 / sq3 < 36.0 && x0 - y0 / sq3 >= 0.0)
        //                {
        //                    lat1 = L1;
        //                    longi1 = 18.0;
        //                }
        //                else if (x0 + y0 / sq3 < -36.0 && x0 - y0 / sq3 >= -72.0)
        //                {
        //                    lat1 = L1;
        //                    longi1 = -54.0;
        //                }
        //                else if (x0 + y0 / sq3 < -108.0 && x0 - y0 / sq3 >= -144.0)
        //                {
        //                    lat1 = L1;
        //                    longi1 = -126.0;
        //                }
        //                else if (x0 + y0 / sq3 < -180.0 && x0 - y0 / sq3 >= -216.0)
        //                {
        //                    lat1 = L1;
        //                    longi1 = -198.0;
        //                }
        //            }

        //            if (y0 / sq3 > 18.0)
        //            { /* bottom row of triangles */
        //                if (x0 + y0 / sq3 < 180.0 && x0 - y0 / sq3 >= 72.0)
        //                {
        //                    lat1 = L2;
        //                    longi1 = 126.0;
        //                }
        //                else if (x0 + y0 / sq3 < 108.0 && x0 - y0 / sq3 >= 0.0)
        //                {
        //                    lat1 = L2;
        //                    longi1 = 54.0;
        //                }
        //                else if (x0 + y0 / sq3 < 36.0 && x0 - y0 / sq3 >= -72.0)
        //                {
        //                    lat1 = L2;
        //                    longi1 = -18.0;
        //                }
        //                else if (x0 + y0 / sq3 < -36.0 && x0 - y0 / sq3 >= -144.0)
        //                {
        //                    lat1 = L2;
        //                    longi1 = -90.0;
        //                }
        //                else if (x0 + y0 / sq3 < -108.0 && x0 - y0 / sq3 >= -216.0)
        //                {
        //                    lat1 = L2;
        //                    longi1 = -162.0;
        //                }
        //            }
        //            if (y0 / sq3 < -18.0)
        //            { /* top row of triangles */
        //                if (x0 - y0 / sq3 < 144.0 && x0 + y0 / sq3 >= 36.0)
        //                {
        //                    lat1 = -L2;
        //                    longi1 = 90.0;
        //                }
        //                else if (x0 - y0 / sq3 < 72.0 && x0 + y0 / sq3 >= -36.0)
        //                {
        //                    lat1 = -L2;
        //                    longi1 = 18.0;
        //                }
        //                else if (x0 - y0 / sq3 < 0.0 && x0 + y0 / sq3 >= -108.0)
        //                {
        //                    lat1 = -L2;
        //                    longi1 = -54.0;
        //                }
        //                else if (x0 - y0 / sq3 < -72.0 && x0 + y0 / sq3 >= -180.0)
        //                {
        //                    lat1 = -L2;
        //                    longi1 = -126.0;
        //                }
        //                else if (x0 - y0 / sq3 < -144.0 && x0 + y0 / sq3 >= -252.0)
        //                {
        //                    lat1 = -L2;
        //                    longi1 = -198.0;
        //                }
        //            }

        //            if (lat1 > 400.0)
        //            {
        //                col[i, j] = BACK;
        //                if (doshade > 0) __Shades[i, j] = 255;
        //            }
        //            else
        //            {
        //                x = (x0 - longi1) / S;
        //                y = (y0 + lat1) / S;

        //                longi1 = longi1 * DEG2_RAD - __Lng;
        //                lat1 = lat1 * DEG2_RAD;

        //                sla = Math.Sin(lat1); cla = Math.Cos(lat1);
        //                slo = Math.Sin(longi1); clo = Math.Cos(longi1);


        //                zz = Math.Sqrt(1.0 / (1.0 + x * x + y * y));
        //                x = x * zz;
        //                y = y * zz;
        //                z = Math.Sqrt(1.0 - x * x - y * y);
        //                x1 = clo * x + slo * sla * y + slo * cla * z;
        //                y1 = cla * y - sla * z;
        //                z1 = -slo * x + clo * sla * y + clo * cla * z;

        //                /*
        //                if (y0/sq3 < 18.05 && y0/sq3 > 17.95  
        //                    && x0+y0/sq3 > -18.05 && x0+y0/sq3 < -17.95)
        //                  fprintf(stderr, "%lf, %lf: %lf, %lf, %lf\n",x0+y0/sq3, y0/sq3, x1,y1,z1);
        //                */
        //                if (y1 < ymin) ymin = y1;
        //                if (y1 > ymax) ymax = y1;
        //                planet0(x1, y1, z1, i, j);
        //            }
        //        }
        //    }
        //}

        //void Gnomonic()
        //{
        //    double x, y, z, x1, y1, z1, zz, theta1, theta2, ymin, ymax;
        //    int i, j;

        //    ymin = 2.0;
        //    ymax = -2.0;
        //    for (j = 0; j < Height; j++)
        //    {
        //        //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //        for (i = 0; i < Width; i++)
        //        {
        //            x = (2.0 * i - Width) / Height;
        //            y = (2.0 * j - Height) / Height;
        //            zz = Math.Sqrt(1.0 / (1.0 + x * x + y * y));
        //            x = x * zz;
        //            y = y * zz;
        //            z = Math.Sqrt(1.0 - x * x - y * y);
        //            x1 = cLng * x + sLng * sLat * y + sLng * cLat * z;
        //            y1 = cLat * y - sLat * z;
        //            z1 = -sLng * x + cLng * sLat * y + cLng * cLat * z;
        //            if (y1 < ymin) ymin = y1;
        //            if (y1 > ymax) ymax = y1;
        //            planet0(x1, y1, z1, i, j);
        //        }
        //    }
        //}

        //void Azimuth()
        //{
        //    double x, y, z, x1, y1, z1, zz, theta1, theta2, ymin, ymax;

        //    ymin = 2.0;
        //    ymax = -2.0;
        //    for (var _y = __StartGenPos.Y; _y < __EndGenPos.Y; _y++)
        //    {
        //        for (var _x = __StartGenPos.X; _x < __EndGenPos.X; _x++)
        //        {
        //            x = (2.0 * _x - Width) / Height;
        //            y = (2.0 * _x - Height) / Height;
        //            zz = x * x + y * y;
        //            z = 1.0 - 0.5 * zz;
        //            if (z < -1.0)
        //                SetNone(_x,_y);
        //            else
        //            {
        //                zz = Math.Sqrt(1.0 - 0.25 * zz);
        //                x = x * zz;
        //                y = y * zz;
        //                x1 = cLng * x + sLng * sLat * y + sLng * cLat * z;
        //                y1 = cLat * y - sLat * z;
        //                z1 = -sLng * x + cLng * sLat * y + cLng * cLat * z;
        //                if (y1 < ymin) ymin = y1;
        //                if (y1 > ymax) ymax = y1;
        //                planet0(x1, y1, z1, _x, _x);
        //            }
        //        }
        //    }
        //}

        //void Conical()
        //{
        //    double k1, c, y2, x, y, zz, x1, y1, z1, theta1, theta2, ymin, ymax, cos2;
        //    int i, j;

        //    ymin = 2.0;
        //    ymax = -2.0;
        //    if (__Lat > 0)
        //    {
        //        k1 = 1.0 / Math.Sin(__Lat);
        //        c = k1 * k1;
        //        y2 = Math.Sqrt(c * (1.0 - Math.Sin(__Lat / k1)) / (1.0 + Math.Sin(__Lat / k1)));
        //        for (j = 0; j < Height; j++)
        //        {
        //            //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //            for (i = 0; i < Width; i++)
        //            {
        //                x = (2.0 * i - Width) / Height;
        //                y = (2.0 * j - Height) / Height + y2;
        //                zz = x * x + y * y;
        //                if (zz == 0.0) theta1 = 0.0; else theta1 = k1 * Math.Atan2(x, y);
        //                if (theta1 < -PI || theta1 > PI)
        //                {
        //                    col[i, j] = BACK;
        //                    if (doshade > 0) __Shades[i, j] = 255;
        //                }
        //                else
        //                {
        //                    theta1 += __Lng - 0.5 * PI; /* theta1 is longitude */
        //                    theta2 = k1 * Math.Asin((zz - c) / (zz + c));
        //                    /* theta2 is latitude */
        //                    if (theta2 > 0.5 * PI || theta2 < -0.5 * PI)
        //                    {
        //                        col[i, j] = BACK;
        //                        if (doshade > 0) __Shades[i, j] = 255;
        //                    }
        //                    else
        //                    {
        //                        cos2 = Math.Cos(theta2);
        //                        y = Math.Sin(theta2);
        //                        if (y < ymin) ymin = y;
        //                        if (y > ymax) ymax = y;
        //                        planet0(Math.Cos(theta1) * cos2, y, -Math.Sin(theta1) * cos2, i, j);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    else
        //    {
        //        k1 = 1.0 / Math.Sin(__Lat);
        //        c = k1 * k1;
        //        y2 = Math.Sqrt(c * (1.0 - Math.Sin(__Lat / k1)) / (1.0 + Math.Sin(__Lat / k1)));
        //        for (j = 0; j < Height; j++)
        //        {
        //            //if (debug && ((j % (Height/25)) == 0)) {fprintf (stderr, "%c", view); fflush(stderr);}
        //            for (i = 0; i < Width; i++)
        //            {
        //                x = (2.0 * i - Width) / Height;
        //                y = (2.0 * j - Height) / Height - y2;
        //                zz = x * x + y * y;
        //                if (zz == 0.0) theta1 = 0.0; else theta1 = -k1 * Math.Atan2(x, -y);
        //                if (theta1 < -PI || theta1 > PI)
        //                {
        //                    col[i, j] = BACK;
        //                    if (doshade > 0) __Shades[i, j] = 255;
        //                }
        //                else
        //                {
        //                    theta1 += __Lng - 0.5 * PI; /* theta1 is longitude */
        //                    theta2 = k1 * Math.Asin((zz - c) / (zz + c));
        //                    /* theta2 is latitude */
        //                    if (theta2 > 0.5 * PI || theta2 < -0.5 * PI)
        //                    {
        //                        col[i, j] = BACK;
        //                        if (doshade > 0) __Shades[i, j] = 255;
        //                    }
        //                    else
        //                    {
        //                        cos2 = Math.Cos(theta2);
        //                        y = Math.Sin(theta2);
        //                        if (y < ymin) ymin = y;
        //                        if (y > ymax) ymax = y;
        //                        planet0(Math.Cos(theta1) * cos2, y, -Math.Sin(theta1) * cos2, i, j);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //void Heightfield()
        //{
        //    double x, y, z, x1, y1, z1;
        //    int i, j;

        //    for (j = 0; j < Height; j++)
        //    {
        //        for (i = 0; i < Width; i++)
        //        {
        //            x = (2.0 * i - Width) / Height;
        //            y = (2.0 * j - Height) / Height;
        //            if (x * x + y * y > 1.0) heights[i, j] = 0;
        //            else
        //            {
        //                z = Math.Sqrt(1.0 - x * x - y * y);
        //                x1 = cLng * x + sLng * sLat * y + sLng * cLat * z;
        //                y1 = cLat * y - sLat * z;
        //                z1 = -sLng * x + cLng * sLat * y + cLng * cLat * z;
        //                heights[i, j] = (int)(10000000 * __Algorithm.GetAlt(x1, y1, z1));
        //            }
        //        }
        //    }
        //}

        #endregion

        void planet0(double x, double y, double z, int _x, int _y, int depth)
        {
            byte shade;

            double alt = __PlanetAlgorithm.GetAlt(x, y, z, depth, out shade);

            __TmpImage[_x - __StartGenPos.X, _y - __StartGenPos.Y] = __ColorContainer.GetColor(alt);
            __Shades[_x - __StartGenPos.X, _y - __StartGenPos.Y] = shade;
        }

        #endregion
    }
}
