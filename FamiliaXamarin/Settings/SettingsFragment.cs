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
using FamiliaXamarin.Medicatie;

namespace FamiliaXamarin.Settings
{
    public class SettingsFragment : Android.Support.V4.App.Fragment
    {
        private Spinner spinner;
        private int optionOfSnooze;
        private String key;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v =  inflater.Inflate(Resource.Layout.fragment_settings, container, false);

            SetupSpinner(v);

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
        }
    }
}