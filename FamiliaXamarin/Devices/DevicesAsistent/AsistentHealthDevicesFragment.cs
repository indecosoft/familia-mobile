using System;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.DevicesManagement;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;
using Familia.Devices.PressureDevice;
using Familia.Helpers;
using Fragment = Android.Support.V4.App.Fragment;
using Result = ZXing.Result;

namespace Familia.Devices.DevicesAsistent {
	public class AsistentHealthDevicesFragment : Fragment {
		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			View view = inflater.Inflate(Resource.Layout.fragment_asistent_health_devices, container, false);
			view.FindViewById<Button>(Resource.Id.BloodPressureButton).Click += BloodPressure;
			view.FindViewById<Button>(Resource.Id.BloodGlucoseButton).Click += Glucose;

			return view;
		}

		private void Glucose(object sender, EventArgs e) {
			if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted &&
			    Activity.CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) {
				RequestPermissions(_permissionsArray, 2);
			} else {
				StartNewActivity(typeof(GlucoseDeviceActivity), DeviceType.Glucose);
			}
		}

		private void BloodPressure(object sender, EventArgs e) {
			if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted &&
			    Activity.CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) {
				RequestPermissions(_permissionsArray, 1);
			} else {
				StartNewActivity(typeof(BloodPressureDeviceActivity), DeviceType.BloodPressure);
			}
		}

		private async void StartNewActivity(Type activity, DeviceType deviceType) {
			string text = await ScanQrCode();
			if (string.IsNullOrEmpty(text)) return;
			var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
			var list = await bleDevicesRecords.QueryValuations("SELECT * FROM BluetoothDeviceRecords");
			if (list.Where(d => d.DeviceType == deviceType).Count() > 0) {
				var intent = new Intent(Activity, activity);
				intent.PutExtra("Data", text);
				Activity.StartActivity(intent);
			} else {
				using AlertDialog alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
				alertDialog.SetTitle("Avertisment");

				alertDialog.SetMessage("Nu aveti niciun dispozitiv inregistrat!");
				alertDialog.SetButton("OK", delegate { Activity.StartActivity(typeof(DevicesManagementActivity)); });
				alertDialog.Show();
			}
		}

		private readonly string[] _permissionsArray = {
			Manifest.Permission.ReadPhoneState, Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation, Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage,
			Manifest.Permission.WriteExternalStorage
		};

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
			Permission[] grantResults) {
			if (grantResults[0] != Permission.Granted) {
				Toast.MakeText(Activity, "Permisiuni pentru telefon refuzate", ToastLength.Short).Show();
			} else if (grantResults[3] != Permission.Granted) {
				Toast.MakeText(Activity, "Permisiuni pentru camera refuzate", ToastLength.Short).Show();
			}

			if (grantResults[3] != Permission.Granted && grantResults[1] == Permission.Granted &&
			    grantResults[2] == Permission.Granted) {
				switch (requestCode) {
					case 1:
						StartNewActivity(typeof(BloodPressureDeviceActivity), DeviceType.BloodPressure);
						break;
					case 2:
						StartNewActivity(typeof(GlucoseDeviceActivity), DeviceType.Glucose);
						break;
					default:
						Log.Error("RequestCode inexistent", requestCode.ToString());
						break;
				}
			} else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted) {
				Toast.MakeText(Activity, "Permisiuni pentru locatie refuzate", ToastLength.Short).Show();
			}
		}

		public async Task<string> ScanQrCode() {
			try {
				Result qrCodeData = await Utils.ScanQrCode(Activity);
				Log.Error("QrCodeData", qrCodeData.Text);
				return qrCodeData is null ? null : qrCodeData.Text;
			} catch (Exception) {
				return null;
			}
		}
	}
}