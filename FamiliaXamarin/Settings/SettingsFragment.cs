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

namespace FamiliaXamarin.Settings
{
    public class SettingsFragment : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View v =  inflater.Inflate(Resource.Layout.fragment_settings, container, false);
            Spinner spinner = (Spinner)v.FindViewById(Resource.Id.alarmSpinner);
            // Create an ArrayAdapter using the string array and a default spinner layout
            string[] categories = {"5 min", "10 min", "15 min", "30 min"};
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerItem, categories);
            // Specify the layout to use when the list of choices appears
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            // Apply the adapter to the spinner
            spinner.Adapter = adapter;
            return v;
        }
    }
}