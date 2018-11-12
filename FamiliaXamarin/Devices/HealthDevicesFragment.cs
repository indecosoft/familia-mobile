using System;
using Android.App;
using Android.OS;
using Android.Support.CustomTabs;
using Android.Text;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Devices.SmartBand;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin.Devices
{
    public class HealthDevicesFragment : Android.Support.V4.App.Fragment, View.IOnClickListener
    {

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }
        private void StartNewActivity(Type newActivity, string button, View v)
        {

            if (Utils.GetDefaults(button, Activity) != null)
            {
                Application.Context.StartActivity(newActivity);
                //Activity.StartActivity(typeof(AddNewGucoseDeviceActivity));
            }
            else
            {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    // Do something for Oreo and above versions
                    AlertDialog alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                    alertDialog.SetTitle(Html.FromHtml("<p style = 'text-align: center; color: #F47445;'>Avertisment</p>", FromHtmlOptions.ModeLegacy));

                    alertDialog.SetMessage(Html.FromHtml("<br/><p style = 'text-align: center; color: #000000;'>Nu aveti niciun dispozitiv inregistrat!</br>Doriti sa adaugati unul acum?</p>", FromHtmlOptions.ModeLegacy));
                    alertDialog.SetButton("OK", delegate
                    {
                        switch (v.Id)
                        {
                            case Resource.Id.BloodPressureButton:
                                Activity.StartActivity(typeof(AddNewBloodPressureDeviceActivity));
                                break;
                            case Resource.Id.BloodGlucoseButton:
                                Activity.StartActivity(typeof(AddNewGucoseDeviceActivity));
                                break;
                            case Resource.Id.SmartbandButton:
                                string url = "https://www.fitbit.com/oauth2/authorize?" +
                                             "response_type=token" +
                                             "&client_id=22CZRL" +
                                             "&redirect_uri=fittauth%3A%2F%2Ffinish" +
                                             "&scope=activity%20heartrate%20location%20nutrition%20profile%20settings%20sleep%20social%20weight" +
                                             "&prompt=login" +
                                             "&expires_in=31536000";
                                CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
                                CustomTabsIntent customTabsIntent = builder.Build();
                                customTabsIntent.LaunchUrl(Activity, Android.Net.Uri.Parse(url));
                                break;
                        }
                    });
                    alertDialog.Show();
                }

            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_health_devices, container, false);
            InitUi(view);
            return view;
        }

        private void InitUi(View view)
        {
            Button BloodPressureButton = view.FindViewById<Button>(Resource.Id.BloodPressureButton);
            Button BloodGlucoseButton = view.FindViewById<Button>(Resource.Id.BloodGlucoseButton);
            Button SmartbandButton = view.FindViewById<Button>(Resource.Id.SmartbandButton);
            //        Button AddNewDeviceButton = view.findViewById(Resource.Id.AddNewDeviceButton);

            BloodPressureButton.SetOnClickListener(this);
            BloodGlucoseButton.SetOnClickListener(this);
            SmartbandButton.SetOnClickListener(this);
            //        AddNewDeviceButton.setOnClickListener(this);
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.BloodPressureButton:
                    StartNewActivity(typeof(BloodPressureDeviceActivity), GetString(Resource.String.blood_pressure_device), v);
                    break;

                case Resource.Id.BloodGlucoseButton:
                    StartNewActivity(typeof(GlucoseDeviceActivity), GetString(Resource.String.blood_glucose_device), v);
                    break;
                case Resource.Id.SmartbandButton:
                    StartNewActivity(typeof(SmartBandDeviceActivity), GetString(Resource.String.smartband_device), v);
                    break;

            }
        }
    }
}

