using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Familia.Devices.Models;
using Newtonsoft.Json;

namespace Familia.Devices.DevicesManagement.DeviceManufactureSelector {
    [Activity(Label = "DeviceSelectorActivity")]

    public class DeviceManufactureSelectorActivity : AppCompatActivity {
        List<SupportedDeviceModel> _list = new List<SupportedDeviceModel>();
        DeviceManufactureSelectorRecyclerViewAdapter _mAdapter;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            InitUi();

            _list = JsonConvert.DeserializeObject<List<SupportedDeviceModel>>(Intent.GetStringExtra("Items"));
            _list = _list.OrderByDescending(el => el.DeviceName).ToList();

            _mAdapter = new DeviceManufactureSelectorRecyclerViewAdapter(this, _list);
            _mAdapter.ItemClick += ItemClick;
            var recyclerView = FindViewById<RecyclerView>(Resource.Id.devices);
            var etSearch = FindViewById<AppCompatEditText>(Resource.Id.et_search);

            etSearch.TextChanged += EtSearch_TextChanged;
            var layoutManager = new LinearLayoutManager(this) { Orientation = LinearLayoutManager.Vertical };
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.SetAdapter(_mAdapter);
            recyclerView.AddItemDecoration(new DividerItemDecoration(recyclerView.Context, DividerItemDecoration.Vertical));
            // Create your application here
        }

        private void EtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            _mAdapter.Search(e.Text.ToString());
        }

        private void ItemClick(object sender, DeviceSelectorAdapterClickEventArgs e) {
            var returnIntent = new Intent();
            returnIntent.PutExtra("result", JsonConvert.SerializeObject(_mAdapter.GetItem(e.Position)));
            SetResult(Result.Ok, returnIntent);
            Finish();
        }
        public override void OnBackPressed() {
            SetResult(Result.Canceled);
            Finish();
        }
        private void InitUi() {
            SetContentView(Resource.Layout.activity_device_selector);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += (sender, args) => OnBackPressed();
            Title = "Inapoi";
        }
    }
}
