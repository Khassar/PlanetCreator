using System;

namespace Common
{
    public class UBitmap
    {
        private readonly int __Width;
        private readonly int __Height;
        private readonly UColor[,] __Map;

        public UColor[,] Map
        {
            get { return __Map; }
        }

        public int Width
        {
            get { return __Width; }
        }

        public int Height
        {
            get { return __Height; }
        }

        public UBitmap(int width, int height)
        {
            __Width = width;
            __Height = height;

            __Map = new UColor[Width, Height];
        }

        public UBitmap(UColor[,] map)
        {
            if (map == null) 
                throw new ArgumentNullException("map");

            __Map = map;
            __Width = Map.GetLength(0);
            __Height = map.GetLength(1);
        }

        public void Set(int x, int y, UColor color)
        {
            __Map[x, y] = color;
        }

        public UColor this[int x, int y]
        {
            get { return __Map[x, y]; }
            set { __Map[x, y] = value; }
        }
    }
}
