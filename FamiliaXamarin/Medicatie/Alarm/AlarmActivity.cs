using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia;
using FamiliaXamarin.Helpers;
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
        private Ringtone r;
 
        protected override void OnDestroy()
        {
            if(r!=null)
            r.Stop();
            base.OnDestroy();
        }
        protected override void OnUserLeaveHint()
        {
            LaunchSnoozeAlarm();
            base.OnUserLeaveHint();
            Finish();
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.SetFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.DismissKeyguard |
                                 WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn, WindowManagerFlags.Fullscreen | WindowManagerFlags.DismissKeyguard |
                                                                                                          WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);

            //            if (Utils.util)
            //            {
            //                Utils.util = false;
            //                Finish();
            //                
            //            }
            Log.Error("AAAAAAAAAAAA", "alarm activity");
            SetContentView(Resource.Layout.activity_alarm);
            
            tvMedName = FindViewById<TextView>(Resource.Id.tv_med_name_alarm);
            btnOk = FindViewById<Button>(Resource.Id.btn_ok_alarm);
            btnOk.SetOnClickListener(this);
            btnSnooze = FindViewById<Button>(Resource.Id.btn_snooze_alarm);
            btnSnooze.SetOnClickListener(this);
            btnSnooze.Visibility = ViewStates.Visible;
            btnOk.Visibility = ViewStates.Visible;
            
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
            Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            r = RingtoneManager.GetRingtone(this, notification);
            r.Play();
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_ok_alarm:
                    Toast.MakeText(this, "Medicament luat.", ToastLength.Short).Show();
                    //btnSnooze.Visibility = ViewStates.Gone;
                    //OnBackPressed();
                    Finish();
                    
                    break;
                case Resource.Id.btn_snooze_alarm:
                    LaunchSnoozeAlarm();
                    Finish();
                    //OnBackPressed();
                    break;
            }
            r.Stop();
        }

        private void LaunchSnoozeAlarm()
        {
            int snoozeInMinutes;
           // bool a = int.TryParse(Utils.GetDefaults("snooze", this), out snoozeInMinutes);
            if (int.TryParse(Utils.GetDefaults("snooze"), out snoozeInMinutes))
                snoozeInMinutes = int.Parse(Utils.GetDefaults("snooze"));
            else
                snoozeInMinutes = 5;

            var snoozeInMilisec = snoozeInMinutes * 60000;
            
            Toast.MakeText(this, "Alarma amanata pentru " + snoozeInMinutes + " minute.", ToastLength.Short).Show();
            
            var am = (AlarmManager) GetSystemService(AlarmService);
            
            var i = new Intent(this, typeof(AlarmBroadcastReceiver));
            i.AddFlags(ActivityFlags.ClearTop);
            i.PutExtra(DiseaseActivity.BOALA_ID, mBoala.Id);
            i.PutExtra(DiseaseActivity.MED_ID, mMed.IdMed);
            i.PutExtra(DiseaseActivity.ALARM_ID, mIdAlarm);
            i.SetFlags(ActivityFlags.NewTask);
            
            //SendBroadcast(i);
            var pi = PendingIntent.GetBroadcast(this, mIdAlarm, i, PendingIntentFlags.OneShot);
           

            if (am != null)
            {
                am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + snoozeInMilisec,
                    AlarmManager.IntervalDay, pi);
            }

           // Utils.util = true;
        }
    }
}