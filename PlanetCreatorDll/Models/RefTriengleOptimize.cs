using System;
using System.Linq;
using PlanetGeneratorDll.AMath;

namespace PlanetGeneratorDll.Models
{
    public class RefTriengleOptimize
    {
        private bool __Proccessed;

        public RefTriengle Triengle;

        public RefTriengleOptimize Parent;

        public RefTriengleOptimize[] Childs;

        private readonly int __Rank;

        private double[] Alts { get; set; }

        public int[] Points
        {
            get { return Triengle.Points; }
        }

        public int Rank
        {
            get { return __Rank; }
        }

        public bool IsOk { get; set; }

        public RefTriengleOptimize(RefTriengle triengle, int rank)
        {
            Triengle = triengle;
            __Rank = rank;
            __Proccessed = false;
            IsOk = false;
        }

        public bool Optimize(SphereModel model, float optimizeIndent)
        {
            if (__Proccessed)
                return false;

            __Proccessed = true;

            if (Childs == null)
            {
                IsOk = true;
                return true;
            }

            if (Childs.Any(e => !e.IsOk))
                return false;

            var pointIndexes = Childs.SelectMany(e => e.Points).Distinct().ToList();
            var points = pointIndexes.Select(index => model.Points[index]).ToList();
            var colors = pointIndexes.Select(index => model.Colors[index]).ToArray();

            var lens = points.Select(e => e.Length).ToList();

            var minPointLenght = lens.Min();
            var maxPointLenght = lens.Max();

            var div = (maxPointLenght / minPointLenght - 1);

            if (Math.Abs(div) < optimizeIndent && IsColorDivEnabled(colors, optimizeIndent))
            {
                Optimize(model);
                return true;
            }

            return false;
        }

        private bool IsColorDivEnabled(UColor[] colors, float indent)
        {
            for (int i = 0; i < colors.Length; i++)
                for (int j = i + 1; j < colors.Length; j++)
                    if (!IsColorsEquels(colors[i], colors[j], indent))
                        return false;

            return true;
        }

        private bool IsColorsEquels(UColor c1, UColor c2, float indent)
        {
            if (Math.Abs((c1.R - c2.R) / 256f) >= indent)
                return false;
            if (Math.Abs((c1.G - c2.G) / 256f) >= indent)
                return false;
            if (Math.Abs((c1.B - c2.B) / 256f) >= indent)
                return false;
            return true;
        }

        private void Optimize(SphereModel model)
        {
            IsOk = true;
            foreach (var child in Childs)
                child.IsOk = false;
        }

        public void FixLines(SphereModel model)
        {
            OptimazeLine(model, Points[0], Points[1]);
            OptimazeLine(model, Points[1], Points[2]);
            OptimazeLine(model, Points[2], Points[0]);
        }

        private void OptimazeLine(SphereModel model, int p1, int p2)
        {
            int pIndex;
            if (model.Cache.Get(p1, p2, out pIndex))
            {
                model.Points[pIndex] = (model.Points[p1] + model.Points[p2]) * 0.5f;

                OptimazeLine(model, p1, pIndex);
                OptimazeLine(model, p2, pIndex);
            }
        }
    }
}
