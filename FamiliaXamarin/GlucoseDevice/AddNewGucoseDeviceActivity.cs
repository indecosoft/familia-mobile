using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.PressureDevice;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.GlucoseDevice
{
    [Activity(Label = "AddNewGucoseDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class AddNewGucoseDeviceActivity : AppCompatActivity
    {
        private RecyclerView recyclerView;
        private BluetoothAdapter bluetoothAdapter;
        private List<string> devices;
        private List<string> devicesAddress;

        private BluetoothLeScanner scanner;
        private int ENABLE_BT = 11;
        private ProgressDialog progressDialog;

        DevicesRecyclerViewAdapter adapter;
        private BluetoothScanCallback scanCallback;

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
            scanCallback = new BluetoothScanCallback(this);

            progressDialog = new ProgressDialog(this);
            progressDialog.SetTitle("Va rugam asteptati ...");
            progressDialog.SetMessage("Se cauta dispozitive");
            progressDialog.SetCancelable(false);
            progressDialog.SetButton(-1, "Anulare", (sender, args) => Finish());

            progressDialog.Show();

            

            devices = new List<string>();
            devicesAddress = new List<string>();

            adapter = new DevicesRecyclerViewAdapter(this, devices);
            adapter.ItemClick += delegate(object sender, int i)
            {
                Utils.SetDefaults(GetString(Resource.String.blood_glucose_device), devicesAddress[i], this);
                StartActivity(typeof(GlucoseDeviceActivity));
                Finish();
            };
            recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.SetAdapter(adapter);

            BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            if (bluetoothManager != null)
            {
                bluetoothAdapter = bluetoothManager.Adapter;
            }
            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth!", ToastLength.Short).Show();
                Finish();
            }
            else
            {
                if (!bluetoothAdapter.IsEnabled)
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), ENABLE_BT);
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
            if (requestCode == ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                {
                    ScanDevices();
                }
                else
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), ENABLE_BT);
                }
            }
        }
        private void ScanDevices()
        {
            scanner = bluetoothAdapter.BluetoothLeScanner;
            scanner.StartScan(scanCallback);
        }
        protected override void OnPause()
        {
            base.OnPause();
            scanner.StopScan(scanCallback);

        }

        public class BluetoothScanCallback : ScanCallback
        {
            private Context _context;
            public BluetoothScanCallback(Context context)
            {
                _context = context;
            }
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (result.Device.Name != null)
                {
                    if (!((AddNewGucoseDeviceActivity)_context).devicesAddress.Contains(result.Device.Address))
                    {
                        (_context as AddNewGucoseDeviceActivity)?.devices.Add(result.Device.Name);
                        (_context as AddNewGucoseDeviceActivity)?.devicesAddress.Add(result.Device.Address);
                        (_context as AddNewGucoseDeviceActivity)?.adapter.NotifyDataSetChanged();
                        (_context as AddNewGucoseDeviceActivity)?.progressDialog.Dismiss();
                    }
                }
            }
        }

        private bool alreadyExist(string e)
        {
            for (int i = 0; i < devicesAddress.Count; i++)
            {
                if (e.CompareTo(devicesAddress[i]) == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}