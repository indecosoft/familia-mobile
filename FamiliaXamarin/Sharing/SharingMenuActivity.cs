﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Sharing
{
    [Activity(Label = "SharingMenuActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SharingMenuActivity : AppCompatActivity
    {
        private Button btnBloodPressure;
        private Button btnBloodGlucose;
        private Button btnSmartBand;
        private string _name, _email,_imei;
        private void InitUi(Bundle savedInstanceState)
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            };

            _name = Intent.GetStringExtra("Name");
            _email = Intent.GetStringExtra("Email");
            _imei = Intent.GetStringExtra("Imei");
            Title = _name;
            btnBloodPressure = FindViewById<Button>(Resource.Id.BloodPressureButton);
            btnBloodGlucose = FindViewById<Button>(Resource.Id.BloodGlucoseButton);
            btnSmartBand = FindViewById<Button>(Resource.Id.SmartbandButton);

            var cvSmartBand = FindViewById<CardView>(Resource.Id.card_view2);
            cvSmartBand.Visibility = ViewStates.Gone;

        }

        private void InitEvents()
        {
            btnBloodPressure.Click += BtnBloodPressureOnClick;
            btnBloodGlucose.Click += BtnBloodGlucoseOnClick;
            btnSmartBand.Click += BtnSmartBandOnClick;
        }

        private void BtnSmartBandOnClick(object sender, EventArgs e)
        {
            
        }

        private void BtnBloodGlucoseOnClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(PressureAndGlucoseActivity));
            intent.PutExtra("DataType", "BloodGlucose");
            intent.PutExtra("Name", _name);
            intent.PutExtra("Email", _email);
            intent.PutExtra("Imei", _imei);
            StartActivity(intent);
        }

        private void BtnBloodPressureOnClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(PressureAndGlucoseActivity));
            intent.PutExtra("DataType", "BloodPressure");
            intent.PutExtra("Name", _name);
            intent.PutExtra("Email", _email);
            intent.PutExtra("Imei", _imei);
            StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            
            SetContentView(Resource.Layout.activity_sharing_menu);
            InitUi(savedInstanceState);
            InitEvents();
            // Create your application here
        }
    }
}