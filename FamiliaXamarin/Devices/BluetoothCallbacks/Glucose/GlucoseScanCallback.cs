﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.GlucoseDevice;

namespace Familia.Devices.BluetoothCallbacks.Glucose {
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
                                        select new { c.Name, c.Address, c.DeviceType };
            if (!devicesDataNormalized.Any())
                return;
            result.Device.ConnectGatt(Context, true,
                ((GlucoseDeviceActivity)Context)._gattCallback, BluetoothTransports.Le);
            ((GlucoseDeviceActivity)Context)._bluetoothScanner.StopScan(
                ((GlucoseDeviceActivity)Context)._scanCallback);

        }
    }


}
