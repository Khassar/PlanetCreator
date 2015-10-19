using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlanetGeneratorDll.Algorithms;
using PlanetGeneratorDll.AMath;
using PlanetGeneratorDll.Enums;
using PlanetGeneratorDll.Models;

namespace PlanetGeneratorDll
{
    public class PlanetGenerator
    {
        private DateTime __StartTime;

        private CancellationTokenSource __CancellationTokenSource;

        private IPlanetAlgorithm __NativeGenerator;

        public bool InWork { get; private set; }

        public bool WasStoped { get; private set; }

        /// <summary>
        /// Last generation time in miliseconds
        /// </summary>
        public int LastGenerationTime { get; private set; }

        #region public methods

        public void Abort()
        {
            if (!InWork)
                return;

            __CancellationTokenSource?.Cancel();
            __CancellationTokenSource = null;
            WasStoped = true;

            InWork = false;
            WasStoped = true;
        }

        public Task<SphereModel> Generate3D(PlanetContainer planetContainer, Action<float> callBack)
        {
            Init(planetContainer);

            __NativeGenerator = new GClassic();
            __NativeGenerator.Initialize(planetContainer.Seeds[0], ShadeType.None);

            SphereModel model;

            var container = planetContainer.Container3D;

            var token = __CancellationTokenSource.Token;
            return Task<SphereModel>.Factory.StartNew(() =>
            {
                InWork = true;

                if (container.Optimize)
                {
                    var res = GetOptimizeSkeleton(container.RecursionLevel, token);

                    if (token.IsCancellationRequested)
                        return null;

                    model = res.Item1;

                    FillSphereOptimize(model, planetContainer, res.Item2);

                    model.RemoveUselessPoints();
                }
                else
                {
                    model = GetSkeletone(container.RecursionLevel, token);

                    if (token.IsCancellationRequested)
                        return null;

                    FillSphere(model, planetContainer);
                }

                InWork = false;
                LastGenerationTime = (int)(DateTime.Now - __StartTime).TotalMilliseconds;
                return model;
            }, token);
        }

        public Task<UBitmap> Generate2D(PlanetContainer planetContainer, Action<float> callBack, bool UseFourThreads)
        {
            Init(planetContainer);

            var token = __CancellationTokenSource.Token;
            return Task<UBitmap>.Factory.StartNew(() =>
            {
                InWork = true;
                var bmp = (UseFourThreads ?
                    Generate2DInFourThreadsSync(planetContainer, callBack, token) :
                    Generate2DInOneThread(planetContainer, callBack, token));

                InWork = false;
                LastGenerationTime = (int)(DateTime.Now - __StartTime).TotalMilliseconds;
                return bmp;
            }, token);
        }

        #endregion

        #region private methods

        private void Init(PlanetContainer planetContainer)
        {
            if (planetContainer == null)
                throw new ArgumentNullException(nameof(planetContainer));
            if (InWork)
                throw new InvalidOperationException("generation just started");

            __CancellationTokenSource = new CancellationTokenSource();

            __StartTime = DateTime.Now;
            WasStoped = false;
        }

        #region 3D

        private Tuple<List<RefTriengle>, List<APoint3F>> GetBaseSkeleton()
        {
            var bp = GetBaseGeodesicPoints();
            var triengles = new List<RefTriengle>();

            triengles.Add(new RefTriengle(0, 11, 5));
            triengles.Add(new RefTriengle(0, 5, 1));
            triengles.Add(new RefTriengle(0, 1, 7));
            triengles.Add(new RefTriengle(0, 7, 10));
            triengles.Add(new RefTriengle(0, 10, 11));

            // 5 adjacent faces 
            triengles.Add(new RefTriengle(1, 5, 9));
            triengles.Add(new RefTriengle(5, 11, 4));
            triengles.Add(new RefTriengle(11, 10, 2));
            triengles.Add(new RefTriengle(10, 7, 6));
            triengles.Add(new RefTriengle(7, 1, 8));

            // 5 faces around point 3
            triengles.Add(new RefTriengle(3, 9, 4));
            triengles.Add(new RefTriengle(3, 4, 2));
            triengles.Add(new RefTriengle(3, 2, 6));
            triengles.Add(new RefTriengle(3, 6, 8));
            triengles.Add(new RefTriengle(3, 8, 9));

            // 5 adjacent faces 
            triengles.Add(new RefTriengle(4, 9, 5));
            triengles.Add(new RefTriengle(2, 4, 11));
            triengles.Add(new RefTriengle(6, 2, 10));
            triengles.Add(new RefTriengle(8, 6, 7));
            triengles.Add(new RefTriengle(9, 8, 1));

            return new Tuple<List<RefTriengle>, List<APoint3F>>(triengles, bp);
        }

