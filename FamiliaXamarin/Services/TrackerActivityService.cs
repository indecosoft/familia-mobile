using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Activity_Tracker;
using Familia.Helpers;

namespace Familia.Services
{
    [Service]
    public class TrackerActivityService : Service, StepCounterSensor.IStepCounterSensorChangedListener
    {
        public static readonly int TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID_USER = 7;
        public static bool RestartService = true;
        string CHANNEL_ID = "my_channel_01";
        private const int ServiceRunningNotificationId = 10000;

        private long _stepsFromSensor = -1;
        public static int DailyTarget;
        public static int HalfHourTarget;
        public static long CurrentStepsHh;
        public static long TotalDailySteps;

        public TrackerActivityService()
        {
            InitStepsTarget();
        }

        public override IBinder OnBind(Intent intent) { return null; }

        public override void OnCreate()
        {
            base.OnCreate();
            try
            {
                Log.Error("TrackerActivityService", "started");
                var sensor = new StepCounterSensor(this);
                sensor.SetListener(this);

                var am = (AlarmManager)GetSystemService(AlarmService);
                var i = new Intent(this, typeof(TrackerActivityReceiver));
                PendingIntent pi = PendingIntent.GetBroadcast(this, TrackerActivityReceiver.TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER, i, PendingIntentFlags.UpdateCurrent);

                if (am != null)
                {
                    am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 100, AlarmManager.IntervalDay, pi);
                }

                var channel = new NotificationChannel(CHANNEL_ID, "Step Counter",
                    NotificationImportance.High);

                ((NotificationManager)GetSystemService(NotificationService))
                    .CreateNotificationChannel(channel);
                Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .SetContentTitle("Familia")
                    .SetContentText("Step counter Ruleaza in fundal")
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetOngoing(true)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .Build();

                StartForeground(ServiceRunningNotificationId, notification);
            }
            catch (Exception e)
            {
                Log.Error("TrackerActivityService Error", e.Message);
            }
        }

        private void InitStepsTarget()
        {
            try
            {
                TotalDailySteps = int.Parse(Utils.GetDefaults("ActivityTrackerDailySteps"));
            }
            catch (Exception e)
            {
                TotalDailySteps = 0;
                Utils.SetDefaults("ActivityTrackerDailySteps", TotalDailySteps + "");
            }

            try
            {
                DailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception e) //daily target not setted, default is 5000
            {
                DailyTarget = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", DailyTarget + "");
            }
            HalfHourTarget = DailyTarget / 15;
            CurrentStepsHh = 0;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public void OnStepCounterSensorChanged(long count)
        {
            Log.Error("TrackerActivityService", "stepsFromSensor: " + count);
            int hour = DateTime.Now.Hour;
            KeepCountingInRange(count, hour);
            Log.Error("TrackerActivityService", "TotalDailySteps: " + TotalDailySteps);
            _stepsFromSensor = count;
        }

        private void KeepCountingInRange(long count, int hour)
        {
            HalfHourTarget = DailyTarget / 15;

            if (hour >= 1)
            {
                try
                {
                    TotalDailySteps = int.Parse(Utils.GetDefaults("ActivityTrackerDailySteps"));
                }
                catch (Exception e)
                {
                    TotalDailySteps = 0;
                }

                if (_stepsFromSensor != count)
                {
                    CurrentStepsHh++;
                    TotalDailySteps++;
                    Utils.SetDefaults("ActivityTrackerDailySteps", TotalDailySteps + "");
                }
            }

            if (hour >= 23)
            {
                Utils.SetDefaults("ActivityTrackerDailySteps", TotalDailySteps + "");
                TotalDailySteps = 0;
            }
        }
    }
}