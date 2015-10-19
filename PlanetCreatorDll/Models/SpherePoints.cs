using System.Collections.Generic;
using PlanetGeneratorDll.AMath;

namespace PlanetGeneratorDll.Models
{
    public class SpherePoints<T>
    {
        private readonly Dictionary<APoint, T> __Cache;

        public SpherePoints()
        {
            __Cache = new Dictionary<APoint, T>();
        }

        public bool Get(int index1, int index2, out T t)
        {
            var ap = new APoint(index1, index2);
            var revAp = new APoint(index2, index1);

            if (__Cache.ContainsKey(ap))
            {
                t = __Cache[ap];
                return true;
            }

            if (__Cache.ContainsKey(revAp))
            {
                t = __Cache[revAp];
                return true;
            }

            t = default(T);
            return false;
        }

        public void Add(int index1, int index2, T t)
        {
            __Cache[new APoint(index1, index2)] = t;
        }
    }
}
