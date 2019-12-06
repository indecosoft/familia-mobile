using System;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Activity_Tracker;
using FamiliaXamarin.Helpers;
using Exception = System.Exception;

namespace Familia.Services
{
    [Service]
    class TrackerActivityService: Service, StepCounterSensor.IStepCounterSensorChangedListener
    {
        public static readonly int TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID = 4;
        public static readonly int TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID_USER = 7;
        public static bool RestartService = true;
        string CHANNEL_ID = "my_channel_01";
        int ServiceRunningNotificationId = 2000;
        private bool isRestarting = true;

        private long stepsFromSensor = -1;
        private static int DailyTarget;// = 6000;
        private static int HalfHourTarget;// = DailyTarget / 15;
        private long stepsHH;

        private readonly Timer handler = new Timer(1000);
        
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            DailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            HalfHourTarget = DailyTarget / 15;

            Log.Error("TrackerActivityService", "DT: " + DailyTarget + " hht: " + HalfHourTarget);
                
            string content;
            var now = DateTime.Now;
            int hour = now.Hour;
            Log.Error("TrackerActivityService", "hour: " + now.Hour);

            switch (hour)
            {
                case 16: // launch notification for hour 4 pm
                    if (DailyTarget <= stepsFromSensor)
                    {
                        content = "Felicitari! Target zilnic atins.";
                    }
                    else
                    {
                        long dif = DailyTarget - stepsFromSensor;
                        content = "Pasi necesari pana la target: " + dif;
                    }
                    LaunchNotification(content, false);
                    break;
                case 20:// launch notification for hour 8 am
                    if (DailyTarget <= stepsFromSensor)
                    {
                        content = "Ziua s-a incheiat cu bine.";
                    }
                    else
                    {
                        content = "Nu ati atins target-ul azi.";
                    }
                    LaunchNotification(content, false);
                    break;
                default:
                    if (hour >= 8)
                    {
                        if (DailyTarget <= stepsFromSensor)// DailyTarget achieved
                        {  
                            content = "Felicitari! Target zilnic atins.";
                            LaunchNotification(content, false);
                        }
                        else
                        {
                            if (hour < 16)
                            {
                                string hhT;
                                if (stepsHH < HalfHourTarget)
                                {
                                    hhT = " mai mic decat " + HalfHourTarget;
                                    if (stepsHH == 0)
                                    {
                                        content = "Nu ati facut pasi in ultima jumatate de ora.";
                                    }
                                    else
                                    {
                                        content = "Numar de pasi facuti in ultima jumatate de ora: " + stepsHH;
                                    }
                                    LaunchNotification(content, true);
                                }
                                else
                                {
                                    hhT = " mai mare decat " + HalfHourTarget;
                                }

                                Log.Error("TrackerActivityService", "hht: " + hhT);
                            }
                            else
                            {
                                if (hour > 20 || hour < 20)
                                {
                                    long minutesTillNextCheck = 60000 * 5; // continue checking before or after 8 pm
                                    handler.Interval = minutesTillNextCheck;
                                }
                            }
                        }
                    }
                    else
                    {
                        long minutesTillNextCheck = 60000 * 5; // check til hour is 8 am
                        handler.Interval = minutesTillNextCheck;
                    }
                    break;
            }
        }

        public void LaunchNotification(string content, bool checkAtEvery30Min)
        {
            
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.logo)
                .SetContentTitle("TrackerActivityService Notify User")
                .SetContentText(content)
                .SetPriority(NotificationCompat.PriorityHigh);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID_USER, builder.Build());

            if (!checkAtEvery30Min)
            {
                //delay 120 minutes
                long hoursTillNextCheck = 3600000 * 2;
                Log.Error("TrackerActivityService", "interval: " + hoursTillNextCheck);
                handler.Interval = hoursTillNextCheck;
            }
            else
            {
                // reset
                stepsHH = 0;
                //delay 30 mins ( 5 for test )
                long minutesTillNextCheck = 60000 * 5; // here will be * 30
                Log.Error("TrackerActivityService", "interval: " + minutesTillNextCheck);
                handler.Interval = minutesTillNextCheck;
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("TrackerActivityService", "destroyed");
            if (RestartService)
            {
                Log.Error("TrackerActivityService", "restarting..");
                StartService(new Intent(this, typeof(TrackerActivityService)));
            }
        }

        public override IBinder OnBind(Intent intent){ return null; }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("TrackerActivityService", "started");

            StepCounterSensor sensor = new StepCounterSensor(this);
            sensor.SetListener(this);
            try
            {
                DailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception e)
            {
                //daily target not setted, default is 5000
                DailyTarget = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", DailyTarget+"");
            }
            
            HalfHourTarget = DailyTarget / 15;
            stepsHH = 0;

            handler.Elapsed += OnTimedEvent;
           

            try
            {

                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Step Counter",
                    NotificationImportance.High);

                ((NotificationManager)GetSystemService(NotificationService))
                    .CreateNotificationChannel(channel);
                Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .SetContentTitle("Familia")
                    .SetContentText("Step counter Ruleaza in fundal")
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetOngoing(true)
                    .Build();

                StartForeground(ServiceRunningNotificationId, notification);

            }
            catch (Exception e)
            {
                Log.Error("TrackerActivityService Error", e.Message);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {

            if(intent == null)
            {
                OnCreate();
            }
            Log.Error("TrackerActivityService", "start command " + startId);
            handler.Start();
            
            return StartCommandResult.RedeliverIntent;
        }

        public void OnStepCounterSensorChanged(long count)
        {
            Log.Error("TrackerActivityService", "steps: " + count);
            if (stepsFromSensor != count)
            {
                stepsHH++;
            }
            stepsFromSensor = count;
        }
    }
}