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
using FamiliaXamarin.Helpers;

namespace Familia.Devices.DevicesAsistent
{
    public class AsistentHealthDevicesFragment : Android.Support.V4.App.Fragment, View.IOnClickListener
    {
     

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_asistent_health_devices, container, false);
            view.FindViewById<Button>(Resource.Id.BloodPressureButton).SetOnClickListener(this);
            view.FindViewById<Button>(Resource.Id.BloodGlucoseButton).SetOnClickListener(this);

            return view;
        }


        public void OnClick(View v)
        {
            switch (v.Id) {
                case Resource.Id.BloodPressureButton:
                    scanQRCode();
                    break;

                case Resource.Id.BloodGlucoseButton:
                    scanQRCode();
                    break;
            }
        }


        public async void scanQRCode() {
            try {
                var qrJsonData = await Utils.ScanQRCode(Activity);
                Log.Error("AsistentHealthDevicesFragment", qrJsonData.ToString());
                var imei = qrJsonData.Get("imei");
                Log.Error("AsistentHealthDevicesFragment", "imei: " + imei);
                Toast.MakeText(Activity, "imei: " + imei, ToastLength.Long).Show();
            }
            catch (Exception e) {
                Log.Error("AsistentHealthDevicesFragment err", e.Message);
                Toast.MakeText(Activity, "error", ToastLength.Long).Show();
            }
        }
    }
}