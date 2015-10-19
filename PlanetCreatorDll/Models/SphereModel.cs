using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PlanetGeneratorDll.AMath;

namespace PlanetGeneratorDll.Models
{
    public class SphereModel
    {
        public List<APoint3F> Points { get; private set; }

        public List<RefTriengle> Triengles { get; set; }

        public List<UColor> Colors { get; private set; }

        private readonly SpherePoints<int> __Cache;

        public SpherePoints<int> Cache
        {
            get { return __Cache; }
        }

        public SphereModel(List<APoint3F> points)
        {
            __Cache = new SpherePoints<int>();

            Points = new List<APoint3F>();
            Points.AddRange(points);

            Colors = new List<UColor>();

            foreach (var f in Points)
                Colors.Add(new UColor(0x80, 0x80, 0x80));
        }

        public int MiddlePoint(int p1, int p2)
        {
            int resIndex;

            if (__Cache.Get(p1, p2, out resIndex))
                return resIndex;

            var res = Points[p1] + Points[p2];

            res.Normalize();

            resIndex = Points.Count;

            Points.Add(res);
            Colors.Add(new UColor(0x80, 0x80, 0x80));

            __Cache.Add(p1, p2, resIndex);

            return resIndex;
        }

        //public void Merge(SphereModel model)
        //{
        //    Points.AddRange(model.Points);
        //    Colors.AddRange(model.Colors);

        //    var offset = Triengles.Count;

        //    foreach (var t in model.Triengles)
        //    {
        //        Triengles.Add(new RefTriengle(t.Points[0] + offset, t.Points[0] + offset, t.Points[0] + offset));
        //    }
        //}

        public void Normalize()
        {
            for (int index = 0; index < Points.Count; index++)
            {
                var point = Points[index];
                point.Normalize();

                Points[index] = point;
            }
        }

        /// <summary>
        /// Method for optimize algorithm
        /// </summary>
        public void RemoveUselessPoints()
        {
            var pointIndexes = Triengles.SelectMany(e => e.Points).Distinct().ToList();

            Points = pointIndexes.Select(e => Points[e]).ToList();
            Colors = pointIndexes.Select(e => Colors[e]).ToList();

            foreach (var t in Triengles)
            {
                for (int i = 0; i < t.Points.Length; i++)
                {
                    var pointIndex = t.Points[i];

                    var newIndex = pointIndexes.IndexOf(pointIndex);

                    t.Points[i] = newIndex;
                }
            }
        }

        public void ToDae(StreamWriter sw)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">");
            sb.AppendLine("");

            sb.AppendLine("<asset>");
            sb.AppendLine("<contributor>");
            sb.AppendFormat("<author>{0}</author>", Constants.AUTHOR);
            sb.AppendFormat("<authoring_tool>{0} {1}</authoring_tool>", Constants.AUTHOR, Constants.Version);
            sb.AppendLine("</contributor>");
            sb.AppendFormat("<created>{0}</created>", DateTime.Now);
            sb.AppendFormat("<modified>{0}</modified>", DateTime.Now);
            sb.AppendLine("<unit name=\"meter\" meter=\"1\"/>");
            sb.AppendLine("<up_axis>Z_UP</up_axis>");
            sb.AppendLine("</asset>");

            sb.AppendLine("<library_geometries>");
            sb.AppendLine("<geometry id=\"meshId0\" name=\"meshId0_name\">");
            sb.AppendLine("<mesh>");
            sb.AppendLine("<source id=\"meshId0-positions\" name=\"meshId0-positions\">");

            sb.AppendFormat("<float_array id=\"meshId0-positions-array\" count=\"{0}\">", Points.Count * 3);
            foreach (var p in Points)
                sb.AppendFormat("{0} {1} {2} ", p.X, p.Y, p.Z);
            sb.AppendLine("</float_array>");

            sb.AppendLine("<technique_common>");
            sb.AppendFormat("<accessor count=\"{0}\" offset=\"0\" source=\"#meshId0-positions-array\"  stride=\"3\">", Points.Count);
            sb.AppendLine("<param name=\"X\" type=\"float\"/>");
            sb.AppendLine("<param name=\"Y\" type=\"float\"/>");
            sb.AppendLine("<param name=\"Z\" type=\"float\"/>");
            sb.AppendLine("</accessor>");
            sb.AppendLine("</technique_common>");
            sb.AppendLine("</source>");

            sb.AppendLine("<source id=\"meshId0-color0\" name=\"meshId0-color0\">");
            sb.AppendFormat("<float_array id=\"meshId0-color0-array\" count=\"{0}\">", Colors.Count * 3);

            foreach (var color in Colors)
                sb.AppendFormat("{0} {1} {2} ", color.Rf, color.Gf, color.Bf);

