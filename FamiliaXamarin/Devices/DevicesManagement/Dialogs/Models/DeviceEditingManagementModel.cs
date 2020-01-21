using FamiliaXamarin.DataModels;

namespace FamiliaXamarin.Devices
{
    public class DeviceEditingManagementModel
    {
        public int ItemType { get; set; }
        public string ItemValue { get; set; }
        public BluetoothDeviceRecords Device { get; set; }
    }
}