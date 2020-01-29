using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.Devices.Helpers;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Helpers;

namespace Familia.Devices.DevicesAsistent
{
    public class AsistentHealthDevicesFragment : Android.Support.V4.App.Fragment
    {


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_asistent_health_devices, container, false);
            view.FindViewById<Button>(Resource.Id.BloodPressureButton).Click += BloodPressure;
            view.FindViewById<Button>(Resource.Id.BloodGlucoseButton).Click += Glucose;

            return view;
        }

        private void Glucose(object sender, EventArgs e) {
            if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) {
                RequestPermissions(_permissionsArray, 2);
            } else {
                StartNewActivity(typeof(GlucoseDeviceActivity), GetString(Resource.String.blood_glucose_device));
            } 
        }

        private void BloodPressure(object sender, EventArgs e) {
            if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted && Activity.CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) {
                RequestPermissions(_permissionsArray, 1);
            } else {
                StartNewActivity(typeof(BloodPressureDeviceActivity), GetString(Resource.String.blood_pressure_device));
            }
        }

        private async void StartNewActivity(Type activity, string deviceType) {
            var deviceId = await scanQRCode();

            var _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list = await _bleDevicesRecords.QueryValuations(
                "select * from BluetoothDeviceRecords");
            if ((from c in list
                 where c.DeviceType == deviceType
                 select new { c.Name, c.Address, c.DeviceType }).Any()) {
                Intent intent = new Intent(Activity, activity);
                intent.PutExtra("Imei", deviceId);
                Activity.StartActivity(intent);
                //Activity.StartActivity(typeof(AddNewGucoseDeviceActivity));
            } else {

                // Do something for Oreo and above versions
                using var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
                alertDialog.SetTitle("Avertisment");

                alertDialog.SetMessage("Nu aveti niciun dispozitiv inregistrat!");
                alertDialog.SetButton("OK", delegate {
                    Activity.StartActivity(typeof(DevicesManagementActivity));
                });
                alertDialog.Show();

            }
        }

        private readonly string[] _permissionsArray =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults) {

            if (grantResults[0] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru telefon refuzate", ToastLength.Short).Show();
            } else if (grantResults[3] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru camera refuzate", ToastLength.Short).Show();
            }
            if (grantResults[3] != Permission.Granted && grantResults[1] == Permission.Granted && grantResults[2] == Permission.Granted) {
                switch (requestCode) {
                    case 1:
                        StartNewActivity(typeof(BloodPressureDeviceActivity),
                            GetString(Resource.String.blood_pressure_device));
                        break;
                    case 2:
                        StartNewActivity(typeof(GlucoseDeviceActivity),
                            GetString(Resource.String.blood_glucose_device));
                        break;
                    default:
                        Log.Error("RequestCode inexistent", requestCode.ToString());
                        break;
                }
            } else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru locatie refuzate", ToastLength.Short).Show();
            }
        }
        public async Task<string> scanQRCode() {
            try {
                var qrCodeData = await Utils.ScanQRCode(Activity);
                return qrCodeData.Text;
                Log.Error("AsistentHealthDevicesFragment", qrCodeData.Text);
                Toast.MakeText(Activity, "imei: " + qrCodeData.Text, ToastLength.Long).Show();
            }
            catch (Exception e) {
                return null;
                Log.Error("AsistentHealthDevicesFragment err", e.Message);
                Toast.MakeText(Activity, "error", ToastLength.Long).Show();
            }
        }
    }
}