using SQLite;

namespace Familia.DataModels
{
    class DeviceConfigRecords
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string IntervalBloodPresure { get; set; }
        public string IntervalGlucose { get; set; }
    }
}