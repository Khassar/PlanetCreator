namespace Common
{
    public struct RangeF
    {
        private readonly float __Min;
        private readonly float __Max;

        public float Min
        {
            get { return __Min; }
        }
        public float Max
        {
            get { return __Max; }
        }

        public RangeF(float min, float max)
        {
            __Min = min;
            __Max = max;
        }
    }
}
