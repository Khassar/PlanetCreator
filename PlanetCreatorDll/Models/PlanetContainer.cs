using System.Collections.Generic;
using System.Runtime.Serialization;
using PlanetGeneratorDll.Enums;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class PlanetContainer
    {
        public const string FILE_EXTENTION = ".pg";
        public const string FILE_DESCRIPTION = "Planet generator settings";

        [DataMember(Name = "Shema")]
        public Shema Shema { get; set; }
        [DataMember(Name = "Seeds")]
        public List<double> Seeds { get; set; }
        [DataMember]
        public AlgorithmType Algorithm { get; set; }

        [DataMember]
        public PlanetContainer2D Container2D { get; set; }
        [DataMember]
        public PlanetContainer3D Container3D { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string TimeCreated { get; set; }

        public PlanetContainer()
        {
            Algorithm = AlgorithmType.Classic;
            Seeds = new List<double> { 0 };
            Shema = Shema.GetDefault();
            Container2D = new PlanetContainer2D();
            Container3D = new PlanetContainer3D();
        }

        public static List<PlanetContainer> GetDefaults()
        {
            var result = new List<PlanetContainer>();

            result.Add(GetDesert());
            result.Add(GetPandorica());
            result.Add(GetSun());
            result.Add(GetIce());

            return result;
        }

        public static PlanetContainer GetDesert()
        {
            var result = new PlanetContainer { Name = "Desert" };

            var shema = new Shema();

            var layer = new ShemaLayer { Shade = ShadeType.OnlyLand, Level = 0 };

            layer.Add(0, new UColor(0xFFCD853F));
            layer.Add(250, new UColor(0xFFCD853F));
            layer.Add(500, new UColor(0xFFF4A460));
            layer.Add(627, new UColor(0xFF8B4513));
            layer.Add(1000, new UColor(0xFFDEB887));

            shema.Layers.Add(layer);

            layer = new ShemaLayer { Shade = ShadeType.None, Level = 1 };

            layer.Add(0, new UColor(0));
            layer.Add(400, new UColor(0x00FFFFFF));
            layer.Add(500, new UColor(0xD0FFDEAD));
            layer.Add(600, new UColor(0x00FFFFFF));
            layer.Add(700, new UColor(0xD0FFDEAD));
            layer.Add(800, new UColor(0x00FFFFFF));
            layer.Add(1000, new UColor(0));

            shema.Layers.Add(layer);

            result.Shema = shema;
            return result;
        }

        public static PlanetContainer GetSun()
        {
            var result = new PlanetContainer { Name = "Sun" };

            var shema = new Shema();

            var layer = new ShemaLayer { Shade = ShadeType.None, Level = 0 };

            layer.Add(0, new UColor(0xFFFF0000));
            layer.Add(1000, new UColor(0xFFFFFF00));

            shema.Layers.Add(layer);

            result.Shema = shema;

            return result;
        }

        public static PlanetContainer GetIce()
        {
            var result = new PlanetContainer { Name = "Ice" };

            var shema = new Shema();

            var layer = new ShemaLayer { Shade = ShadeType.OnlyLand, Level = 0 };

            layer.Add(0, new UColor(0xFF000000));
            layer.Add(0x30, new UColor(0xFF1A1BAF));
            layer.Add(0x60 * 5, new UColor(0xFF00BBFF));
            layer.Add(0x80 * 5, new UColor(0xFF0000FF));
            layer.Add(0x91 * 5, new UColor(0xFF000000));
            layer.Add(0xC0 * 5, new UColor(0xFF5C5C9C));
            layer.Add(0xFF * 5, new UColor(0xFF8FAFFF));

            shema.Layers.Add(layer);
            result.Shema = shema;
            return result;
        }

        public static PlanetContainer GetPandorica()
        {
            var result = new PlanetContainer { Name = "Pandorica" };

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

            result.Shema = shema;

            return result;
        }
    }
}
