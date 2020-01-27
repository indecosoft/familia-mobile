using System;
using System.Collections.Generic;
using Familia.Devices.Models;
namespace Familia.Devices.Helpers {
    public class SupportedDevices {
        public static readonly List<SupportedDeviceModel> GlucoseDevices = new List<SupportedDeviceModel> {
            new SupportedDeviceModel {
                DeviceName = "CareSens N Premier",
                Manufacturer = SupportedManufacturers.Caresens,
            DeviceType = DeviceType.Glucose},
                new SupportedDeviceModel {
                    DeviceName = "Medisana MediTouch 2",
                Manufacturer = SupportedManufacturers.Medisana,
                DeviceType = DeviceType.Glucose} };
        public static readonly List<SupportedDeviceModel> BloodPressureDevices = new List<SupportedDeviceModel> {
            new SupportedDeviceModel { DeviceName = "Medisana BU 530 Connect" ,
            Manufacturer = SupportedManufacturers.Medisana,
            DeviceType = DeviceType.BloodPressure} };
    }
}
