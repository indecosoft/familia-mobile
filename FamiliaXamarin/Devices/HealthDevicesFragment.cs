using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.CustomTabs;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.DevicesManagement;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;
using Familia.Devices.PressureDevice;
using Familia.Devices.SmartBand;
using Familia.Helpers;
using Fragment = Android.Support.V4.App.Fragment;
using Uri = Android.Net.Uri;

namespace Familia.Devices {
    public class HealthDevicesFragment : Fragment, View.IOnClickListener {
        private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
        private async void StartNewActivity(Type newActivity, DeviceType deviceType) {
            _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list = await _bleDevicesRecords.QueryValuations(
                "select * from BluetoothDeviceRecords");
            if ((from c in list
                 where c.DeviceType == deviceType
                 select new { c.Name, c.Address, c.DeviceType }).Any()) {
                Activity.StartActivity(newActivity);
            } else {
                if (deviceType == DeviceType.SmartBand) {
                    const string url = "https://www.fitbit.com/oauth2/authorize?" + /*"grant_type=authorization_code"+*/
                                       "response_type=code" +
                                       "&client_id=22CZRL" +
                                       "&redirect_uri=fittauth%3A%2F%2Ffinish" +
                                       "&scope=activity%20heartrate%20location%20nutrition%20profile%20settings%20sleep%20social%20weight" +
                                       "&prompt=login" +
                                       "&expires_in=31536000";
                    CustomTabsIntent customTabsIntent = new CustomTabsIntent.Builder().Build();
                    customTabsIntent.LaunchUrl(Activity, Uri.Parse(url));
                } else {
                    using AlertDialog alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
                    alertDialog.SetTitle("Avertisment");

                    alertDialog.SetMessage("Nu aveti niciun dispozitiv inregistrat!");
                    alertDialog.SetButton("OK", delegate {
                        Activity.StartActivity(typeof(DevicesManagementActivity));
                    });
                    alertDialog.Show();
                }
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            View view = inflater.Inflate(Resource.Layout.fragment_health_devices, container, false);
            InitUi(view);
            return view;
        }

        private void InitUi(View view) {
            var bloodPressureButton = view.FindViewById<Button>(Resource.Id.BloodPressureButton);
            var bloodGlucoseButton = view.FindViewById<Button>(Resource.Id.BloodGlucoseButton);
            var smartBandButton = view.FindViewById<Button>(Resource.Id.SmartbandButton);

            bloodPressureButton.SetOnClickListener(this);
            bloodGlucoseButton.SetOnClickListener(this);
            smartBandButton.SetOnClickListener(this);
        }
        

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults) {

            if (grantResults[0] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru telefon refuzate", ToastLength.Short).Show();
            } else if (grantResults[3] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru camera refuzate", ToastLength.Short).Show();
            }
            if (grantResults[1] == Permission.Granted && grantResults[2] == Permission.Granted) {
                switch ((DeviceType)requestCode) {
                    case DeviceType.BloodPressure:
                        StartNewActivity(typeof(BloodPressureDeviceActivity), DeviceType.BloodPressure);
                        break;
                    case DeviceType.Glucose:
                        StartNewActivity(typeof(GlucoseDeviceActivity), DeviceType.Glucose);
                        break;
                    case DeviceType.SmartBand:
                        StartNewActivity(typeof(SmartBandDeviceActivity), DeviceType.SmartBand);
                        break;
                    default:
                        Log.Error("RequestCode inexistent", requestCode.ToString());
                        break;
                }
            } else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted) {
                Toast.MakeText(Activity, "Permisiuni pentru locatie refuzate", ToastLength.Short).Show();
            }
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.BloodPressureButton:
                    if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted) {
                        RequestPermissions(Constants.PermissionsArray, (int)DeviceType.BloodPressure);
                    } else {
                        StartNewActivity(typeof(BloodPressureDeviceActivity), DeviceType.BloodPressure);
                    }
                    break;
                case Resource.Id.BloodGlucoseButton:
                    if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted) {
                        RequestPermissions(Constants.PermissionsArray, (int)DeviceType.Glucose);
                    } else {
                        StartNewActivity(typeof(GlucoseDeviceActivity), DeviceType.Glucose);
                    }
                    break;
                case Resource.Id.SmartbandButton:
                    if (Activity.CheckSelfPermission(Manifest.Permission.Internet) != Permission.Granted) {
                        RequestPermissions(Constants.PermissionsArray, (int)DeviceType.SmartBand);
                    } else {
                        StartNewActivity(typeof(SmartBandDeviceActivity), DeviceType.SmartBand);
                    }
                    break;
            }
        }
    }
}

