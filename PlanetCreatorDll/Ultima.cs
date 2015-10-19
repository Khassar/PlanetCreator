using System.Runtime.InteropServices;

namespace PlanetGeneratorDll
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Ultima
    {
        [FieldOffset(0)]
        public double Double;
        [FieldOffset(0)]
        public long Long;
        [FieldOffset(0)]
        public ulong ULong;
        [FieldOffset(0)]
        public int Int1;
        [FieldOffset(4)]
        public int Int2;
    }
}
