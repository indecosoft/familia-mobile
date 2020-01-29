using System;
using System.IO;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.CustomTabs;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia;
using Familia.Devices;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Devices.SmartBand;
using FamiliaXamarin.Helpers;
using SQLite;
using Resource = Familia.Resource;

namespace FamiliaXamarin.Devices {
    public class HealthDevicesFragment : Android.Support.V4.App.Fragment, View.IOnClickListener {
        private enum DeviceType {
            FitBit=1,
            BLEBloodPressureDevice = 2,
            BLEGlucoseDevice = 3
        }
        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }
        private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
        private async void StartNewActivity(Type newActivity, string button, DeviceType devicetype) {
            _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list = await _bleDevicesRecords.QueryValuations(
                "select * from BluetoothDeviceRecords");
            if ((from c in list
                 where c.DeviceType == button
                 select new { c.Name, c.Address, c.DeviceType }).Any()) {
                Activity.StartActivity(newActivity);
                //Activity.StartActivity(typeof(AddNewGucoseDeviceActivity));
            } else {
                if (devicetype == DeviceType.FitBit) {
                    const string url = "https://www.fitbit.com/oauth2/authorize?" + /*"grant_type=authorization_code"+*/
                                       "response_type=code" +
                                       "&client_id=22CZRL" +
                                       "&redirect_uri=fittauth%3A%2F%2Ffinish" +
                                       "&scope=activity%20heartrate%20location%20nutrition%20profile%20settings%20sleep%20social%20weight" +
                                       "&prompt=login" +
                                       "&expires_in=31536000";
                    var builder = new CustomTabsIntent.Builder();
                    var customTabsIntent = builder.Build();
                    customTabsIntent.LaunchUrl(Activity, Android.Net.Uri.Parse(url));
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
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_health_devices, container, false);
            InitUi(view);
            return view;
        }

        private void InitUi(View view) {
            var bloodPressureButton = view.FindViewById<Button>(Resource.Id.BloodPressureButton);
            var bloodGlucoseButton = view.FindViewById<Button>(Resource.Id.BloodGlucoseButton);
            var smartBandButton = view.FindViewById<Button>(Resource.Id.SmartbandButton);
            //        Button AddNewDeviceButton = view.findViewById(Resource.Id.AddNewDeviceButton);

            bloodPressureButton.SetOnClickListener(this);
            bloodGlucoseButton.SetOnClickListener(this);
            smartBandButton.SetOnClickListener(this);
            //        AddNewDeviceButton.setOnClickListener(this);
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
            if (grantResults[1] == Permission.Granted && grantResults[2] == Permission.Granted) {
                switch ((DeviceType)requestCode) {
                    case DeviceType.BLEBloodPressureDevice:
                        StartNewActivity(typeof(BloodPressureDeviceActivity),
                            GetString(Resource.String.blood_pressure_device), DeviceType.BLEBloodPressureDevice);
                        break;
                    case DeviceType.BLEGlucoseDevice:
                        StartNewActivity(typeof(GlucoseDeviceActivity),
                            GetString(Resource.String.blood_glucose_device), DeviceType.BLEGlucoseDevice);
                        break;
                    case DeviceType.FitBit:
                        StartNewActivity(typeof(SmartBandDeviceActivity),
                            GetString(Resource.String.smartband_device), DeviceType.FitBit);
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
                        RequestPermissions(_permissionsArray, (int)DeviceType.BLEBloodPressureDevice);
                    } else {
                        StartNewActivity(typeof(BloodPressureDeviceActivity),
                        GetString(Resource.String.blood_pressure_device), DeviceType.BLEBloodPressureDevice);
                    }
                    break;
                case Resource.Id.BloodGlucoseButton:
                    if (Activity.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted) {
                        RequestPermissions(_permissionsArray, (int)DeviceType.BLEGlucoseDevice);
                    } else {
                        StartNewActivity(typeof(GlucoseDeviceActivity),
                        GetString(Resource.String.blood_glucose_device), DeviceType.BLEGlucoseDevice);
                    }
                    break;
                case Resource.Id.SmartbandButton:
                    if (Activity.CheckSelfPermission(Manifest.Permission.Internet) != Permission.Granted) {
                        RequestPermissions(_permissionsArray, (int)DeviceType.FitBit);
                    } else {
                        StartNewActivity(typeof(SmartBandDeviceActivity),
                        GetString(Resource.String.smartband_device), DeviceType.FitBit);
                    }
                    break;
            }
        }
    }
}

