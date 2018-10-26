using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using FamiliaXamarin.Settings;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [Activity(Label = "AlarmActivity")]
    public class AlarmActivity : AppCompatActivity, View.IOnClickListener
    {
        private TextView tvMedName;
        private string medId;
        private Button btnOk;
        private Button btnSnooze;
        private string boalaId;
        private List<Disease> boli;

        private Disease mBoala;
        private Medicine mMed;
        private int mIdAlarm;
        protected override void OnPause()
        {
            LaunchSnoozeAlarm();
            Finish();
            base.OnPause();
        }

     


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_alarm);
            tvMedName = FindViewById<TextView>(Resource.Id.tv_med_name_alarm);
            btnOk = FindViewById<Button>(Resource.Id.btn_ok_alarm);
            btnOk.SetOnClickListener(this);
            btnSnooze = FindViewById<Button>(Resource.Id.btn_snooze_alarm);
            btnSnooze.SetOnClickListener(this);
            Intent intent = Intent;
            boalaId = intent.GetStringExtra(DiseaseActivity.BOALA_ID);
            medId = intent.GetStringExtra(DiseaseActivity.MED_ID);
            boli = Storage.GetInstance().GetListOfDiseasesFromFile(this);
            mBoala = Storage.GetInstance().GetDisease(boalaId);
            if (mBoala != null)
            {
                mMed = mBoala.GetMedicineById(medId);
                mIdAlarm = intent.GetIntExtra(DiseaseActivity.ALARM_ID, -1);
                tvMedName.Text = mMed.Name;
            }
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_ok_alarm:
                    Toast.MakeText(this, "Medicament luat.", ToastLength.Short).Show();
                    btnSnooze.Visibility = ViewStates.Gone;
                    Finish();
                    break;
                case Resource.Id.btn_snooze_alarm:
                    LaunchSnoozeAlarm();
                    Finish();
                    break;
            }
        }

        private void LaunchSnoozeAlarm()
        {
            var snoozePreferences = new SnoozePreferences(this);
            var key = snoozePreferences.GetAccessKey();
            var snoozeInMinutes = Int32.Parse(key) * 60000;
            Toast.MakeText(this, "Alarma amanata pentru " + key + " minute.", ToastLength.Short).Show();
            btnOk.Visibility = ViewStates.Gone;
            var am = (AlarmManager) GetSystemService(AlarmService);

            var i = new Intent(this, typeof(AlarmBroadcastReceiver));
            i.PutExtra(DiseaseActivity.BOALA_ID, mBoala.Id);
            i.PutExtra(DiseaseActivity.MED_ID, mMed.IdMed);
            i.PutExtra(DiseaseActivity.ALARM_ID, mIdAlarm);

            var pi = PendingIntent.GetBroadcast(this, mIdAlarm, i, PendingIntentFlags.OneShot);
           

            if (am != null)
            {
                am.SetInexactRepeating(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + snoozeInMinutes,
                    AlarmManager.IntervalDay, pi);
            }
        }
    }
}