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

using Org.Json;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Linq;


using FamiliaXamarin.DataModels;
using FamiliaXamarin.Services;


namespace FamiliaXamarin.Medicatie.Alarm
{
    [Activity(Label = "AlarmActivity")]
    public class AlarmActivity : AppCompatActivity, View.IOnClickListener
    {
        private TextView tvMedName;
        private TextView tvLabel;
        private string medId;
        private Button btnOk;
        private Button btnSnooze;
        private string boalaId;
        private List<Disease> boli;

        private Disease mBoala;
        private Medicine mMed;
        private int mIdAlarm;
        private Ringtone r;
        private string extraMessage;

        private string uuid;
        private int postpone;
        private string title;
        private string content;
        private int NotifyId;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;

        protected override void OnDestroy()
        {
            if (r != null)
                r.Stop();
            base.OnDestroy();
        }
        protected override void OnUserLeaveHint()
        {
            LaunchSnoozeAlarm();
            base.OnUserLeaveHint();
            Finish();
        }


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

//            PowerManager pm = (PowerManager)this.GetSystemService(Context.PowerService);
//            bool isScreenOn = pm.IsInteractive; // check if screen is on
//            if (!isScreenOn)
//            {
//                PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "my tag");
//                wl.Acquire(3000); //set your time in milliseconds
//            }


//            Window.SetFlags(WindowManagerFlags.ShowWhenLocked ,  WindowManagerFlags.DismissKeyguard | WindowManagerFlags.ShowWhenLocked );

            //            if (Utils.util)
            //            {
            //                Utils.util = false;
            //                Finish();
            //                
            //            }
            SetContentView(Resource.Layout.activity_alarm);
            Log.Error("alarm activity", " started");
            tvMedName = FindViewById<TextView>(Resource.Id.tv_med_name_alarm);
            tvLabel = FindViewById<TextView>(Resource.Id.tv_label_med_alarm);
            tvLabel.Visibility = ViewStates.Invisible;
            btnOk = FindViewById<Button>(Resource.Id.btn_ok_alarm);
            btnOk.SetOnClickListener(this);
            btnSnooze = FindViewById<Button>(Resource.Id.btn_snooze_alarm);
            btnSnooze.SetOnClickListener(this);
            btnSnooze.Visibility = ViewStates.Visible;
            btnOk.Visibility = ViewStates.Visible;

            Intent intent = Intent;
            extraMessage = intent.GetStringExtra("message");
            if (extraMessage == AlarmBroadcastReceiver.FROM_APP)
            {
                tvLabel.Visibility = ViewStates.Visible;

                boalaId = intent.GetStringExtra(DiseaseActivity.BOALA_ID);
                medId = intent.GetStringExtra(DiseaseActivity.MED_ID);
                NotifyId = intent.GetIntExtra(AlarmBroadcastReceiver.NOTIFICATION_ID, 5);
                Log.Error("ID NOTIFICARE", NotifyId + "");
                boli = Storage.GetInstance().GetListOfDiseasesFromFile(this);
                mBoala = Storage.GetInstance().GetDisease(boalaId);
               


                if (mBoala != null)
                {
                    mMed = mBoala.GetMedicineById(medId);
                    mIdAlarm = intent.GetIntExtra(DiseaseActivity.ALARM_ID, -1);
                    tvMedName.Text = mMed.Name;
                }
            }
            else
            {
                if (extraMessage == AlarmBroadcastReceiverServer.FROM_SERVER)
                {
                    tvLabel.Text = "";
                    title = intent.GetStringExtra(AlarmBroadcastReceiverServer.MEDICATION_NAME);
                    uuid = intent.GetStringExtra(AlarmBroadcastReceiverServer.Uuid);
                    content = intent.GetStringExtra(AlarmBroadcastReceiverServer.Content);
                    postpone =intent.GetIntExtra(AlarmBroadcastReceiverServer.Postpone, 5);
                    NotifyId = intent.GetIntExtra("notifyId", 5);
                    tvMedName.Text = "Pentru afectiunea " + title + " trebuie luat medicamentul " + content;
                    Log.Error("Alarm activity", "1");
                    Log.Error("alarm activity", postpone + " ");
                    var path =
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var nameDb = "devices_data.db";
                    _db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
                    await _db.CreateTableAsync<MedicineRecords>();
                    Log.Error("Alarm activity", "2");

                }

            }


            //            Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            //            r = RingtoneManager.GetRingtone(this, notification);
            //            r.Play();
        }

        public async void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_ok_alarm:

