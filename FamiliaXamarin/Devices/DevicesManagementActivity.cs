using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using SQLite;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices
{
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class DevicesManagementActivity : AppCompatActivity
    {
        private RecyclerView _recyclerViewDevices;
        private Button _btnAddDevice;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitUi();
            
            
        }

        private void InitUi()
        {
            SetContentView(Resource.Layout.activity_devices_management);
            _btnAddDevice = FindViewById<Button>(Resource.Id.btn_add_device);
            _recyclerViewDevices = FindViewById<RecyclerView>(Resource.Id.rv_available_devices);
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Dispozitive Bluetooth";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                OnBackPressed();
            };
            InitEvents();
            InitDatabaseConnection();
        }

        private void InitEvents()
        {
            _btnAddDevice.Click += (sender, args) => { };
        }

        private async void InitDatabaseConnection()
        {
            SqlHelper<BluetoothDeviceRecords> t =
                await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list =
                await t.QueryValuations(
                    "select * from BluetoothDeviceRecords order by DeviceType");
            var mMessages = new List<DevicesManagementModel>();
            
            var divType = string.Empty;
            foreach (var deviceRecord in list)
            {
                if (!divType.Equals(deviceRecord.DeviceType))
                {
                    mMessages.Add(new DevicesManagementModel
                    {
                        ItemType = 0,
                        ItemValue = deviceRecord.DeviceType,
                        Device = deviceRecord
                    });
                }
                else
                {
                    mMessages.Add(new DevicesManagementModel
                    {
                        ItemType = 1,
                        ItemValue = deviceRecord.Name,
                        Device = deviceRecord
                    });
                }
                
            }
            var layoutManager = new LinearLayoutManager(this)
                {Orientation = LinearLayoutManager.Vertical};
            var adapter  = new DevicesManagementRecyclerViewAdapter(this, mMessages);
            _recyclerViewDevices.SetLayoutManager(layoutManager);
            _recyclerViewDevices.SetAdapter(adapter);
        }
    }
}