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

namespace FamiliaXamarin.Settings
{
    public class SettingsFragment : Android.Support.V4.App.Fragment
    {
        private Spinner spinner;
        private int optionOfSnooze;
//        private string key;
        private Switch enablefingerprint;
        private TextView _tvDevicesManagement;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v =  inflater.Inflate(Resource.Layout.fragment_settings, container, false);

            SetupSpinner(v);
            enablefingerprint = v.FindViewById<Switch>(Resource.Id.fingerPrintSwitch);
            _tvDevicesManagement = v.FindViewById<TextView>(Resource.Id.devices);
            _tvDevicesManagement.Click += (sender, args) =>
                Activity.StartActivity(typeof(DevicesManagementActivity));
            FingerprintManagerCompat checkHardware;

            checkHardware = FingerprintManagerCompat.From(Activity);


            bool fingerprint = Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

            if (!checkHardware.IsHardwareDetected)
                enablefingerprint.Enabled = false;

            enablefingerprint.Checked = fingerprint ? true : false;

            enablefingerprint.CheckedChange += Enablefingerprint_CheckedChange;
            return v;
        }

        private void SetupSpinner(View v)
        {
            spinner = (Spinner) v.FindViewById(Resource.Id.alarmSpinner);
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
        }
    }
}