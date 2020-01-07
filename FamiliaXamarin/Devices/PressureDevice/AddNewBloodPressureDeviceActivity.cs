using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Familia;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Icu.Text;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Org.Json;
using SQLite;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Familia.Devices;

namespace FamiliaXamarin.Devices.PressureDevice
{
    [Activity(Label = "AddNewBloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddNewBloodPressureDeviceActivity : AppCompatActivity
    {
        private RecyclerView _recyclerView;
        private const int EnableBt = 11;
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _scanner;
        private DevicesRecyclerViewAdapter _adapter;
        private ProgressBarDialog _progressBarDialog;
        private static AddNewBloodPressureDeviceActivity _context;
        private readonly BluetoothScanCallback _scanCallback = new BluetoothScanCallback();
        private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
        protected override async void OnCreate(Bundle savedInstanceState)
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
            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati",
                "Se cauta dispozitive...", this, false, 
                null, null, null, 
                null, "Anulare",
                (sender, args) => Finish());
            _progressBarDialog.Show();

            _scanCallback.OnScanResultChanged += OnScanResult;
            //            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //            var numeDb = "devices_data.db";
            //            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDb));
            //            await _db.CreateTableAsync<BluetoothDeviceRecords>();
            _adapter = new DevicesRecyclerViewAdapter();
            _adapter.ItemClick += async delegate (object sender, DevicesRecyclerViewAdapterClickEventArgs args)
            {
                _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                await _bleDevicesRecords.Insert(
                    new BluetoothDeviceRecords
                    {
                        Name = _adapter.GetItem(args.Position).Name, Address = _adapter.GetItem(args.Position).Address,
                        DeviceType = GetString(Resource.String.blood_pressure_device)
                    });
                if (!Intent.GetBooleanExtra("RegisterOnly", false))
                {
                    StartActivity(typeof(BloodPressureDeviceActivity));
                }
                Finish();

            };

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(_adapter);

            var bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);

            if (bluetoothManager != null)
            {
                _bluetoothAdapter = bluetoothManager.Adapter;
            }

            if (_bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth Low Energy!",
                    ToastLength.Short).Show();
                Finish();
            }
            else
            {
                if (!_bluetoothAdapter.IsEnabled)
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable),
                        EnableBt);
                }
                else
                {
                    ScanDevices();
                }
            }
        }

        private void OnScanResult(object source, Familia.Devices.BluetoothEvents.BluetoothScanCallbackEventArgs args) {
            if (args.Result.Device.Name == null) return;
            _adapter.Add(args.Result.Device);
            _progressBarDialog.Dismiss();
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

        //private class BluetoothScanCallback : ScanCallback
        //{
        //    public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        //    {
        //        base.OnScanResult(callbackType, result);
        //        if (result.Device.Name == null ||
        //            _context._devicesAddress.Contains(result.Device.Address)) return;
        //        _context._devices.Add(result.Device.Name);
        //        _context._devicesAddress.Add(result.Device.Address);
        //        _context._adapter.NotifyDataSetChanged();
        //        _context._progressBarDialog.Dismiss();
        //    }
        //}
    }
}