            sb.AppendLine("</float_array>");
            sb.AppendLine("<technique_common>");
            sb.AppendFormat("<accessor offset=\"0\" source=\"#meshId0-color0-array\" count=\"{0}\" stride=\"3\">", Colors.Count);
            sb.AppendLine("<param name=\"R\" type=\"float\"/>");
            sb.AppendLine("<param name=\"G\" type=\"float\"/>");
            sb.AppendLine("<param name=\"B\" type=\"float\"/>");
            sb.AppendLine("</accessor>");
            sb.AppendLine("</technique_common>");
            sb.AppendLine(" </source>");

            sb.AppendLine("<vertices id=\"meshId0-vertices\">");
            sb.AppendLine("<input semantic=\"POSITION\" source=\"#meshId0-positions\"/>");
            sb.AppendLine("<input semantic=\"COLOR\" source=\"#meshId0-color0\"  />");
            sb.AppendLine("</vertices>");

            sb.AppendFormat("<polylist count=\"{0}\">", Triengles.Count);
            sb.AppendLine("<input offset=\"0\" semantic=\"VERTEX\" source=\"#meshId0-vertices\" />");

            sb.AppendFormat("<vcount>");
            for (int i = 0; i < Triengles.Count; i++)
                sb.AppendFormat("3 ");
            sb.AppendLine("</vcount>");

            sb.AppendFormat("<p>");
            foreach (var triengle in Triengles)
                sb.AppendFormat("{0} {1} {2} ", triengle.Points[0], triengle.Points[1], triengle.Points[2]);
            sb.AppendLine("</p>");

            sb.AppendLine("</polylist>");
            sb.AppendLine("</mesh>");
            sb.AppendLine("<extra><technique profile=\"MAYA\"><double_sided>1</double_sided></technique></extra>");
            sb.AppendLine("</geometry>");
            sb.AppendLine("</library_geometries>");

            sb.AppendLine("<library_visual_scenes>");
            sb.AppendLine("<visual_scene id=\"Scene\" name=\"Scene\">");
            sb.AppendLine("<node id=\"Node_0x54a8880\" name=\"Node_0x54a8880\">");
            sb.AppendLine("<matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix>");

            //instance_geometry
            sb.AppendLine("<instance_geometry url=\"#meshId0\">");
            sb.AppendLine("<bind_material>");
            sb.AppendLine("<technique_common>");
            sb.AppendLine("<instance_material symbol=\"defaultMaterial\" target=\"#m0mat\" />");
            sb.AppendLine("</technique_common>");
            sb.AppendLine("</bind_material>");
            sb.AppendLine("</instance_geometry>");

            sb.AppendLine("</node>");
            sb.AppendLine("</visual_scene>");
            sb.AppendLine("</library_visual_scenes>");
            sb.AppendLine("<scene>");
            sb.AppendLine("<instance_visual_scene url=\"#Scene\"/>");
            sb.AppendLine("</scene>");
            sb.AppendLine("");
            sb.AppendLine("</COLLADA>");

            var doc = XDocument.Parse(sb.ToString());

            sw.Write(doc.ToString());
        }

        public void ToPly(StreamWriter sw)
        {
            sw.WriteLine("ply");
            sw.WriteLine("format ascii 1.0");
            sw.WriteLine("element vertex {0}", Points.Count);
            sw.WriteLine("property float x");
            sw.WriteLine("property float y");
            sw.WriteLine("property float z");
            sw.WriteLine("property uchar red");
            sw.WriteLine("property uchar green");
            sw.WriteLine("property uchar blue");
            sw.WriteLine("element face {0}", Triengles.Count);
            sw.WriteLine("property list uint uint vertex_index");
            sw.WriteLine("end_header");

            for (int i = 0; i < Points.Count; i++)
            {
                var point = Points[i];
                var color = Colors[i];
                sw.WriteLine("{0} {1} {2} {3} {4} {5}",
                    point.X,
                    point.Y,
                    point.Z,
                    color.R,
                    color.G,
                    color.B);
            }

            foreach (var t in Triengles)
            {
                sw.WriteLine("3 {0} {1} {2}",
                    t.Points[0],
                    t.Points[1],
                    t.Points[2]);
            }
        }

        public void ToObj(StreamWriter sw)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                var point = Points[i];
                var color = Colors[i];
                sw.WriteLine("v {0} {1} {2} {3} {4} {5}",
                    point.X,
                    point.Y,
                    point.Z,
                    color.R,
                    color.G,
                    color.B);
            }

            sw.WriteLine();

            foreach (var triengle in Triengles)
            {
                sw.WriteLine("f {0} {1} {2}",
                    triengle.Points[0] + 1,
                    triengle.Points[1] + 1,
                    triengle.Points[2] + 1);
            }
        }
    }
}
