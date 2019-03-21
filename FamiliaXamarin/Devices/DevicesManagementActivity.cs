using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices;
using FamiliaXamarin.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices
{
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class DevicesManagementActivity : AppCompatActivity
    {
        private RecyclerView _recyclerViewDevices;
        private Button _btnAddDevice;
        private List<DevicesManagementModel> _devicesList = new List<DevicesManagementModel>();
        private DevicesManagementAdapter _adapter;
        private SqlHelper<BluetoothDeviceRecords> _sqlHelper;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitUi(); 
        }

        private async void InitUi()
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
            
            await InitDatabaseConnection();
            InitEvents();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            ClearRecyclerView();
            await InitDatabaseConnection();
        }

        private void InitEvents()
        {
            _btnAddDevice.Click += (sender, args) =>
            {
                
                var cdd = new AddNewDeviceDialog(this);
                cdd.DialogState += async delegate(object o, DialogStateEventArgs eventArgs)
                {
                    if (eventArgs.Status != DialogStateEventArgs.DialogStatus.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
       
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            };
        }

        private void ClearRecyclerView()
        {
            _devicesList?.Clear();
            _adapter?.Clear();
            _adapter?.NotifyDataSetChanged();
        }

        private void LoadDataInRecyclerViewer(IReadOnlyList<BluetoothDeviceRecords> list)
        {
            ClearRecyclerView();
            _adapter  = new DevicesManagementAdapter(_devicesList);
            _recyclerViewDevices.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerViewDevices.SetAdapter(_adapter);
            
            _adapter.ItemClick += (sender, args) =>
            {
                //Toast.MakeText(this, _adapter?.GetItemModel(args.Position)?.Device?.Name, ToastLength.Short).Show();
                var model = _adapter.GetItemModel(args.Position);
                if (model == null) return;
                var cdd = new DeviceManagementDialog(this, model);
                cdd.DialogState += async delegate(object o, DialogStateEventArgs eventArgs)
                {
                    if (eventArgs.Status !=
                        DialogStateEventArgs.DialogStatus.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
            };
            _adapter.ItemLongClick += delegate (object sender, DevicesManagementAdapterClickEventArgs args)
            {
                var model = _adapter.GetItemModel(args.Position);
                if (model == null) return;
                var cdd = new DeleteDeviceDialog(this, model);
                cdd.DialogState += async delegate (object o, DialogStateEventArgs eventArgs)
                {
                    if (eventArgs.Status !=
                        DialogStateEventArgs.DialogStatus.Dismissed) return;
                    ClearRecyclerView();
                    await InitDatabaseConnection();
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);


            };
            
            var divType = string.Empty;
            for (var i = 0; i < list.Count(); i++)
            {
                if (!divType.Equals(list[i].DeviceType))
                {
                    _adapter.AddMessage(new DevicesManagementModel
                    {
                        ItemType = 0,
                        ItemValue = list[i].DeviceType,
                        Device = list[i]
                    });
                    _adapter.NotifyDataSetChanged();
                    divType = list[i].DeviceType;
                    i--;
                }
                else
                {
                    _adapter.AddMessage(new DevicesManagementModel
                    {
                        ItemType = 1,
                        ItemValue = list[i].Name,
                        Device = list[i]
                    });
                    _adapter.NotifyDataSetChanged();
                }
            }
        }
        private async Task InitDatabaseConnection()
        {
             _sqlHelper =
                await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list =
                (await _sqlHelper.QueryValuations(
                    "select * from BluetoothDeviceRecords order by DeviceType, Name")).ToList();

            LoadDataInRecyclerViewer(list);
            
            
        }
    }
}