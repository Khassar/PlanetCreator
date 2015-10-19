using System;

namespace Common.Helpers
{
    public class RandomHelper
    {
        private static readonly Random __Random = new Random((int)DateTime.Now.Ticks);
        public static double NextDouble(double min, double max, int round = -1)
        {
            var d = __Random.NextDouble() * (max - min) + min;

            if (round > 0)
                d = Math.Round(d, round);

            return d;
        }

        public static float NextFloat(float min, float max)
        {
            return min + (float)__Random.NextDouble() * (max - min);
        }

        public static int Next(int max)
        {
            return Next(0, max);
        }

        public static int Next(int min, int max)
        {
            return __Random.Next(min, max);
        }

        public static int NextMaxInt()
        {
            return __Random.Next(int.MinValue, int.MaxValue);
        }

        public static Ultima NextUltima()
        {
            return new Ultima
            {
                Int1 = __Random.Next(int.MinValue, int.MaxValue),
                Int2 = __Random.Next(int.MinValue, int.MaxValue)
            };
        }
    }
}
