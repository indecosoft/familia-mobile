﻿using System;
using Android.Bluetooth.LE;

namespace Familia.Devices.Bluetooth.Events {
    public class BluetoothScanCallbackEventArgs : EventArgs {
        public ScanCallbackType CallbackType { get; set; }
        public ScanResult Result { get; set; }
    }
}
