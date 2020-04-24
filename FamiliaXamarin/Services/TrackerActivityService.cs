using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Activity_Tracker;
using Familia.Helpers;
using Familia.Medicatie.Alarm;
using Java.Util;

namespace Familia.Services
{
    [Service]
    public class TrackerActivityService : Service, StepCounterSensor.IStepCounterSensorChangedListener
    {
        public static readonly int TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID_USER = 7;

        private long _stepsFromSensor = -1;
        public static int DailyTarget;
        public static int HalfHourTarget;
        public static long CurrentStepsHh;
        public static long TotalDailySteps;
        private StepCounterSensor sensor;
        private static IStepsChangeListener listener;

        public TrackerActivityService()
        {
            InitStepsTarget();
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Log.Error("Service:", "TrackerActivityService STARTED");

            sensor = new StepCounterSensor(this);
            sensor.SetListener(this);

            Schedule(30);

            ScheduleForResetSteps(this);

            var notification = new NotificationCompat.Builder(this, App.NonStopChannelIdForServices)
            .SetContentTitle("Familia")
            .SetContentText("Ruleaza in fundal")
            .SetSmallIcon(Resource.Drawable.logo)
            .SetOngoing(true)
            .Build();

            StartForeground(App.NonstopNotificationIdForServices, notification);
        }

        public override IBinder OnBind(Intent intent) { return null; }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            sensor.CloseListener();
            ResetSteps();
            Log.Error("TrackerActivityService", "step counter service destroyed");
        }

        private void InitStepsTarget()
        {
            TotalDailySteps = GetTotalDailySteps();
            DailyTarget = GetDailyTarget();
            HalfHourTarget = DailyTarget / 15;
            CurrentStepsHh = 0;
        }

        public static void ScheduleForResetSteps(Context context)
        {
            var oneHourInMillis = 3600 * 1000;
            var remainingTime = TrackerActivityReceiver.GetRemainingTime(DateTime.Now);
            var date = DateTime.Now.AddMilliseconds(remainingTime + oneHourInMillis); // call at hour 1:00 to reset steps
            var calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, 0);

            var am = (AlarmManager)context.GetSystemService(AlarmService);
            var i = new Intent(context, typeof(TrackerActivityReceiver));
            i.PutExtra(TrackerActivityReceiver.EXTRA_RESET_STEPS, "reset");

            PendingIntent pi = PendingIntent.GetBroadcast(context,
                TrackerActivityReceiver.TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_RESET_STEPS,
                i,
                PendingIntentFlags.UpdateCurrent);

            if (am != null)
            {
                am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pi);
            }
        }

        private static DateTime GetDateAfterMinutes(int minutes)
        {
            DateTime dt = DateTime.Now;
            return dt.AddMilliseconds(minutes * 60000);
        }

        public static Calendar GetCalendarAfterAddingMinutes(int minutes)
        {
            var date = GetDateAfterMinutes(minutes);
            var calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, 0);
            return calendar;
        }

        private void Schedule(int minutes)
        {
            Calendar calendar = GetCalendarAfterAddingMinutes(minutes);
            var am = (AlarmManager)GetSystemService(AlarmService);

            PendingIntent pi = PendingIntent.GetBroadcast(this, 
                TrackerActivityReceiver.TRACKER_ACTIVITY_RECEIVER_PENDING_INTENT_ID_USER,
                new Intent(this, typeof(TrackerActivityReceiver)),
                PendingIntentFlags.UpdateCurrent);

            if (am != null)
            {
                am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pi);
            }
        }

        private int GetDailyTarget()
        {
            try
            {
                return int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception) //daily target not setted, default is 5000
            {
                var target = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", target + "");
                return target;
            }
        }

        private static int GetTotalDailySteps()
        {
            try
            {
                return int.Parse(Utils.GetDefaults("ActivityTrackerDailySteps"));
            }
            catch (Exception)
            {
                Utils.SetDefaults("ActivityTrackerDailySteps", 0 + "");
                return 0;
            }
        }

        public void OnStepCounterSensorChanged(int count)
        {

            DailyTarget = GetDailyTarget();
            HalfHourTarget = DailyTarget / 15;

            int hour = DateTime.Now.Hour;
            KeepCountingInRange(count, hour);
            _stepsFromSensor = count;

            if (listener != null)
            {
                listener.OnStepsChanged(TotalDailySteps);
            }

            Log.Error("TrackerActivityService", "Sensor: " + count
             + " Total " + TotalDailySteps
             + " CurrentHHT " + CurrentStepsHh
             + " HHTarget " + HalfHourTarget
             + " Target " + DailyTarget);
        }

        private void KeepCountingInRange(long count, int hour)
        {
            HalfHourTarget = DailyTarget / 15;

            if (hour >= 1)
            {
                TotalDailySteps =  GetTotalDailySteps();

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


        public static void ResetSteps() {
            TotalDailySteps = 0;
            CurrentStepsHh = 0;
            Utils.SetDefaults("ActivityTrackerDailySteps", TotalDailySteps + "");
        }

        public static void SetListener(IStepsChangeListener mlistener)
        {
            listener = mlistener;
        }

        public interface IStepsChangeListener
        {
            public void OnStepsChanged(long steps);
        }

    }
}