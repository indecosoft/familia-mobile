using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using Familia;
using Familia.Devices;
using Familia.Devices.BluetoothEvents;
using Familia.Devices.BroadcastReceivers;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.GlucoseDevice {
    [Activity(Label = "AddNewGucoseDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddNewGlucoseDeviceActivity : AppCompatActivity {
        private RecyclerView _recyclerView;
        private BluetoothAdapter _bluetoothAdapter;

        private BluetoothLeScanner _scanner;
        private const int EnableBt = 11;
        private ProgressBarDialog _progressBarDialog;
        private readonly DevicesRecyclerViewAdapter _adapter = new DevicesRecyclerViewAdapter();
        private readonly BluetoothScanCallback _scanCallback = new BluetoothScanCallback();
        private readonly BondingBroadcastReceiver bondingBroadcastReceiver = new BondingBroadcastReceiver();

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.add_new_blood_glucose_device);
            Toolbar toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Dispozitive Bluetooth";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                Finish();
            };
            _scanCallback.OnScanResultChanged += OnScanResult;
            
            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se cauta dispozitive...", this, false, null, null, null, null, "Anulare", (sender, args) => Finish());
            _progressBarDialog.Show();
            RegisterReceiver(bondingBroadcastReceiver, new IntentFilter(BluetoothDevice.ActionBondStateChanged) {
                Priority = (int)IntentFilterPriority.HighPriority
            });
            bondingBroadcastReceiver.OnBondedStatusChanged += BondedStatusChanged;
            _adapter.ItemClick += delegate (object sender, DevicesRecyclerViewAdapterClickEventArgs args) {
                _adapter.GetItem(args.Position).CreateBond();
            };
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(_adapter);

            var bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            if (bluetoothManager != null) {
                _bluetoothAdapter = bluetoothManager.Adapter;
            }
            if (_bluetoothAdapter == null) {
                Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth!", ToastLength.Short).Show();
                Finish();
            } else {
                if (!_bluetoothAdapter.IsEnabled) {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
                } else {
                    ScanDevices();
                }
            }
        }

        private async Task BondedStatusChanged(object source, BondingStatusEventArgs args) {
            switch (args.Device.BondState) {
                case Bond.None:
                    Log.Error("Bonding State", "Failed to bond");
                    break;
                case Bond.Bonding:
                    Log.Error("Bonding State", "Bonding......");
                    break;
                case Bond.Bonded:
                    Log.Error("Bonding State", "Bonded");
                    await (await SqlHelper<BluetoothDeviceRecords>.CreateAsync()).Insert(
                       new BluetoothDeviceRecords {
                           Name = args.Device.Name,
                           Address = args.Device.Address,
                           DeviceType = GetString(Resource.String.blood_glucose_device)
                       });
                    Finish();
                    break;
            }
        }

        private void OnScanResult(object source, BluetoothScanCallbackEventArgs args) {
            if (args.Result.Device.Name == null) return;
            _adapter.Add(args.Result.Device);
            _progressBarDialog.Dismiss();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == EnableBt) {
                if (resultCode == Result.Ok) {
                    ScanDevices();
                } else {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
                }
            }
        }

        private void ScanDevices() {
            _scanner = _bluetoothAdapter.BluetoothLeScanner;
            _scanner.StartScan(_scanCallback);
        }
        protected override void OnPause() {
            base.OnPause();
            _scanner?.StopScan(_scanCallback);

        }
    }

}
