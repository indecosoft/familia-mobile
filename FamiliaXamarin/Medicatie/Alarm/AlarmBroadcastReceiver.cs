using System;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Widget;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;
using Calendar = Java.Util.Calendar;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiver : BroadcastReceiver
    {
        private Hour _mHour;
        private Disease _mDisease;
        private Medicine _mMed;
        public override void OnReceive(Context context, Intent intent)
        {
            var medId = intent.GetStringExtra(DiseaseActivity.MED_ID);
            var boalaId = intent.GetStringExtra(DiseaseActivity.BOALA_ID);
            var hourId = intent.GetStringExtra(DiseaseActivity.HOUR_ID);

            Storage.GetInstance().GetListOfDiseasesFromFile(context);
            _mDisease = Storage.GetInstance().GetDisease(boalaId);

            if (_mDisease == null) return;
            _mMed = _mDisease.GetMedicineById(medId);

            if (_mMed == null) return;
            _mHour = _mMed.FindHourById(hourId);

            if (_mMed.NumberOfDays != 0)
            {   
                var hourString = _mHour.HourName;
                var parts = hourString.Split(':');
                var timeHour = Convert.ToInt32(parts[0]);
                var timeMinute = Convert.ToInt32(parts[1]);
                var calendar = Calendar.Instance;
                var setCalendar = Calendar.Instance;
                setCalendar.Set(CalendarField.HourOfDay, timeHour);
                setCalendar.Set(CalendarField.Minute, timeMinute);
                setCalendar.Set(CalendarField.Second, 0);
                var dateString = _mMed.Date;
                parts = dateString.Split('.');
                var day = Convert.ToInt32(parts[0]);
                var month = Convert.ToInt32(parts[1]) - 1;
                var year = Convert.ToInt32(parts[2]);


                setCalendar.Set(CalendarField.Year, year);
                setCalendar.Set(CalendarField.Month, month);
                setCalendar.Set(CalendarField.DayOfMonth, day);

                        
                setCalendar.Add(CalendarField.Date, _mMed.NumberOfDays);

                if (setCalendar.After(calendar))
                {                                     
                    LaunchAlarm(context, intent, medId, boalaId);
                }          
            }
            else
            {
                LaunchAlarm(context, intent, medId, boalaId);
            }
        }

        private static void LaunchAlarm(Context context, Intent intent, string medId, string boalaId)
        {
            Toast.MakeText(context, "ALARMA !!!", ToastLength.Long).Show();
            var i = new Intent(context, typeof(AlarmActivity));
            var intentNotification = new Intent(context, typeof(MedicineFragment));
            //context.startActivity(new Intent(context, AlarmActivity.class));
            i.PutExtra(DiseaseActivity.MED_ID, medId);
            i.PutExtra(DiseaseActivity.BOALA_ID, boalaId);
            

            context.StartActivity(i);
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var pendingIntent = PendingIntent.GetActivity(context, 0, intentNotification, 0);

            var mBuilder =
                new NotificationCompat.Builder(context, Constants.ChannelId)
                    .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                    .SetWhen(DateTime.Now.Millisecond)
                    .SetContentTitle(Constants.NotificationTitle)
                    .SetContentText(Constants.NotifContent)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityDefault)
                    .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(context);


            // notificationId is a unique int for each notification that you must define
            notificationManager.Notify(Constants.NotifId, mBuilder.Build());
        }
    }
}