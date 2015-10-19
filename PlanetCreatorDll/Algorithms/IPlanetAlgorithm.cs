using Common;

namespace PlanetGeneratorDll.Algorithms
{
    public interface IPlanetAlgorithm
    {
        void Initialize(double seed, params object[] objs);

        double GetAlt(double x, double y, double z, int depth, out byte shade);

        RangeF GetAltRange();
    }
}