        private SphereModel GetSkeletone(int recursionLevel, CancellationToken token)
        {
            var res = GetBaseSkeleton();

            var bp = res.Item2;
            var triengles = res.Item1;

            var model = new SphereModel(bp);

            for (int i = 0; i < recursionLevel; i++)
            {
                var newTriengles = new List<RefTriengle>();

                foreach (var t in triengles)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    var a = model.MiddlePoint(t.Points[0], t.Points[1]);
                    var b = model.MiddlePoint(t.Points[1], t.Points[2]);
                    var c = model.MiddlePoint(t.Points[2], t.Points[0]);

                    newTriengles.Add(new RefTriengle(t.Points[0], a, c));
                    newTriengles.Add(new RefTriengle(t.Points[1], b, a));
                    newTriengles.Add(new RefTriengle(t.Points[2], c, b));
                    newTriengles.Add(new RefTriengle(a, b, c));
                }

                triengles = newTriengles;
            }

            model.Normalize();
            model.Triengles = triengles;

            return model;
        }

        private Tuple<SphereModel, Dictionary<int, List<RefTriengleOptimize>>> GetOptimizeSkeleton(int recursionLevel, CancellationToken token)
        {
            var res = GetBaseSkeleton();

            var bp = res.Item2;
            var triengles = res.Item1;
            var resultDic = new Dictionary<int, List<RefTriengleOptimize>>();

            resultDic[0] = triengles.Select(e => new RefTriengleOptimize(e, 0)).ToList();

            var model = new SphereModel(bp);

            for (int i = 0; i < recursionLevel; i++)
            {
                var parents = resultDic[i];
                var childs = new List<RefTriengleOptimize>();

                foreach (var t in parents)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    var a = model.MiddlePoint(t.Points[0], t.Points[1]);
                    var b = model.MiddlePoint(t.Points[1], t.Points[2]);
                    var c = model.MiddlePoint(t.Points[2], t.Points[0]);

                    var refs = new List<RefTriengleOptimize>
                    {
                        new RefTriengleOptimize(new RefTriengle(t.Points[0], a, c),i+1),
                        new RefTriengleOptimize(new RefTriengle(t.Points[1], b, a),i+1),
                        new RefTriengleOptimize(new RefTriengle(t.Points[2], c, b),i+1),
                        new RefTriengleOptimize(new RefTriengle(a, b, c),i+1)
                    };

                    foreach (var refT in refs)
                        refT.Parent = t;

                    t.Childs = refs.ToArray();
                    childs.AddRange(refs);
                }

                resultDic[i + 1] = childs;
            }

            model.Normalize();
            model.Triengles = triengles;

