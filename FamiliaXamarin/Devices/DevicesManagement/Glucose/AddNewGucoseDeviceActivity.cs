using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.Bluetooth.Callbacks;
using Familia.Devices.Bluetooth.Events;
using Familia.Devices.Bluetooth.Receivers;
using Familia.Devices.Helpers;
using Familia.Devices.Models;
using Familia.Helpers;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices.DevicesManagement.Glucose {
	[Activity(Label = "AddNewGucoseDeviceActivity", Theme = "@style/AppTheme.Dark",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class AddNewGlucoseDeviceActivity : AppCompatActivity {
		private RecyclerView _recyclerView;
		private BluetoothAdapter _bluetoothAdapter;

		private BluetoothLeScanner _scanner;
		private const int EnableBt = 11;
		private ProgressBarDialog _progressBarDialog;
		private readonly DevicesRecyclerViewAdapter _adapter = new DevicesRecyclerViewAdapter();
		private readonly BluetoothScanCallback _scanCallback = new BluetoothScanCallback();
		private readonly BondingBroadcastReceiver _bondingBroadcastReceiver = new BondingBroadcastReceiver();
		private SupportedDeviceModel _device;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.add_new_blood_glucose_device);
			var toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			Title = "Dispozitive Bluetooth";

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			toolbar.NavigationClick += (sender, args) => Finish();
			_device = JsonConvert.DeserializeObject<SupportedDeviceModel>(Intent.GetStringExtra("Device"));

			if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted) {
				RequestPermissions(Constants.PermissionsArray, 0);
			} else {
				StartDicovery();
			}
		}

		private void StartDicovery() {
			_scanCallback.OnScanResultChanged += OnScanResult;

			_progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se cauta dispozitive...", this, false,
				null, null, null, null, "Anulare", (sender, args) => Finish());
			_progressBarDialog.Show();
			RegisterReceiver(_bondingBroadcastReceiver,
				new IntentFilter(BluetoothDevice.ActionBondStateChanged) {
					Priority = (int) IntentFilterPriority.HighPriority
				});
			_bondingBroadcastReceiver.OnBondedStatusChanged += BondedStatusChanged;
			_adapter.ItemClick += async (sender, args) => {
				switch (_device.Manufacturer) {
					case SupportedManufacturers.Medisana:
						var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
						await bleDevicesRecords.Insert(new BluetoothDeviceRecords {
							Name = _adapter.GetItem(args.Position).Name,
							Address = _adapter.GetItem(args.Position).Address,
							DeviceType = DeviceType.Glucose,
							DeviceManufacturer = _device.Manufacturer
						});
						Finish();
						break;
					case SupportedManufacturers.Caresens:
						_adapter.GetItem(args.Position).CreateBond();
						break;
					default:
						Log.Error("Error", "Unsupported device");
						break;
				}
			};
			_recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
			_recyclerView.SetLayoutManager(new LinearLayoutManager(this));
			_recyclerView.SetAdapter(_adapter);

			var bluetoothManager = (BluetoothManager) GetSystemService(BluetoothService);
			if (bluetoothManager != null) {
				_bluetoothAdapter = bluetoothManager.Adapter;
			}

			if (_bluetoothAdapter == null) {
				Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth!", ToastLength.Short).Show();
				Finish();
			} else {
				if (!_bluetoothAdapter.IsEnabled) {
					StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
				} else {
					ScanDevices();
				}
			}
		}

		private async Task BondedStatusChanged(object source, BondingStatusEventArgs args) {
			switch (args.Device.BondState) {
				case Bond.None:
					Log.Error("Bonding State", "Failed to bond");
					break;
				case Bond.Bonding:
					Log.Error("Bonding State", "Bonding......");
					break;
				case Bond.Bonded:
					Log.Error("Bonding State", "Bonded");
					await (await SqlHelper<BluetoothDeviceRecords>.CreateAsync()).Insert(new BluetoothDeviceRecords {
						Name = args.Device.Name,
						Address = args.Device.Address,
						DeviceType = DeviceType.Glucose,
						DeviceManufacturer = _device.Manufacturer
					});
					Finish();
					break;
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
			Permission[] grantResults) {
			if (grantResults[0] != Permission.Granted) {
				Toast.MakeText(this, "Permisiuni pentru telefon refuzate", ToastLength.Short).Show();
			}

			if (grantResults[3] != Permission.Granted) {
				Toast.MakeText(this, "Permisiuni pentru camera refuzate", ToastLength.Short).Show();
			}

			if (grantResults[4] != Permission.Granted || grantResults[5] != Permission.Granted) {
				Toast.MakeText(this, "Permisiuni pentru stocare refuzate", ToastLength.Short).Show();
			}

			if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted) {
				Toast.MakeText(this, "Permisiuni pentru locatie refuzate", ToastLength.Short).Show();
				Finish();
			} else {
				StartDicovery();
			}
		}

		private void OnScanResult(object source, BluetoothScanCallbackEventArgs args) {
			if (args.Result.Device.Name == null) return;
			_adapter.Add(args.Result.Device);
			_progressBarDialog.Dismiss();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == EnableBt) {
				if (resultCode == Result.Ok) {
					ScanDevices();
				} else {
					StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
				}
			}
		}

		private void ScanDevices() {
			_scanner = _bluetoothAdapter.BluetoothLeScanner;
			_scanner.StartScan(_scanCallback);
		}

		protected override void OnPause() {
			base.OnPause();
			_scanner?.StopScan(_scanCallback);
		}
	}
}