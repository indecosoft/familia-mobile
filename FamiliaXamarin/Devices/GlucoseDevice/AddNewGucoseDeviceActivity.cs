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
using Android.Widget;
using FamiliaXamarin.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.GlucoseDevice
{
    [Activity(Label = "AddNewGucoseDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class AddNewGucoseDeviceActivity : AppCompatActivity
    {
        private RecyclerView _recyclerView;
        private BluetoothAdapter _bluetoothAdapter;
        private List<string> _devices;
        private List<string> _devicesAddress;

        private BluetoothLeScanner _scanner;
        private const int EnableBt = 11;
        private ProgressBarDialog _progressBarDialog;

        private DevicesRecyclerViewAdapter _adapter;
        private BluetoothScanCallback _scanCallback;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.add_new_blood_glucose_device);

            Toolbar toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Dispozitive Bluetooth";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                Finish();
            };
            _scanCallback = new BluetoothScanCallback(this);
            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se cauta dispozitive...", this, false, null, null, null, null, "Anulare", (sender, args) => Finish());
            _progressBarDialog.Show();

            

            _devices = new List<string>();
            _devicesAddress = new List<string>();

            _adapter = new DevicesRecyclerViewAdapter(this, _devices);
            _adapter.ItemClick += delegate(object sender, int i)
            {
                Contract.Requires(sender != null);
                Utils.SetDefaults(GetString(Resource.String.blood_glucose_device), _devicesAddress[i], this);
                StartActivity(typeof(GlucoseDeviceActivity));
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
            if (requestCode == EnableBt)
            {
                if (resultCode == Result.Ok)
                {
                    ScanDevices();
                }
                else
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
                }
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
            private readonly Context _context;
            public BluetoothScanCallback(Context context)
            {
                _context = context;
            }
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (result.Device.Name == null ||
                    ((AddNewGucoseDeviceActivity) _context)._devicesAddress.Contains(result.Device.Address)) return;
                ((AddNewGucoseDeviceActivity) _context)?._devices.Add(result.Device.Name);
                ((AddNewGucoseDeviceActivity) _context)?._devicesAddress.Add(result.Device.Address);
                ((AddNewGucoseDeviceActivity) _context)?._adapter.NotifyDataSetChanged();
                ((AddNewGucoseDeviceActivity) _context)?._progressBarDialog.Dismiss();
            }
        }

    }
}