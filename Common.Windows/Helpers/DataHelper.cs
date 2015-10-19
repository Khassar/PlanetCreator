using System;
using System.Drawing;
using System.Linq;
using System.Text;
using PlanetGeneratorDll;

namespace CommonWindows.Helpers
{
    public class DataHelper
    {
        private static readonly Random __Random = new Random((int)DateTime.Now.Ticks);

        public static byte[] SerializeToByte<T>(T obj)
        {
            var jsonStr = JsonHelper.SaveToString(obj);
            return StringToBytes(jsonStr);
        }

        public static T DeserializeFromByte<T>(byte[] data, int length)
        {
            var jsonStr = ByteToString(data, length);
            return JsonHelper.LoadFromString<T>(jsonStr);
        }

        public static string ByteToString(byte[] data, int length)
        {
            return CommonConstants.DefaultEncoding.GetString(data, 0, length);
        }

        public static string ByteToString(byte[] data)
        {
            return ByteToString(data, data.Length);
        }

        public static byte[] StringToBytes(string str)
        {
            return CommonConstants.DefaultEncoding.GetBytes(str);
        }

        public static Bitmap ToBitmap(UBitmap uBmp)
        {
            int w = uBmp.Width;
            int h = uBmp.Height;
            var u = uBmp.Map;
            var bmp = new Bitmap(w, h);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    var uc = u[x, y];
                    bmp.SetPixel(x, y, Color.FromArgb(uc.A, uc.R, uc.G, uc.B));
                }

            return bmp;
        }

        #region coding

        public static string ExportKey(byte[] key)
        {
            var stringBuilder = new StringBuilder();

            foreach (var k in key)
                stringBuilder.Append(k.ToString("X2"));

            return stringBuilder.ToString();
        }

        public static byte[] DecodeHex(string hex)
        {
            if (hex.Length % 2 == 1)
                hex = hex + "0";

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] GenerateKey(int length)
        {
            var bytes = new byte[length];

            for (int i = 0; i < length; i++)
                bytes[i] = (byte)__Random.Next(256);

            return bytes;
        }

        public static string FormatKey(byte[] key, int keyOnLine)
        {
            string result = "";

            for (int i = 0; i < key.Length; i++)
            {
                if (i != 0 && i % keyOnLine == 0)
                    result += Environment.NewLine;

                result += "0x" + key[i].ToString("X2") + ",";
            }

            return result;
        }

        public static void Code(ref byte[] data, byte[] key)
        {
            int some = 0;
            Code(ref data, key, ref some);
        }

        public static void Code(ref byte[] data, byte[] key, ref int codeIndex)
        {
            Code(ref data, data.Length, key, ref codeIndex);
        }

        public static void Code(ref byte[] data, int length, byte[] key, ref int codeIndex)
        {
            var keyLength = key.Length;

            if (keyLength == 1)
            {
                var keyByte = key[0];
                for (var index = 0; index < length; index++)
                    data[index] ^= keyByte;
            }
            else
            {
                for (var index = 0; index < length; index++)
                {
                    data[index] ^= key[codeIndex];

                    codeIndex++;

                    if (codeIndex >= keyLength)
                        codeIndex = 0;
                }
            }
        }

        #endregion
    }
}
