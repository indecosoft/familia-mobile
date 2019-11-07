using FamiliaXamarin.DataModels;

namespace FamiliaXamarin.Devices
{
    public class DevicesManagementModel
    {
        public int ItemType { get; set; }
        public string ItemValue { get; set; }
        public BluetoothDeviceRecords Device { get; set; }
    }
}