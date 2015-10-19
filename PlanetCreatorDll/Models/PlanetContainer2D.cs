using System.Runtime.Serialization;
using PlanetGeneratorDll.Enums;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class PlanetContainer2D
    {
        public const string FILE_EXTENTION = ".pc2";

        #region public properties
        
        [DataMember]
        public double Lng { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double SeaLevel { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public ProjectionType Projection { get; set; }
        

        #endregion

        public PlanetContainer2D()
        {
            Projection = ProjectionType.Orthographic;
            
            Width = 400;
            Height = 400;
        }
        //public void Save(string filePath)
        //{
        //    JsonHelper.SerializeToFile(this, filePath, true);
        //}

        //public static PlanetContainer2D Load(string filePath)
        //{
        //    return JsonHelper.DeserializeFromFile<PlanetContainer2D>(filePath);
        //}

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
