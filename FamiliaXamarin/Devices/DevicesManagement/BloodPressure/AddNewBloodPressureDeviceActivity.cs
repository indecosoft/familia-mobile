using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.Bluetooth.Callbacks;
using Familia.Devices.Bluetooth.Events;
using Familia.Devices.Helpers;
using Familia.Devices.PressureDevice;
using Familia.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices.DevicesManagement.BloodPressure {
	[Activity(Label = "AddNewBloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class AddNewBloodPressureDeviceActivity : AppCompatActivity {
		private RecyclerView _recyclerView;
		private const int EnableBt = 11;
		private BluetoothAdapter _bluetoothAdapter;
		private BluetoothLeScanner _scanner;
		private DevicesRecyclerViewAdapter _adapter;
		private ProgressBarDialog _progressBarDialog;
		private readonly BluetoothScanCallback _scanCallback = new BluetoothScanCallback();
		private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_add_new_blood_pressure_device);

			var toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			Title = "Dispozitive Bluetooth";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			toolbar.NavigationClick += delegate { Finish(); };
			if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted) {
				RequestPermissions(Constants.PermissionsArray, 0);
			} else {
				StartDicovery();
			}
		}

		private void StartDicovery() {
			_progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se cauta dispozitive...", this, false,
				null, null, null, null, "Anulare", (sender, args) => Finish());
			_progressBarDialog.Show();

			_scanCallback.OnScanResultChanged += OnScanResult;

			_adapter = new DevicesRecyclerViewAdapter();
			_adapter.ItemClick += async (sender, args) => {
				_bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
				await _bleDevicesRecords.Insert(new BluetoothDeviceRecords {
					Name = _adapter.GetItem(args.Position).Name,
					Address = _adapter.GetItem(args.Position).Address,
					DeviceType = DeviceType.BloodPressure
				});
				if (!Intent.GetBooleanExtra("RegisterOnly", false)) {
					StartActivity(typeof(BloodPressureDeviceActivity));
				}

				Finish();
			};

			_recyclerView = FindViewById<RecyclerView>(Resource.Id.addNewDeviceRecyclerView);
			_recyclerView.SetLayoutManager(new LinearLayoutManager(this));
			_recyclerView.SetAdapter(_adapter);

			var bluetoothManager = (BluetoothManager) GetSystemService(BluetoothService);

			if (bluetoothManager != null) {
				_bluetoothAdapter = bluetoothManager.Adapter;
			}

			if (_bluetoothAdapter == null) {
				Toast.MakeText(this, "Dispozitivul nu suporta Bluetooth Low Energy!", ToastLength.Short).Show();
				Finish();
			} else {
				if (!_bluetoothAdapter.IsEnabled) {
					StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
				} else {
					ScanDevices();
				}
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
			if (requestCode != EnableBt) return;
			if (resultCode == Result.Ok) {
				ScanDevices();
			} else {
				StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), EnableBt);
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