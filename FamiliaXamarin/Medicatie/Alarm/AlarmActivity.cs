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
        private Button btnSnooze;
        private string boalaId;
        private List<Boala> boli;

        private Boala mBoala;
        private Medicament mMed;
        private int mIdAlarm;
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
            boalaId = intent.GetStringExtra(BoalaActivity.BOALA_ID);
            medId = intent.GetStringExtra(BoalaActivity.MED_ID);
            boli = Storage.getInstance().getBoliTest(this);
            mBoala = Storage.getInstance().getBoala(boalaId);
            mMed = mBoala.getMedicamentById(medId);
           
            mIdAlarm = intent.GetIntExtra(BoalaActivity.ALARM_ID, -1);
            

            tvMedName.Text = mMed.Name;
            // Create your application here
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
                    Toast.MakeText(this, "Alarma amanata pentru 5 minute.", ToastLength.Short).Show();
                    btnOk.Visibility = ViewStates.Gone;
                    var am = (AlarmManager)GetSystemService(AlarmService);

                    var i = new Intent(this, typeof(AlarmBroadcastReceiver));
                    i.PutExtra(BoalaActivity.BOALA_ID, mBoala.Id);
                    i.PutExtra(BoalaActivity.MED_ID, mMed.IdMed);
                    i.PutExtra(BoalaActivity.ALARM_ID, mIdAlarm);

                    var pi = PendingIntent.GetBroadcast(this, mIdAlarm, i, PendingIntentFlags.OneShot);
                    var afterFiveMins = 2 * 60000;

                    if (am != null)
                    {
                        am.SetInexactRepeating(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + afterFiveMins, AlarmManager.IntervalDay, pi);
                        
                    }
                    Finish();
                    break;
            }
        }
    }
}