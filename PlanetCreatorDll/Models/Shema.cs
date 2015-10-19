using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common;
using Common.Helpers;
using PlanetGeneratorDll.Enums;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class Shema
    {
        #region constants

        public const string FILE_EXTENTION = ".shm2";

        #endregion

        #region public properties

        [DataMember]
        public List<ShemaLayer> Layers { get; set; }


        #endregion

        public Shema()
        {
            Layers = new List<ShemaLayer>();
        }

        #region public methods

        public void SortByLevel()
        {
            Layers = Layers.OrderBy(l => l.Level).ToList();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public static Shema GetDefault()
        {
            var shema = new Shema();

            var layer = new ShemaLayer { Shade = ShadeType.OnlyLand, Level = 0 };

            layer.Add(0, new UColor(0xFF000000));
            layer.Add(0x80 * 5, new UColor(0xFF483D8B));
            layer.Add(0x87 * 5, new UColor(0xFF8470FF));
            layer.Add(0x9A * 5, new UColor(0xFFEEDD82));
            layer.Add(0xBD * 5, new UColor(0xFF6B8E23));
            layer.Add(0xFF * 5, new UColor(0xFF000000));

            shema.Layers.Add(layer);

            layer = new ShemaLayer { Shade = ShadeType.None, Level = 1 };

            layer.Add(0, new UColor(0));
            layer.Add(0x60 * 5, new UColor(0));
            layer.Add(0x61 * 5, new UColor(0x20FFFFFF));
            layer.Add(0x88 * 5, new UColor(0xB0FFFFFF));
            layer.Add(0x98 * 5, new UColor(0xB0FFFFFF));
            layer.Add(0xD0 * 5, new UColor(0x20FFFFFF));
            layer.Add(0xD1 * 5, new UColor(0));
            layer.Add(0xFF * 5, new UColor(0));

            shema.Layers.Add(layer);

            return shema;
        }

        public static Shema GenerateRandom()
        {
            var container = new Shema();

            int layersCount = RandomHelper.Next(2);

            for (int i = 0; i <= layersCount; i++)
            {
                var layer = new ShemaLayer
                {
                    Shade = i == 0 ? RandomShadeType() : ShadeType.None,
                    Level = i
                };

                int levels = 2 + RandomHelper.Next((i == 0 ? 5 : 3));
                int prevH = 0;
                if (i % 2 == 0)
                {
                    for (int l = 0; l < levels; l++)
                    {
                        int curH = l == 0 ? 0 : RandomHelper.Next(200) + 1;
                        layer.Add(curH + prevH, new UColor(0xFF, (byte)RandomHelper.Next(256), (byte)RandomHelper.Next(256), (byte)RandomHelper.Next(256)));
                        prevH += curH;
                    }
                }
                else
                {
                    for (int l = 0; l < levels; l++)
                    {
                        int curH = l == 0 ? 0 : RandomHelper.Next(200) + 1;
                        layer.Add(curH + prevH, new UColor((byte)(RandomHelper.Next(200) + 5), (byte)RandomHelper.Next(256), (byte)RandomHelper.Next(256), (byte)RandomHelper.Next(256)));
                        prevH += curH;
                    }
                }

                container.Layers.Add(layer);
            }

            return container;
        }

        public static ShadeType RandomShadeType()
        {
            switch (RandomHelper.Next(3))
            {
                case 0:
                    return ShadeType.None;
                case 1:
                    return ShadeType.OnlyLand;
                default:
                    return ShadeType.All;
            }
        }

        #endregion

    }
}
