using SQLite;

namespace FamiliaXamarin.DataModels
{
    public class BluetoothDeviceRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DeviceType { get; set; }
    }
}