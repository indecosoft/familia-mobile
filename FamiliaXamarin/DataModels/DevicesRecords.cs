using System;
using SQLite;

namespace FamiliaXamarin.DataModels
{
    public class DevicesRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Imei { get; set; }
        public string DateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool LastLocation { get; set; }
        public bool SendPanicAlerts { get; set; }
        public int StepCounter { get; set; }
        public float BloodPresureSystolic { get; set; }
        public float BloodPresureDiastolic { get; set; }
        public float BloodPresurePulsRate { get; set; }
        public int BloodGlucose { get; set; }
        public int OxygenSaturation { get; set; }
        public string Extension { get; set; }
        public string MinutesOfActivity { get; set; }
        public string SecondsOfSleep { get; set; }
        public string SleepType { get; set; }
    }
}