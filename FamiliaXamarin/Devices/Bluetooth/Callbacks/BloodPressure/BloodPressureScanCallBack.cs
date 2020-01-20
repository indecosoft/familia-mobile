﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Helpers;

namespace Familia.Devices.BluetoothCallbacks.BloodPressure {
	public class BloodPressureScanCallback : ScanCallback
	{
		private readonly Context _context;
        private readonly IEnumerable<BluetoothDeviceRecords> ListOfSavedDevices;
        public BloodPressureScanCallback(Context context, IEnumerable<BluetoothDeviceRecords> listOfSavedDevices)
		{
			_context = context;
            ListOfSavedDevices = listOfSavedDevices;
        }

		public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
		{
            var devicesDataNormalized = from c in ListOfSavedDevices
                                        where c.Address == result.Device.Address
                                        select new { c.Name, c.Address, c.DeviceType };
            if (!devicesDataNormalized.Any())
                return;
            result.Device.ConnectGatt(_context, false,
				(_context as BloodPressureDeviceActivity)?._gattCallback);
			(_context as BloodPressureDeviceActivity)?._bluetoothScanner.StopScan(
				((BloodPressureDeviceActivity)_context)?._scanCallback);
		}
	}
}
