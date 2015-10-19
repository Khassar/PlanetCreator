using System;

namespace PlanetGeneratorDll.Helpers
{
    public class StringHelper
    {
        public static string SizeToFriendly(long size)
        {
            const int MULTIPER = 1024;

            var str = "";
            long deltaSize = 1;

            if (size > MULTIPER * MULTIPER * MULTIPER)
            {
                str = "Gb";
                deltaSize = MULTIPER * MULTIPER * MULTIPER;
            }
            else
                if (size > MULTIPER * MULTIPER)
                {
                    str = "Mb";
                    deltaSize = MULTIPER * MULTIPER;
                }
                else
                    if (size > MULTIPER)
                    {
                        str = "Kb";
                        deltaSize = MULTIPER;
                    }
                    else
                    {
                        str = "b";
                        deltaSize = 1;
                    }


            return (size / (double)deltaSize).ToString("F2") + str;
        }

        public static string UintToString(uint val)
        {
            return val.ToString("X8");
        }

        public static string GetLastNameInPath(string path, string indent)
        {
            var strs = path.Split(new[] {indent}, StringSplitOptions.None);

            int count = strs.Length;

            if (count == 0)
                return string.Empty;

            return strs[count - 1];
        }

        public static uint StringToUint(string str)
        {
            return Convert.ToUInt32(str, 16);
        }
    }
}
