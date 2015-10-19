using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanetGeneratorDll.Enums
{
    public enum ShadeType
    {
        None = 0,
        All = 1,
        OnlyLand = 2
    }

    public static class ShadeTypeExtension
    {
        public static List<ShadeType> GetAllTypes()
        {
            return ((ShadeType[])Enum.GetValues(typeof(ShadeType))).ToList();
        }
    }
}
