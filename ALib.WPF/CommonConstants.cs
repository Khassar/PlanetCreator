using System;
using System.Text;

namespace CommonWindows
{
    public class CommonConstants
    {
        public readonly static Version Version = new Version(1, 0);

        public readonly static Encoding DefaultEncoding = Encoding.UTF8;

        public const int TCP_MAX_PACKET_SIZE_COMMAND = 8192;        // для команд
        public const int TCP_MAX_PACKET_SIZE_DATA_TRANFER = 32768;  // для данных
        public const byte TCP_PACKET_INDENT = 0xFF;                 // Для команд
    }
}
