using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Familia.Devices.Bluetooth.Events;

namespace Familia.Devices.Bluetooth.Receivers {
    [BroadcastReceiver]
    public class BondingBroadcastReceiver : BroadcastReceiver {

        public delegate Task BondedEventHandler(object source, BondingStatusEventArgs args);
        public event BondedEventHandler OnBondedStatusChanged;

        public override void OnReceive(Context context, Intent intent) {
            if (intent.Action == BluetoothDevice.ActionBondStateChanged) {
                BondedStatusChanged((BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice));
            }
        }
        protected virtual void BondedStatusChanged(BluetoothDevice device) => OnBondedStatusChanged?.Invoke(this, new BondingStatusEventArgs {
            Device = device
        });
    }
}
