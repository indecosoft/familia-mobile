using System;
using Android.Bluetooth;

namespace Familia.Devices.BluetoothEvents {
    public class BondingStatusEventArgs : EventArgs {
        public BluetoothDevice Device { get; set; }
    }

}
