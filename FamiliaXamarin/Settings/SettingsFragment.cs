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
using FamiliaXamarin.Medicatie;

namespace FamiliaXamarin.Settings
{
    public class SettingsFragment : Android.Support.V4.App.Fragment
    {
        private Spinner spinner;
        private int optionOfSnooze;
        private string key;
        private Switch enablefingerprint;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v =  inflater.Inflate(Resource.Layout.fragment_settings, container, false);

            SetupSpinner(v);
            enablefingerprint = v.FindViewById<Switch>(Resource.Id.fingerPrintSwitch);
            ISharedPreferences prefs;
            FingerprintManagerCompat checkHardware;

            prefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
            checkHardware = FingerprintManagerCompat.From(Activity);


            bool fingerprint = prefs.GetBoolean("fingerprint", false);

            if (!checkHardware.IsHardwareDetected)
                enablefingerprint.Enabled = false;

            if (fingerprint)
                enablefingerprint.Checked = true;
            else
                enablefingerprint.Checked = false;

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
                    SnoozePreferences snooze = new SnoozePreferences(Activity);
                    
                    switch (optionOfSnooze)
                    {
                        case 0:
                            snooze.SaveAccesKey("5");
                            break;
                        case 1:
                            snooze.SaveAccesKey("10");
                            break;
                        case 2:
                            snooze.SaveAccesKey("15");
                            break;
                        case 3:
                            snooze.SaveAccesKey("30");
                            break;
                    }
                    Toast.MakeText(Activity, "Snooze: " + snooze, ToastLength.Long).Show();
                };


            string[] categories = {"5 min", "10 min", "15 min", "30 min"};

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerItem, categories);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            // Set our view from the "main" layout resource

            

            //return base.OnCreateView(inflater, container, savedInstanceState);
        }
        private void Enablefingerprint_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // this is an Activity
            if (enablefingerprint.Checked)
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this.Activity);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutBoolean("fingerprint", true);
//                editor.PutString("fingerUser", UserData.user);
//                editor.PutString("fingerTip", UserData.tip);
//                editor.PutString("fingerSucursala", UserData.sucursala);
                editor.Apply();


            }
            else
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this.Activity);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutBoolean("fingerprint", false);
                editor.Apply();
            }

        }
    }
}