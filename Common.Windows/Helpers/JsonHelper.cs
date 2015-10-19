using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CommonWindows.Helpers
{
    public class JsonHelper
    {
        private const string INDENT_STRING = "    ";

        private static readonly Encoding __Encoding = Encoding.UTF8;

        private static DataContractJsonSerializer __Ds;
        public static string SaveToString<T>(T obj, bool formating = false)
        {
            var memoryStream = new MemoryStream();
            __Ds = new DataContractJsonSerializer(typeof(T));
            __Ds.WriteObject(memoryStream, obj);
            var jsonStr = __Encoding.GetString(memoryStream.ToArray());
            memoryStream.Close();
            return formating ? FormatJson(jsonStr) : jsonStr;
        }

        public static T LoadFromString<T>(string jsonString)
        {
            __Ds = new DataContractJsonSerializer(typeof(T));
            var stream1 = new MemoryStream(__Encoding.GetBytes(jsonString));
            return (T)__Ds.ReadObject(stream1);
        }

        public static void SaveToFile<T>(T obj, string filePath, bool formating = false)
        {
            using (var sw = new StreamWriter(filePath))
            {
                string toFile = SaveToString(obj, formating);
                sw.Write(toFile);
                sw.Close();
            }
        }

        public static T LoadFromFile<T>(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                string fromFile = sr.ReadToEnd();
                return LoadFromString<T>(fromFile);
            }
        }

        private static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            foreach (var j in Enumerable.Range(0, ++indent))
                                sb.Append(INDENT_STRING);
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            foreach (int j in Enumerable.Range(0, --indent))
                                sb.Append(INDENT_STRING);
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            foreach (int j in Enumerable.Range(0, indent))
                                sb.Append(INDENT_STRING);
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
