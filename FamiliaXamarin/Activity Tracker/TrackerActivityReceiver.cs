using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Helpers;
using Familia.Services;
using Java.Util;

namespace Familia.Activity_Tracker
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class TrackerActivityReceiver : BroadcastReceiver
    {
        public static readonly int TRACKER_ACTIVITY_RECEIVER_NOTIFICATION_ID_USER = 7;
        public static readonly int TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER = 8;
        public static readonly int TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_RESET_STEPS = 9;
        public static readonly string EXTRA_RESET_STEPS = "reset_steps";
        string CHANNEL_ID = "activity_tracker_channel";

        public TrackerActivityReceiver()
        {
            CreateNotificationChannel(CHANNEL_ID, "Step Counter", "Activity Tracker");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Error("TrackerActivityReceiver", "arrived");
            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            if (intent.HasExtra(EXTRA_RESET_STEPS))
            {
                TrackerActivityService.ResetSteps();
                TrackerActivityService.ScheduleForResetSteps(context);
            }
            else {
                int hour = DateTime.Now.Hour;
                HandleCurrentHour(context, hour);
            }
        }
        
        private void HandleCurrentHour(Context context, int hour)
        {
            string content;
            var dt = DateTime.Now;
            var remainingTime = GetRemainingTime(dt);
            var oneHourInMillis = 3600 * 1000;
            DateTime date;

            Log.Error("TrackerActivity RECEIVER", "1. currentHHT: "
                                       + TrackerActivityService.CurrentStepsHh
                                       + " HHT: " + TrackerActivityService.HalfHourTarget);
            switch (hour)
            {
                case 16: // launch notification for hour 16:00
                    content = GetContentFor4pm();
                    LaunchNotification(context, content);
                    date = dt.AddMilliseconds(remainingTime - 4 * oneHourInMillis); // set to 20:00 today
                    Schedule(context, date);
                    break;
                case 20:// launch notification for hour 20:00
                    content = GetContentFor8pm();
                    LaunchNotification(context, content);
                    date = dt.AddMilliseconds(remainingTime + 8 * oneHourInMillis); // set to 8:00 tomorrow
                    Schedule(context, date);
                    break;
                default:
                    if (hour >= 8)
                    {
                        if (TrackerActivityService.DailyTarget <= TrackerActivityService.TotalDailySteps) // DailyTarget achieved
                        {
                            content = "Felicitari! Target zilnic atins.";
                            LaunchNotification(context, content);
                            date = dt.AddMilliseconds(remainingTime - 8 * oneHourInMillis); // set to 16:00 today
                            Schedule(context, date);
                        }
                        else
                        {
                            if (hour < 16)
                            {
                                if (TrackerActivityService.CurrentStepsHh < TrackerActivityService.HalfHourTarget)
                                {
                                    content = GetContentFor30minsTarget();
                                    LaunchNotification(context, content);
                                    TrackerActivityService.CurrentStepsHh = 0;
                                }
                                Schedule(context, 30); //should be 30 here
                            }
                        }
                    }

                    break;
            }
        }

        public static int GetRemainingTime(DateTime dt)
        {
            var milisec = dt.Hour * 3600000 + dt.Minute * 60000 + dt.Second * 1000;
            return 24 * 3600000 - milisec;
        }

        private void Schedule(Context context, int minutes)
        {
            Log.Error("TrackerActivity RECEIVER", "Schedule minutes" + minutes + " minutes");
            Calendar calendar = TrackerActivityService.GetCalendarAfterAddingMinutes(minutes);
            SetAlarm(context, calendar);
        }

        private void Schedule(Context context, DateTime date)
        {
            var calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, 0);
            Log.Error("TrackerActivity RECEIVER", "Schedule calendar" + calendar.ToString());
            SetAlarm(context, calendar);
        }

        private static void SetAlarm(Context context, Calendar calendar)
        {
            var am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            PendingIntent pi = PendingIntent.GetBroadcast(context,
                TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER,
                new Intent(context, typeof(TrackerActivityReceiver)),
                PendingIntentFlags.UpdateCurrent);

            if (am == null) return;
            am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pi);
        }

        private void LaunchNotification(Context context, string content)
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
            string description = mContent;

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
                content = "Numar de pasi facuti in ultima jumatate de ora: " 
                    + TrackerActivityService.CurrentStepsHh;
            }
            return content;
        }

        private static string GetContentFor8pm()
        {
            string content;
            if (TrackerActivityService.DailyTarget <= TrackerActivityService.TotalDailySteps) 
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
            if (TrackerActivityService.DailyTarget <= TrackerActivityService.TotalDailySteps)
            {
                content = "Felicitari! Target zilnic atins.";
            }
            else
            {
                content = "Pasi necesari pana la target: " +
                    (TrackerActivityService.DailyTarget - TrackerActivityService.TotalDailySteps);
            }

            return content;
        }

    }
}