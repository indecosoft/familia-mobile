using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.PressureDevice
{
    [Activity(Label = "AddNewBloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class AddNewBloodPressureDeviceActivity : AppCompatActivity
    {
        private RecyclerView _recyclerView;
        private List<string> _devices;
        private List<string> _devicesAddress;
        private const int EnableBt = 11;
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _scanner;
        private DevicesRecyclerViewAdapter _adapter;
        private ProgressBarDialog _progressBarDialog;
        private static AddNewBloodPressureDeviceActivity _context;
        private readonly BluetoothScanCallback _scanCallback = new BluetoothScanCallback();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_add_new_blood_pressure_device);

            Toolbar toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Dispozitive Bluetooth";
            _context = this;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                Finish();
            };
            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se cauta dispozitive...", this, false, null, null, null, null, "Anulare", (sender, args) => Finish());
            _progressBarDialog.Show();

            _devices = new List<string>();
            _devicesAddress = new List<string>();

            _adapter = new DevicesRecyclerViewAdapter(this, _devices);
            _adapter.ItemClick += delegate (object sender, int i)
            {
                Contract.Requires(sender != null);
                Log.Error("Address", _devicesAddress[i]);
                Utils.SetDefaults(GetString(Resource.String.blood_pressure_device), _devicesAddress[i], this);
                StartActivity(typeof(BloodPressureDeviceActivity));
                Finish();
            };

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(_adapter);

            BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);

            if (bluetoothManager != null)
            {
                _bluetoothAdapter = bluetoothManager.Adapter;
            }

            if (_bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth!", ToastLength.Short).Show();
                Finish();
            }
            else
            {
                if (!_bluetoothAdapter.IsEnabled)
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
                }
                else
                {
                    ScanDevices();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != EnableBt) return;
            if (resultCode == Result.Ok)
            {
                ScanDevices();
            }
            else
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
            }
        }

        private void ScanDevices()
        {
            _scanner = _bluetoothAdapter.BluetoothLeScanner;
            _scanner.StartScan(_scanCallback);
        }
        protected override void OnPause()
        {
            base.OnPause();
            _scanner?.StopScan(_scanCallback);
        }

        private class BluetoothScanCallback : ScanCallback
        {
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (result.Device.Name == null || _context._devicesAddress.Contains(result.Device.Address)) return;
                _context._devices.Add(result.Device.Name);
                _context._devicesAddress.Add(result.Device.Address);
                _context._adapter.NotifyDataSetChanged();
                _context._progressBarDialog.Dismiss();
            }
        }
    }
}