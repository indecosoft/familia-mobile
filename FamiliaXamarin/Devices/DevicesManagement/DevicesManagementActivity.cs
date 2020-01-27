using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using Familia.Devices.DevicesManagement.Dialogs.DialogEvents;
using Familia.Devices.DevicesManagement.Dialogs.DialogHelpers;
using Familia.Devices.Helpers;
using Familia.Devices.Models;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Helpers;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices {
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class DevicesManagementActivity : AppCompatActivity {
        private RecyclerView _recyclerViewDevices;
        private Button _btnAddDevice;
        private List<DeviceEditingManagementModel> _devicesList = new List<DeviceEditingManagementModel>();
        private DevicesManagementAdapter _adapter;
        private SqlHelper<BluetoothDeviceRecords> _sqlHelper;


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            InitUi();
        }

        private async void InitUi() {
            SetContentView(Resource.Layout.activity_devices_management);
            _btnAddDevice = FindViewById<Button>(Resource.Id.btn_add_device);
            _recyclerViewDevices = FindViewById<RecyclerView>(Resource.Id.rv_available_devices);
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Dispozitive Bluetooth";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                OnBackPressed();
            };

            await InitDatabaseConnection();
            InitEvents();
        }

        protected override async void OnResume() {
            base.OnResume();
            ClearRecyclerView();
            await InitDatabaseConnection();
        }

        private void InitEvents() {
            _btnAddDevice.Click += (sender, args) => {

                var cdd = new AddNewDeviceDialog(this);
                cdd.DialogState += delegate (object o, DialogStateEventArgs eventArgs) {
                    //if (eventArgs.Status != DialogStateEventArgs.DialogStatus.Dismissed) return;
                    if (eventArgs.Status == DialogStatuses.Dismissed && eventArgs.DeviceType != DeviceType.Unknown) {
                        Task.Run( () => {
                            RunOnUiThread(async () => {
                                ClearRecyclerView();
                                await InitDatabaseConnection();
                            });
                            Intent intent = new Intent(this, typeof(DeviceManufactureSelectorActivity));
                            var list = new List<SupportedDeviceModel>();
                            if (eventArgs.DeviceType == DeviceType.BloodPressure) {
                                list = SupportedDevices.BloodPressureDevices;
                            } else if (eventArgs.DeviceType == DeviceType.Glucose) {
                                list = SupportedDevices.GlucoseDevices;
                            }
                            
                            intent.PutExtra("Items", JsonConvert.SerializeObject(list));
                            RunOnUiThread(() => {
                                StartActivityForResult(intent, 1);
                            });
                        });
                    }
                };
                cdd.Show();

                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            };
        }

        private void ClearRecyclerView() {
            _devicesList?.Clear();
            _adapter?.Clear();
            _adapter?.NotifyDataSetChanged();
        }

        private void LoadDataInRecyclerViewer(IReadOnlyList<BluetoothDeviceRecords> list) {
            ClearRecyclerView();
            _adapter = new DevicesManagementAdapter(_devicesList);
            _recyclerViewDevices.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerViewDevices.SetAdapter(_adapter);

            _adapter.ItemClick += (sender, args) => {
                //Toast.MakeText(this, _adapter?.GetItemModel(args.Position)?.Device?.Name, ToastLength.Short).Show();
                var model = _adapter.GetItemModel(args.Position);
                if (model == null) return;
                var cdd = new DeviceEditingDialog(this, model);
                cdd.DialogState += async delegate (object o, DialogStateEventArgs eventArgs) {
                    if (eventArgs.Status != DialogStatuses.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            };
            _adapter.ItemLongClick += delegate (object sender, DevicesManagementAdapterClickEventArgs args) {
                var model = _adapter.GetItemModel(args.Position);
                if (model == null) return;
                var cdd = new DeleteDeviceDialog(this, model);
                cdd.DialogState += async delegate (object o, DialogStateEventArgs eventArgs) {
                    if (eventArgs.Status != DialogStatuses.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);


            };

            var divType = string.Empty;
            for (var i = 0; i < list.Count(); i++) {
                if (!divType.Equals(list[i].DeviceType)) {
                    _adapter.AddMessage(new DeviceEditingManagementModel {
                        ItemType = 0,
                        ItemValue = list[i].DeviceType,
                        Device = list[i]
                    });
                    _adapter.NotifyDataSetChanged();
                    divType = list[i].DeviceType;
                    i--;
                } else {
                    _adapter.AddMessage(new DeviceEditingManagementModel {
                        ItemType = 1,
                        ItemValue = list[i].Name,
                        Device = list[i]
                    });
                    _adapter.NotifyDataSetChanged();
                }
            }
        }
        private async Task InitDatabaseConnection() {
            _sqlHelper =
               await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list =
                (await _sqlHelper.QueryValuations(
                    "select * from BluetoothDeviceRecords order by DeviceType, Name")).ToList();

            LoadDataInRecyclerViewer(list);
        }

  
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
            if(resultCode == Result.Ok) {
                Log.Error("RequestCode", requestCode.ToString());
                if (requestCode == 1) {
                    var item = JsonConvert.DeserializeObject<SupportedDeviceModel>(data.GetStringExtra("result"));
                    if (item.DeviceType == DeviceType.Glucose) {
                        var intent = new Intent(this, typeof(AddNewGlucoseDeviceActivity));
                        intent.PutExtra("RegisterOnly", true);
                        intent.PutExtra("Device", data.GetStringExtra("result"));
                        if (item.Manufacturer == SupportedManufacturers.Medisana) {

                            Log.Error("Selected Device", "You have selected Medisana Glucometer");
                        }
                        StartActivity(intent);

                    } else if (item.DeviceType == DeviceType.BloodPressure) {
                        var intent = new Intent(this, typeof(AddNewBloodPressureDeviceActivity));
                        intent.PutExtra("RegisterOnly", true);
                        StartActivity(intent);
                    }
                }
            }
            
        }
    }
}