using System;
using Familia.Devices.Helpers;
namespace Familia.Devices.Models {
    public class SupportedDeviceModel {
		public string DeviceName { get; set; }
		public SupportedManufacturers Manufacturer { get; set; }
        public DeviceType DeviceType { get; set; }
    }
}