            return new Tuple<SphereModel, Dictionary<int, List<RefTriengleOptimize>>>(model, resultDic);
        }

        #endregion

        #region private methods

        private void FillSphereOptimize(SphereModel model, PlanetContainer settings,
            Dictionary<int, List<RefTriengleOptimize>> triengles)
        {
            var container = settings.Container3D;
            var lastRank = container.RecursionLevel;

            var cc = new ColorContainer(settings.Shema.Layers[0], __NativeGenerator.GetAltRange());
            var optimazeLevel = container.OptimizePecent * 0.01f;
            var lastTriengles = triengles[lastRank];

            foreach (var triengle in lastTriengles)
                triengle.IsOk = true;

            var points = lastTriengles
                .SelectMany(e => e.Points)
                .Distinct();

            foreach (var pointIndex in points)
            {
                var p = model.Points[pointIndex];

                byte s;
                var alt = __NativeGenerator.GetAlt(p.X, p.Y, p.Z, 100, out s);

                if (alt > 0)
                    p *= (1 + (float)alt * container.LandscapeOver);
                else
                    p *= (1 + (float)alt * container.LandscapeUnder);

                var color = cc.GetColor(alt);

                model.Colors[pointIndex] = color;
                model.Points[pointIndex] = p;
            }

            var forProcess = lastTriengles;

            while (forProcess.Count > 0)
            {
                var nextWave = new List<RefTriengleOptimize>();

                foreach (var t in forProcess.Where(e => e.Parent != null && e.Rank > 2))
                {
                    if (t.Optimize(model, optimazeLevel))
                        nextWave.Add(t.Parent);
                }

                forProcess = nextWave;
            }

            var refResult = triengles
                .SelectMany(e => e.Value)
                .Where(e => e.IsOk).ToList();

            foreach (var t in refResult)
                t.FixLines(model);

            model.Triengles = refResult
                .Select(e => e.Triengle)
                .ToList();
        }

        private void FillSphere(SphereModel model, PlanetContainer settings)
        {
            var container = settings.Container3D;
            var cc = new ColorContainer(settings.Shema.Layers[0], __NativeGenerator.GetAltRange());
            var len = model.Points.Count;

            for (int i = 0; i < len; i++)
            {
                var p = model.Points[i];
                byte s;
                var alt = __NativeGenerator.GetAlt(p.X, p.Y, p.Z, 100, out s);

                if (alt > 0)
                    p *= (1 + (float)alt * container.LandscapeOver);
                else
                    p *= (1 + (float)alt * container.LandscapeUnder);

                var color = cc.GetColor(alt);

                model.Colors[i] = color;
                model.Points[i] = p;
            }
        }

        private static List<APoint3F> GetBaseGeodesicPoints()
        {
            var t = (float)((1.0 + Math.Sqrt(5.0)) / 2.0);
            var bw = new List<APoint3F>
            {
                new APoint3F(-1, t, 0),
                new APoint3F(1, t, 0),
                new APoint3F(-1, -t, 0),
                new APoint3F(1, -t, 0),

                new APoint3F(0, -1, t),
                new APoint3F(0, 1, t),
                new APoint3F(0, -1, -t),
                new APoint3F(0, 1, -t),

                new APoint3F(t, 0, -1),
                new APoint3F(t, 0, 1),
                new APoint3F(-t, 0, -1),
                new APoint3F(-t, 0, 1)
            };

            return bw;
        }

        #endregion

        #region 2D

        private UBitmap Generate2DInOneThread(PlanetContainer planetContainer, Action<float> callBack, CancellationToken token)
        {
            var pc = planetContainer;
            var generator = new PlanetGeneratorNative(pc.Algorithm);

            generator.OnProgressChange += callBack;

            return generator.Generate(pc, token);
        }

        public UBitmap Generate2DInFourThreadsSync(PlanetContainer planetContainer, Action<float> callBack, CancellationToken token)
        {
            const int CORES = 4;

            var container = planetContainer.Container2D;

            __StartTime = DateTime.Now;

            var genTasks = new Task[CORES];
            var bmps = new UColor[CORES][,];

            var pg = new PlanetGeneratorNative[CORES];

            var percents = new float[CORES];

            const int PICTURE_SHIFT = 2;

            var w = container.Width / 2 + PICTURE_SHIFT;
            var h = container.Height / 2 + PICTURE_SHIFT;

            var resultBitmap = new UColor[container.Width, container.Height];

            for (int i = 0; i < CORES; i++)
            {
                var index = i;
                genTasks[i] = new Task(() =>
                {
                    pg[index] = new PlanetGeneratorNative(AlgorithmType.Classic);

                    pg[index].OnProgressChange = p =>
                    {
                        percents[index] = p;

                        callBack?.Invoke(percents.Sum() / CORES);
                    };

                    var lx = w * (index / 2);
                    var ly = h * (index % 2);

                    if (lx > 0)
                        lx -= PICTURE_SHIFT;

                    if (ly > 0)
                        ly -= PICTURE_SHIFT;

                    var uBmp = pg[index].Generate(planetContainer, token, lx, ly, w, h);
                    if (token.IsCancellationRequested)
                        return;
                    var tmpBmp = uBmp.Map;
                    bmps[index] = tmpBmp;
                }, token);
            }

            for (int i = 0; i < CORES; i++)
                genTasks[i].Start();

            Task.WaitAll(genTasks);

            if (token.IsCancellationRequested)
                return null;

            for (int i = 0; i < CORES; i++)
            {
                var lw = w - PICTURE_SHIFT;
                var lh = h - PICTURE_SHIFT;

                var lx = (i / 2) * lw;
                var ly = (i % 2) * lh;

                var dx = lx > 0 ? PICTURE_SHIFT : 0;
                var dy = ly > 0 ? PICTURE_SHIFT : 0;

                for (int x = 0; x < lw; x++)
                    for (int y = 0; y < lh; y++)
                        resultBitmap[lx + x, ly + y] = bmps[i][x + dx, y + dy];
            }

            return new UBitmap(resultBitmap);
        }

        #endregion

        #endregion
    }
}
