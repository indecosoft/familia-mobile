using Java.Util;

namespace FamiliaXamarin.PressureDevice
{
    public class BloodPressureData
    {

        public float Sys { get; set; }
        public float Dia { get; set; }
        public float Pul { get; set; }
        public Calendar Data { get; set; }

        public BloodPressureData(float sys, float dia, float pul, Calendar data)
        {
            Sys = sys;
            Dia = dia;
            Pul = pul;
            Data = data;
        }
    }
}