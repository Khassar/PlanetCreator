namespace PlanetGeneratorDll.AMath
{
    public class RefTriengle
    {
        private readonly int[] __Points;

        public int[] Points
        {
            get { return __Points; }
        }

        public RefTriengle(int p1, int p2, int p3)
        {
            __Points = new[] { p1, p2, p3 };
        }
    }
}