                    if (extraMessage == AlarmBroadcastReceiverServer.FROM_SERVER)
                    {   
                        var now = DateTime.Now;

                        var mArray = new JSONArray().Put(new JSONObject().Put("uuid", uuid)
                            .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

//                        NotificationManager notificationManager = (NotificationManager)ApplicationContext.GetSystemService(Context.NotificationService);
//                        notificationManager.Cancel(NotifyId);

                        await Task.Run(async () =>
                        {
                            if (await SendData(this, mArray))
                            {
                                var running = IsServiceRunning(typeof(MedicationService), this);
                                if (running)
                                {
                                    Log.Error("SERVICE", "Medication service is running");
                                    this.StopService(_medicationServiceIntent);
                                }
                            }
                            else
                             {
                                AddMedicine(_db, uuid, now);
                                Log.Error("SERVICE", "Medication service started");
                                _medicationServiceIntent =
                                new Intent(this, typeof(MedicationService));
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    this.StartForegroundService(_medicationServiceIntent);
                                }
                                else
                                {
                                    this.StartService(_medicationServiceIntent);
                                }
                               }
                        });

                    }

                    Log.Error("ID NOTIFICARE", NotifyId +"");
                        NotificationManager notificationManager = (NotificationManager)ApplicationContext.GetSystemService(Context.NotificationService);
                        notificationManager.Cancel(NotifyId);
                    

                    Toast.MakeText(this, "Medicament luat.", ToastLength.Short).Show();
                    Finish();

                    break;
                case Resource.Id.btn_snooze_alarm:
                        LaunchSnoozeAlarm();
                    Finish();
                    break;
            }
            //            r.Stop();
        }

        private void LaunchSnoozeAlarm()
        {
            Intent i;
            var snoozeInMilisec = postpone * 60000;
            PendingIntent pi;
            if (extraMessage == AlarmBroadcastReceiver.FROM_APP)
            {
                Log.Error("Alarm activity", "3");

                int snoozeInMinutes;

                if (int.TryParse(Utils.GetDefaults("snooze"), out snoozeInMinutes))
                    snoozeInMinutes = int.Parse(Utils.GetDefaults("snooze"));
                else
                    snoozeInMinutes = 5;

                 snoozeInMilisec = snoozeInMinutes * 60000;

                Toast.MakeText(this, "Alarma amanata pentru " + snoozeInMinutes + " minute.", ToastLength.Short).Show();

                i = new Intent(this, typeof(AlarmBroadcastReceiver));
               
                i.AddFlags(ActivityFlags.ClearTop);
                i.PutExtra(DiseaseActivity.BOALA_ID, mBoala.Id);
                i.PutExtra(DiseaseActivity.MED_ID, mMed.IdMed);
                i.PutExtra(DiseaseActivity.ALARM_ID, mIdAlarm);
//                i.SetFlags(ActivityFlags.NewTask);

                pi = PendingIntent.GetBroadcast(this, mIdAlarm, i, PendingIntentFlags.OneShot);

                NotificationManager notificationManager = (NotificationManager)ApplicationContext.GetSystemService(Context.NotificationService);
                notificationManager.Cancel(NotifyId);
            }
            else
            {
                Log.Error("Alarm activity", "4");

                Toast.MakeText(this, "Alarma amanata pentru " + postpone + " minute.", ToastLength.Short).Show();
                Log.Error("Alarm activity", "5");

                i = new Intent(this, typeof(AlarmBroadcastReceiverServer));
                i.AddFlags(ActivityFlags.ClearTop);
                i.PutExtra(AlarmBroadcastReceiverServer.Uuid, uuid);
                i.PutExtra("notifyId", NotifyId);
                i.PutExtra("message", AlarmBroadcastReceiverServer.FROM_SERVER);
                i.PutExtra(AlarmBroadcastReceiverServer.Title, title);
                i.PutExtra(AlarmBroadcastReceiverServer.Content, content);
                i.PutExtra(AlarmBroadcastReceiverServer.Postpone, postpone);
//                i.SetFlags(ActivityFlags.NewTask);
                Log.Error("Alarm activity", "6");


                pi = PendingIntent.GetBroadcast(this, NotifyId, i, PendingIntentFlags.OneShot);

                NotificationManager notificationManager = (NotificationManager)ApplicationContext.GetSystemService(Context.NotificationService);
                notificationManager.Cancel(NotifyId);

            }


            var am = (AlarmManager)GetSystemService(AlarmService);

            //SendBroadcast(i);


            if (am != null)
            {
                am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + snoozeInMilisec,
                    AlarmManager.IntervalDay, pi);
            }

            // Utils.util = true;
        }


        

        private static bool IsServiceRunning(Type classTypeof, Context context)
        {
            var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
#pragma warning disable 618
            return manager.GetRunningServices(int.MaxValue).Any(service =>
                service.Service.ShortClassName == classTypeof.ToString());
        }

        private static async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {
            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        private static async Task<bool> SendData(Context context, JSONArray mArray)
        {
            var result = await WebServices.Post(
                $"{Constants.PublicServerAddress}/api/medicine", mArray,
                Utils.GetDefaults("Token"));
            if (!Utils.CheckNetworkAvailability()) return false;
            switch (result)
            {
                case "Done":
                case "done":
                    return true;
                default:
                    return false;
            }
        }

    }

}