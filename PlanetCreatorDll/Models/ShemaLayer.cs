using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Common;
using PlanetGeneratorDll.Enums;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class ShemaLayer
    {
        [DataMember]
        public List<ColorHeight> ColorHeights { get; set; }
        [DataMember]
        public ShadeType Shade { get; set; }
        [DataMember]
        public int Level { get; set; }
        [DataMember]
        public bool IsEnable { get; set; }

        [Bindable(true)]
        public IEnumerable<string> ShadeTypes
        {
            get { return ShadeTypeExtension.GetAllTypes().Select(e => e.ToString()); }
        }

        public int ShadeTypeIndex
        {
            get
            {
                var types = ShadeTypeExtension.GetAllTypes();

                for (int i = 0; i < types.Count; i++)
                {
                    if (Shade == types[i])
                        return i;
                }

                throw new Exception("Some fucking error");
            }
            set
            {
                Shade = ShadeTypeExtension.GetAllTypes()[value];
            }
        }

        public int MaxHeight
        {
            get { return ColorHeights.Max(e => e.Heigth); }
        }

        public ShemaLayer()
        {
            ColorHeights = new List<ColorHeight>();
            IsEnable = true;
        }

        #region public methods

        public void Add(int heigth, UColor color)
        {
            ColorHeights.Add(new ColorHeight
            {
                Heigth = heigth,
                Color = color
            });
        }

        public UBitmap GenerateBitmap()
        {
            if (ColorHeights.Count == 0)
                return new UBitmap(1, 1);

            SortColors();

            var maxHeigth = MaxHeight;

            if (maxHeigth == 0)
                return new UBitmap(1, 1);

            var bitmap = new UBitmap(1, maxHeigth);

            ColorHeight prevC = null;

            foreach (var colorHeight in ColorHeights)
            {
                if (prevC == null)
                {
                    prevC = colorHeight;
                    continue;
                }

                if (colorHeight.Heigth == prevC.Heigth)
                    continue;

                for (int i = prevC.Heigth; i < colorHeight.Heigth; i++)
                    bitmap.Map[0, i] =
                        MergeColors(prevC.Color, colorHeight.Color,
                            (1 - (i - prevC.Heigth) / (double)(colorHeight.Heigth - prevC.Heigth)));

                prevC = colorHeight;
            }

            foreach (var colorHeight in ColorHeights)
            {
                if (colorHeight.Heigth > 0)
                    bitmap.Map[0, colorHeight.Heigth - 1] = new UColor(0xff, 0, 0);
            }

            var newBmp = new UBitmap(1, maxHeigth);

            for (int i = 0; i < maxHeigth; i++)
            {
                newBmp[0, i] = bitmap[0, maxHeigth - i - 1];
            }

            return newBmp;
        }

        public void AddNewHeigth()
        {
            ColorHeight toAdd = null;

            switch (Shade)
            {
                case ShadeType.None:
                    toAdd = new ColorHeight { Color = UColor.Transparent };
                    break;
                default:
                    toAdd = new ColorHeight();
                    break;
            }

            ColorHeights.Add(toAdd);
        }

        public void SortColors()
        {
            ColorHeights = ColorHeights.OrderBy(c => c.Heigth).ToList();
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            SortColors();

            foreach (var ch in ColorHeights)
            {
                result.AppendLine(ch.Heigth.ToString("x") + " #" + ch.Color.A.ToString("X2") + ch.Color.R.ToString("X2") + ch.Color.G.ToString("X2") + ch.Color.B.ToString("X2"));
            }

            return result.ToString();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        private UColor MergeColors(UColor l, UColor r, double percent)
        {
            return new UColor(
                (byte)(l.A * percent + r.A * (1 - percent)),
                (byte)(l.R * percent + r.R * (1 - percent)),
                (byte)(l.G * percent + r.G * (1 - percent)),
                (byte)(l.B * percent + r.B * (1 - percent)));
        }
    }
}
