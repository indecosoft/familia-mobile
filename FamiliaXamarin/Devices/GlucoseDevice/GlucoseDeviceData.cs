using Java.Util;

namespace FamiliaXamarin.GlucoseDevice
{
    class GlucoseDeviceData
    {

        public Calendar Time { get; set; }
        public int TimeOffset { get; set; }
        public float GlucoseConcentration { get; set; }
        public int Unit { get; set; }
        public int Type { get; set; }
        public int SampleLocation { get; set; }
        public int Status { get; set; }
        public int SequenceNumber{ get; set;}
    }
}