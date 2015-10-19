using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class PlanetContainer3D : INotifyPropertyChanged
    {
        public const string FILE_EXTENSION = ".3dc";
        public const string FILE_DESCRIPTION = "3d generation settings";

        private int __RecursionLevel;
        private float __LandscapeOver;
        private float __LandscapeUnder;
        private double __Seed;
        private bool __Optimize;
        private int __OptimizePecent;

        [DataMember]
        public int RecursionLevel
        {
            get { return __RecursionLevel; }
            set
            {
                __RecursionLevel = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public float LandscapeOver
        {
            get { return __LandscapeOver; }
            set
            {
                __LandscapeOver = (float)Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        [DataMember]
        public float LandscapeUnder
        {
            get { return __LandscapeUnder; }
            set
            {
                __LandscapeUnder = (float)Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        [DataMember]
        public double Seed
        {
            get { return __Seed; }
            set
            {
                __Seed = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public bool Optimize
        {
            get { return __Optimize; }
            set
            {
                __Optimize = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int OptimizePecent
        {
            get { return __OptimizePecent; }
            set
            {
                __OptimizePecent = value; 
                OnPropertyChanged();
            }
        }

        public PlanetContainer3D()
        {
            RecursionLevel = 3;
            LandscapeOver = 0.8f;
            LandscapeUnder = 0.1f;
            OptimizePecent = 3;
            Seed = 0;
            Optimize = false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
