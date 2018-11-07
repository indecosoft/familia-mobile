using Java.Util;

namespace FamiliaXamarin.Devices.PressureDevice
{
    public class BloodPressureData
    {

        public float Sys { get;}
        public float Dia { get;}
        public float Pul { get;}
        public Calendar Data { get;}

        public BloodPressureData(float sys, float dia, float pul, Calendar data)
        {
            Sys = sys;
            Dia = dia;
            Pul = pul;
            Data = data;
        }
    }
}