using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [Activity(Label = "AlarmActivity")]
    public class AlarmActivity : AppCompatActivity, View.IOnClickListener
    {
        private TextView tvMedName;
        private string medId;
        private Button btnOk;
        private string boalaId;
        private List<Boala> boli;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_alarm);
            tvMedName = FindViewById<TextView>(Resource.Id.tv_med_name_alarm);
            btnOk = FindViewById<Button>(Resource.Id.btn_ok_alarm);
            btnOk.SetOnClickListener(this);
            Intent intent = Intent;
            boalaId = intent.GetStringExtra(BoalaActivity.BOALA_ID);
            medId = intent.GetStringExtra(BoalaActivity.MED_ID);
            boli = Storage.getInstance().getBoliTest(this);
            Boala mBoala = Storage.getInstance().getBoala(boalaId);
            Medicament mMed = mBoala.getMedicamentById(medId);
            tvMedName.Text = mMed.Name;
            // Create your application here
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_ok_alarm:
                    Toast.MakeText(this, "Medicament luat.", ToastLength.Short).Show();
                    Finish();
                    break;
            }
        }
    }
}