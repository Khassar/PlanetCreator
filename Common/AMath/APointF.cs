using System;

namespace Common.AMath
{
    public struct APointF
    {
        public float X;
        public float Y;

        public APointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        #region Public Methods

        public static APointF operator +(APointF point, APointF vector)
        {
            return new APointF(point.X + vector.X, point.Y + vector.Y);
        }

        public static APointF operator -(APointF point, APointF vector)
        {
            return new APointF(point.X - vector.X, point.Y - vector.Y);
        }

        public static APointF operator *(APointF point, float len)
        {
            return new APointF(point.X * len, point.Y * len);
        }

        public bool Equals(APointF point)
        {
            return Equals(point, 0.001f);
        }

        public bool Equals(APointF point, float accuracy)
        {
            return Math.Abs(point.X - X) < accuracy && Math.Abs(point.Y - Y) < accuracy;
        }

        #endregion Public Methods
    }
}
