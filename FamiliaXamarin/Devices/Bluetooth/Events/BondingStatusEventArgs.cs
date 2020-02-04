using System;
using Android.Bluetooth;

namespace Familia.Devices.Bluetooth.Events {
    public class BondingStatusEventArgs : EventArgs {
        public BluetoothDevice Device { get; set; }
    }

}
