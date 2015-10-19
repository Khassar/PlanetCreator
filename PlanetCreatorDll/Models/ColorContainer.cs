namespace PlanetGeneratorDll.Models
{
    public class ColorContainer
    {
        private readonly UColor[] __ColorTable;
        private const int MAX_COLOR_INDEX = 0xFFFF;

        private readonly int __MaxHeight;
        private readonly double __MinAlt;
        private readonly double __MaxAlt;
        private readonly double __AltRange;

        public ColorContainer(ShemaLayer layer, RangeF rangeF)
        {
            __MinAlt = rangeF.Min;
            __MaxAlt = rangeF.Max;

            __AltRange = __MaxAlt - __MinAlt;

            int cNum = 0;
            int oldcNum = 0;

            __ColorTable = new UColor[MAX_COLOR_INDEX];

            layer.SortColors();

            foreach (var ch in layer.ColorHeights)
            {
                cNum = ch.Heigth;

                ReadColor(oldcNum, cNum, ch.Color);

                oldcNum = cNum;
            }

            __MaxHeight = cNum;
        }

        private void ReadColor(int oldcNum, int cNum, UColor color)
        {
            if (cNum < oldcNum) cNum = oldcNum;
            if (cNum > MAX_COLOR_INDEX) cNum = MAX_COLOR_INDEX;

            __ColorTable[cNum] = color;

            for (var i = oldcNum + 1; i < cNum; i++)
                __ColorTable[i] = UColor.Merge(
                    __ColorTable[oldcNum],
                    __ColorTable[cNum],
                    cNum - i,
                    i - oldcNum);
        }

        public UColor GetColor(double alt)
        {
            if (alt < __MinAlt)
                alt = __MinAlt;
            if (alt > __MaxAlt)
                alt = __MaxAlt;

            alt = (alt - __MinAlt) / __AltRange;

            var colorIndex = alt * __MaxHeight;
            return __ColorTable[(int)colorIndex];
        }
    }
}
