using Android.Bluetooth.LE;
using Android.Runtime;
using Familia.Devices.Bluetooth.Events;

namespace Familia.Devices.Bluetooth.Callbacks {
    public class BluetoothScanCallback : ScanCallback {
        public delegate void ScanResultEventHandler(object source, BluetoothScanCallbackEventArgs args);
        public event ScanResultEventHandler OnScanResultChanged;
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result) {
            OnScanResultChanged?.Invoke(this, new BluetoothScanCallbackEventArgs {
                CallbackType = callbackType,
                Result = result
            });
        }
    }
}
