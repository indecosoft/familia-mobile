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
        public int BloodPresureSystolic { get; set; }
        public int BloodPresureDiastolic { get; set; }
        public int BloodPresurePulsRate { get; set; }
        public int BloodGlucose { get; set; }
        public int OxygenSaturation { get; set; }
        public string Extension { get; set; }
    }
}