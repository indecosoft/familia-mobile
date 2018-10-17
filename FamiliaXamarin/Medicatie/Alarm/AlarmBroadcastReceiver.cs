using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Text;
using Java.Util;
using Calendar = Java.Util.Calendar;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiver : BroadcastReceiver
    {
        private List<Boala> boli;
        
        private Hour mHour;
        private Boala mBoala;
        private Medicament mMed;
        public override void OnReceive(Context context, Intent intent)
        {

            string medId = intent.GetStringExtra(BoalaActivity.MED_ID);
            string boalaId = intent.GetStringExtra(BoalaActivity.BOALA_ID);
            string hourId = intent.GetStringExtra(BoalaActivity.HOUR_ID);

            boli = Storage.getInstance().getBoliTest(context);
            mBoala = Storage.getInstance().getBoala(boalaId);
             
            if (mBoala != null)
            {
                mMed = mBoala.getMedicamentById(medId);

                if (mMed != null)
                {
                    
                    mHour = mMed.FindHourById(hourId);



                    if (mMed.NrZile != 0)
                    {   
                        var hourString = mHour.Nume;
                        var parts = hourString.Split(':');
                        var timeHour = Convert.ToInt32(parts[0]);
                        var timeMinute = Convert.ToInt32(parts[1]);
                        var calendar = Calendar.Instance;
                        var setCalendar = Calendar.Instance;
                        setCalendar.Set(CalendarField.HourOfDay, timeHour);
                        setCalendar.Set(CalendarField.Minute, timeMinute);
                        setCalendar.Set(CalendarField.Second, 0);
                        var dateString = mMed.Date;
                        parts = dateString.Split('/');
                        var day = Convert.ToInt32(parts[0]);
                        var month = Convert.ToInt32(parts[1]) - 1;
                        var year = Convert.ToInt32(parts[2]);


                        setCalendar.Set(CalendarField.Year, year);
                        setCalendar.Set(CalendarField.Month, month);
                        setCalendar.Set(CalendarField.DayOfMonth, day);

                        
                        setCalendar.Add(CalendarField.Date, mMed.NrZile);

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
            }
        }

        private static void LaunchAlarm(Context context, Intent intent, string medId, string boalaId)
        {
            Toast.MakeText(context, "ALARMA !!!", ToastLength.Long).Show();
            Intent i = new Intent(context, typeof(AlarmActivity));
            Intent intentNotification = new Intent(context, typeof(MedicineFragment));
            //context.startActivity(new Intent(context, AlarmActivity.class));
            i.PutExtra(BoalaActivity.MED_ID, medId);
            i.PutExtra(BoalaActivity.BOALA_ID, boalaId);
            

            context.StartActivity(i);
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intentNotification, 0);

            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context, Constants.ChannelId)
                    .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                    .SetWhen(DateTime.Now.Millisecond)
                    .SetContentTitle(Constants.NotificationTitle)
                    .SetContentText(Constants.NotifContent)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityDefault)
                    .SetContentIntent(pendingIntent);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);


            // notificationId is a unique int for each notification that you must define
            notificationManager.Notify(Constants.NotifId, mBuilder.Build());
        }
    }
}