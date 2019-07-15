using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Diagnostics.Contracts;
using Android.Preferences;
using Android.Support.V4.Hardware.Fingerprint;
using Familia;
using Familia.Devices;
using FamiliaXamarin.Devices;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie;
using Familia.Settings;

namespace FamiliaXamarin.Settings
{
    public class SettingsFragment : Android.Support.V4.App.Fragment
    {
        private Spinner spinner;
        private int optionOfSnooze;
//        private string key;
        private Switch enablefingerprint;
        private Switch enablePin;
        private TextView _tvDevicesManagement;
        private TextView _tvMedicineTitle;
        private RelativeLayout _rlMedicineTitle;

        private TextView _tvDeviceTitle;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v =  inflater.Inflate(Resource.Layout.fragment_settings, container, false);
            spinner = (Spinner)v.FindViewById(Resource.Id.alarmSpinner);
            SetupSpinner(v);
            enablefingerprint = v.FindViewById<Switch>(Resource.Id.fingerPrintSwitch);
            enablePin = v.FindViewById<Switch>(Resource.Id.pin_switch);
            _tvDevicesManagement = v.FindViewById<TextView>(Resource.Id.devices);
            _rlMedicineTitle = v.FindViewById<RelativeLayout>(Resource.Id.medicine_relative);
            _tvDeviceTitle = v.FindViewById<TextView>(Resource.Id.tv_devices);
            _tvMedicineTitle = v.FindViewById<TextView>(Resource.Id.tv_medicine);
            _tvDevicesManagement.Click += (sender, args) =>
                Activity.StartActivity(typeof(DevicesManagementActivity));
            FingerprintManagerCompat checkHardware;

            checkHardware = FingerprintManagerCompat.From(Activity);


            bool fingerprint = Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

            if (!checkHardware.IsHardwareDetected)
                enablefingerprint.Enabled = false;

            enablefingerprint.Checked = fingerprint;
            enablePin.Checked = !string.IsNullOrEmpty(Utils.GetDefaults("UserPin"));
            enablefingerprint.CheckedChange += Enablefingerprint_CheckedChange;
            enablePin.CheckedChange += EnablePin_CheckedChange;
            if (int.Parse(Utils.GetDefaults("UserType")) == 2 || int.Parse(Utils.GetDefaults("UserType")) == 1)
            {
                _tvDevicesManagement.Visibility = ViewStates.Gone;
                _tvDeviceTitle.Visibility = ViewStates.Gone;

            }
            if (int.Parse(Utils.GetDefaults("UserType")) == 2)
            {
                spinner.Visibility = ViewStates.Gone;
                _rlMedicineTitle.Visibility = ViewStates.Gone;
                _tvMedicineTitle.Visibility = ViewStates.Gone;
            }
            return v;
        }

        void EnablePin_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (enablePin.Checked)
                Activity.StartActivity(typeof(ActivitySetPin));
            else
                Utils.SetDefaults("UserPin", string.Empty);
        }


        private void SetupSpinner(View v)
        {
           
            spinner.ItemSelected += delegate (object sender, AdapterView.ItemSelectedEventArgs args)
                {
                    Contract.Requires(sender != null);
                    optionOfSnooze = args.Position;
                    

                    switch (optionOfSnooze)
                    {
                        case 0:
                            Utils.SetDefaults("snooze", "5");
                            break;
                        case 1:
                            Utils.SetDefaults("snooze", "10");
                            break;
                        case 2:
                            Utils.SetDefaults("snooze", "15");
                            break;
                        case 3:
                            Utils.SetDefaults("snooze", "30");
                            break;
                    }
                };


            string[] categories = {"5 min", "10 min", "15 min", "30 min"};

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerItem, categories);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }
        private void Enablefingerprint_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // this is an Activity
            Utils.SetDefaults("fingerprint",
                enablefingerprint.Checked ? true.ToString() : false.ToString());
            if(enablefingerprint.Checked && !enablePin.Checked)
            {
                enablePin.Checked = true;
            }
        }
    }
}