using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using Familia.DataModels;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;

namespace Familia.Devices.Bluetooth.Callbacks.Glucose {
    public class GlucoseScanCallback : ScanCallback {
        private readonly Context Context;
        private readonly IEnumerable<BluetoothDeviceRecords> ListOfSavedDevices;
        public GlucoseScanCallback(Context context, IEnumerable<BluetoothDeviceRecords> listOfSavedDevices) {
            Context = context;
            ListOfSavedDevices = listOfSavedDevices;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result) {
            var devicesDataNormalized = from c in ListOfSavedDevices
                                        where c.Address == result.Device.Address
                                        select new { c.Name, c.Address, c.DeviceType, c.DeviceManufacturer };
            if (!devicesDataNormalized.Any())
                return;
            switch(devicesDataNormalized.FirstOrDefault().DeviceManufacturer) {
                case SupportedManufacturers.Medisana:
                    result.Device.ConnectGatt(Context, true,
                ((GlucoseDeviceActivity)Context).MedisanaGattCallback, BluetoothTransports.Le);
                    ((GlucoseDeviceActivity)Context).BluetoothScanner.StopScan(
                ((GlucoseDeviceActivity)Context).ScanCallback);
                    break;
                case SupportedManufacturers.Caresens:
                    result.Device.ConnectGatt(Context, true,
                ((GlucoseDeviceActivity)Context).GattCallback, BluetoothTransports.Le);
                    ((GlucoseDeviceActivity)Context).BluetoothScanner.StopScan(
                ((GlucoseDeviceActivity)Context).ScanCallback);
                    break;
            }
            

        }
    }


}
