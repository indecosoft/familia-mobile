using System;

namespace Familia.Devices.PressureDevice
{
    public class BloodPressureData
    {
        public float Systolic { get; set; }
        public float Diastolic { get; set; }
        public float PulseRate { get; set; }
        public DateTime RecordDateTime { get; set; }
    }
}