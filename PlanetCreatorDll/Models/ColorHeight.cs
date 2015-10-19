using System.ComponentModel;
using System.Runtime.Serialization;
using Common;
using Common.Helpers;

namespace PlanetGeneratorDll.Models
{
    [DataContract]
    public class ColorHeight
    {
        private UColor __Color;
        private string __Argb;

        [DataMember(Name = "Heigth")]
        public int Heigth { get; set; }

        [DataMember(Name = "ARGB")]
        public string Argb
        {
            get { return __Argb; }
            set
            {
                __Argb = value;
                __Color = new UColor(StringHelper.StringToUint(__Argb));
            }
        }

        [Bindable(true)]
        public UColor Color
        {
            get { return __Color; }
            set
            {
                __Color = value;
                __Argb = StringHelper.UintToString(__Color.ARGB);
            }
        }

        public ColorHeight()
        {
            Heigth = 0;
            Color = UColor.White;
        }
    }
}
