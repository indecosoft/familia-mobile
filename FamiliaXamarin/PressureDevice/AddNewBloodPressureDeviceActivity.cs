﻿using System;
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
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.GlucoseDevice;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.PressureDevice
{
    [Activity(Label = "AddNewBloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class AddNewBloodPressureDeviceActivity : AppCompatActivity
    {
        private RecyclerView recyclerView;
        private List<string> devices;
        private List<string> devicesAddress;
        private int ENABLE_BT = 11;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothLeScanner scanner;
        private DevicesRecyclerViewAdapter adapter;
        private ProgressDialog progressDialog;
        private static AddNewBloodPressureDeviceActivity _context;
        BluetoothScanCallback scanCallback = new BluetoothScanCallback();
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
                Log.Error("Address", devicesAddress[i]);
                Utils.SetDefaults(GetString(Resource.String.blood_pressure_device), devicesAddress[i], this);
                StartActivity(typeof(BloodPressureDeviceActivity));
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
            if (scanner != null)
            {
                scanner.StopScan(scanCallback);
            }
        }
        public class BluetoothScanCallback : ScanCallback
        {
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (result.Device.Name != null)
                {
                    if (!_context.devicesAddress.Contains(result.Device.Address))
                    {
                        _context.devices.Add(result.Device.Name);
                        _context.devicesAddress.Add(result.Device.Address);
                        _context.adapter.NotifyDataSetChanged();
                        _context.progressDialog.Dismiss();
                    }
                }
            }
        }
    }
}