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
using Familia.Services;

namespace Familia.Activity_Tracker
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class TrackerActivityReceiver : BroadcastReceiver
    {

        public static readonly int TRACKER_ACTIVITY_RECEIVER_NOTIFICATION_ID_USER = 7;
        public static readonly int TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER = 8;
        string CHANNEL_ID = "my_channel_01";

        public static int TotalDailySteps { get; private set; }

        public TrackerActivityReceiver() {
            CreateNotificationChannel(CHANNEL_ID, "Step Counter", "Activity Tracker");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Error("TrackerActivityReceiver", "arrived");
            string content;
            var hour = DateTime.Now.Hour;

            switch (hour)
            {
                case 16: // launch notification for hour 4 pm
                    content = GetContentFor4pm();
                    LaunchNotification(context, content);
                    SetSchedule(context, false);
                    break;
                case 20:// launch notification for hour 8 am
                    content = GetContentFor8pm();
                    LaunchNotification(context, content);
                    SetSchedule(context, false);
                    break;
                default:
                    if (hour >= 8)
                    {
                        if (TrackerActivityService.DailyTarget <= TotalDailySteps)//stepsFromSensor)// DailyTarget achieved
                        {
                            content = "Felicitari! Target zilnic atins.";
                            LaunchNotification(context, content);
                            SetSchedule(context, false);
                        }
                        else
                        {
                            if (hour < 16)
                            {
                                if (TrackerActivityService.CurrentStepsHh < TrackerActivityService.HalfHourTarget)
                                {
                                    Log.Error("TrackerActivityReceiver", "1. currentHHT: " + TrackerActivityService.CurrentStepsHh + " HHT: " + TrackerActivityService.HalfHourTarget);
                                    content = GetContentFor30minsTarget();
                                    LaunchNotification(context, content);
                                    SetSchedule(context, true);
                                }
                                else
                                {
                                    Log.Error("TrackerActivityReceiver", "2. currentHHT: " + TrackerActivityService.CurrentStepsHh + " HHT: " + TrackerActivityService.HalfHourTarget);
                                    SetSchedule(context, false);
                                }
                            }
                            else
                            {
                                if (hour > 20 || hour < 20)
                                {
                                    Log.Error("TrackerActivityReceiver", "3. currentHHT: " + TrackerActivityService.CurrentStepsHh + " HHT: " + TrackerActivityService.HalfHourTarget);
                                    SetSchedule(context, false);// continue checking before or after 8 pm
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Error("TrackerActivityReceiver", "4. currentHHT: " + TrackerActivityService.CurrentStepsHh + " HHT: " + TrackerActivityService.HalfHourTarget);
                        SetSchedule(context, false); // check til hour is 8 am
                    }
                    break;
            }
          
        }

        private void SetSchedule(Context context, bool checkAtEvery30Min)
        {
            if (!checkAtEvery30Min)
            {
                long hoursTillNextCheck = 3600000 * 2; //delay 120 minutes
                Log.Error("TrackerActivityReceiver", "interval: " + hoursTillNextCheck);
                Schedule(context, hoursTillNextCheck);
            }
            else
            {
                TrackerActivityService.CurrentStepsHh = 0; 
                long minutesTillNextCheck = 60000 * 5; //delay 30 mins 
                Log.Error("TrackerActivityReceiver", "interval: " + minutesTillNextCheck);
                Schedule(context, minutesTillNextCheck);
            }

        }

        private void Schedule(Context context, long milisec) {
            var am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var i = new Intent(context, typeof(TrackerActivityReceiver));
            var pi = PendingIntent.GetBroadcast(context, TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER, i, PendingIntentFlags.UpdateCurrent);
            if (am == null) return;
            am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + milisec, AlarmManager.IntervalDay, pi);
        }

        private  void LaunchNotification(Context context,  string content)
        {
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
               .SetSmallIcon(Resource.Drawable.logo)
               .SetContentTitle("Consiliere de activitate")
               .SetContentText(content)
               .SetPriority(NotificationCompat.PriorityHigh);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(TRACKER_ACTIVITY_RECEIVER_NOTIFICATION_ID_USER, builder.Build());
        }

        private static void CreateNotificationChannel(string mChannel, string mTitle, string mContent)
        {
            var description = mContent;

            var channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.Default)
                {
                    Description = description
                };
            channel.EnableVibration(true);
            var notificationManager =
                (NotificationManager)Application.Context.GetSystemService(
                    Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }


        private string GetContentFor30minsTarget()
        {
            string content;
            if (TrackerActivityService.CurrentStepsHh == 0)
            {
                content = "Nu ati facut pasi in ultima jumatate de ora.";
            }
            else
            {
                content = "Numar de pasi facuti in ultima jumatate de ora: " + TrackerActivityService.CurrentStepsHh;
            }
            return content;
        }

        private static string GetContentFor8pm()
        {
            string content;
            if (TrackerActivityService.DailyTarget <= TotalDailySteps) // stepsFromSensor)
            {
                content = "Ziua s-a incheiat cu bine.";
            }
            else
            {
                content = "Nu ati atins target-ul azi.";
            }

            return content;
        }

        private static string GetContentFor4pm()
        {
            string content;
            if (TrackerActivityService.DailyTarget <= TotalDailySteps) //stepsFromSensor)
            {
                content = "Felicitari! Target zilnic atins.";
            }
            else
            {
                long dif = TrackerActivityService.DailyTarget - TotalDailySteps; //stepsFromSensor;
                content = "Pasi necesari pana la target: " + dif;
            }

            return content;
        }

    }
}