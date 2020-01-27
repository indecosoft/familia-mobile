
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Familia.Devices.Helpers;
using Familia.Devices.Models;
using FamiliaXamarin;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices {
    [Activity(Label = "DeviceSelectorActivity")]

    public class DeviceManufactureSelectorActivity : AppCompatActivity {
        List<SupportedDeviceModel> list = new List<SupportedDeviceModel>();
        DeviceManufactureSelectorRecyclerViewAdapter mAdapter;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            InitUI();

            list = JsonConvert.DeserializeObject<List<SupportedDeviceModel>>(Intent.GetStringExtra("Items"));
            list = list.OrderByDescending(el => el.DeviceName).ToList();

            mAdapter = new DeviceManufactureSelectorRecyclerViewAdapter(this, list);
            mAdapter.ItemClick += ItemClick;
            var recyclerView = FindViewById<RecyclerView>(Resource.Id.devices);
            var etSearch = FindViewById<AppCompatEditText>(Resource.Id.et_search);

            etSearch.TextChanged += EtSearch_TextChanged;
            var layoutManager = new LinearLayoutManager(this) { Orientation = LinearLayoutManager.Vertical };
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.SetAdapter(mAdapter);
            recyclerView.AddItemDecoration(new DividerItemDecoration(recyclerView.Context, DividerItemDecoration.Vertical));
            // Create your application here
        }

        private void EtSearch_TextChanged(object sender, Android.Text.TextChangedEventArgs e) {
            mAdapter.Search(e.Text.ToString());
        }

        private void ItemClick(object sender, DeviceSelectorAdapterClickEventArgs e) {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("result", JsonConvert.SerializeObject(mAdapter.GetItem(e.Position)));
            SetResult(Result.Ok, returnIntent);
            Finish();
        }
        public override void OnBackPressed() {
            SetResult(Result.Canceled);
            Finish();
        }
        private void InitUI() {
            SetContentView(Resource.Layout.activity_device_selector);
            // Create your application here
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;
            //SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = "Inapoi";
        }
    }
}
