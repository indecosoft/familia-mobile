using Familia.DataModels;

namespace Familia.Devices.DevicesManagement.Dialogs.Models
{
    public struct DeviceEditingManagementModel
    {
        public int ItemType { get; set; }
        public string ItemValue { get; set; }
        public BluetoothDeviceRecords Device { get; set; }
    }
}