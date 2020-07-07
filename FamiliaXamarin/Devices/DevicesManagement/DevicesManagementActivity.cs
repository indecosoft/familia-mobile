using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.DevicesAsistent;
using Familia.Devices.DevicesManagement.BloodPressure;
using Familia.Devices.DevicesManagement.DeviceManufactureSelector;
using Familia.Devices.DevicesManagement.Dialogs;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.DevicesManagement.Dialogs.Models;
using Familia.Devices.DevicesManagement.Glucose;
using Familia.Devices.Helpers;
using Familia.Devices.Models;
using Familia.Helpers;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices.DevicesManagement {
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class DevicesManagementActivity : AppCompatActivity {
        private RecyclerView _recyclerViewDevices;
        private Button _btnAddDevice;
        private List<DeviceEditingManagementModel> _devicesList = new List<DeviceEditingManagementModel>();
        private DevicesManagementAdapter _adapter;
        private SqlHelper<BluetoothDeviceRecords> _sqlHelper;
        public static readonly string DevicesRoot = "DevicesRoot";

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

        //public override void OnBackPressed()
        //{
        //    base.OnBackPressed();

        //    var intent = new Intent(this, typeof(MainActivity));
        //    intent.PutExtra(DevicesRoot, int.Parse(Utils.GetDefaults("UserType")));
        //    StartActivity(intent);

        //   /* if (int.Parse(Utils.GetDefaults("UserType")) == 2) {

        //        SupportFragmentManager.BeginTransaction()
        //                .Replace(Resource.Id.fragment_container, new AsistentHealthDevicesFragment())
        //                .AddToBackStack(null).Commit();
        //    }
        //    if (int.Parse(Utils.GetDefaults("UserType")) == 3 || int.Parse(Utils.GetDefaults("UserType")) == 4) {
        //        SupportFragmentManager.BeginTransaction()
        //                .Replace(Resource.Id.fragment_container, new HealthDevicesFragment()).AddToBackStack(null)
        //                .Commit();
        //    }*/
              
        //}

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
                            var intent = new Intent(this, typeof(DeviceManufactureSelectorActivity));
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

            var divType = DeviceType.Unknown;
            foreach (BluetoothDeviceRecords device in list) {
                if (!divType.Equals(device.DeviceType)) {
                    _devicesList.Add(new DeviceEditingManagementModel {
                        ItemType = 0,
                        Device = device
                    });
                    divType = device.DeviceType;

                    _devicesList.Add(new DeviceEditingManagementModel {
                        ItemType = 1,
                        Device = device
                    });
                } else {
                    _devicesList.Add(new DeviceEditingManagementModel {
                        ItemType = 1,
                        Device = device
                    });
                }
            }

            _adapter = new DevicesManagementAdapter(_devicesList);
            _recyclerViewDevices.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerViewDevices.SetAdapter(_adapter);

            _adapter.ItemClick += (sender, args) => {
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
            _adapter.ItemLongClick += (sender, args) => {
                var model = _adapter.GetItemModel(args.Position);
                if (model == null) return;
                var cdd = new DeleteDeviceDialog(this, model);
                cdd.DialogState += async (o, eventArgs) => {
                    if (eventArgs.Status != DialogStatuses.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            };
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