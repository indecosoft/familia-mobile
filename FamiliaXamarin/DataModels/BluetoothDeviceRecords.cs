using Familia.Devices.Helpers;
using SQLite;

namespace Familia.DataModels
{
    public struct BluetoothDeviceRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public SupportedManufacturers DeviceManufacturer { get; set; }
        public DeviceType DeviceType { get; set; }
    }
}