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
        public static readonly int TRACKER_ACTIVITY_SERVICE_NOTIFICATION_ID_USER = 7;
        public static bool RestartService = true;
        string CHANNEL_ID = "my_channel_01";
//        int ServiceRunningNotificationId = 2000; //for test on another id
        private const int ServiceRunningNotificationId = 10000;
        private bool _isRestarting = true;

        private long _stepsFromSensor = -1;
        private static int _dailyTarget;
        private static int _halfHourTarget;
        private long _currentStepsHh;
        public static long TotalDailySteps;
        private readonly Timer _handler = new Timer(1000);
        
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _dailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            _halfHourTarget = _dailyTarget / 15;
                
            string content;
            var hour = DateTime.Now.Hour;
            switch (hour)
            {
                case 16: // launch notification for hour 4 pm
                    content = GetContentFor4pm();
                    LaunchNotification(content, false);
                    break;
                case 20:// launch notification for hour 8 am
                    content = GetContentFor8pm();
                    LaunchNotification(content, false);
                    break;
                default:
                    bool continueChecking = false;
                    if (hour >= 8)
                    {
                        if (_dailyTarget <= TotalDailySteps)//stepsFromSensor)// DailyTarget achieved
                        {  
                            content = "Felicitari! Target zilnic atins.";
                            LaunchNotification(content, false);
                        }
                        else
                        {
                            if (hour < 16)
                            {
                                if (_currentStepsHh < _halfHourTarget)
                                {
                                    Log.Error("TrackerActivityService", "1. currentHHT: " + _currentStepsHh + " HHT: " + _halfHourTarget);
                                    content = GetContentFor30minsTarget();
                                    LaunchNotification(content, true);
                                }
                                else
                                {
                                    Log.Error("TrackerActivityService", "2. currentHHT: " + _currentStepsHh + " HHT: " + _halfHourTarget);
                                    continueChecking = true; // continue checking 
                                }
                            }
                            else
                            {
                                if (hour > 20 || hour < 20)
                                {
                                    Log.Error("TrackerActivityService", "3. currentHHT: " + _currentStepsHh + " HHT: " + _halfHourTarget);
                                    continueChecking = true; // continue checking before or after 8 pm
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Error("TrackerActivityService", "4. currentHHT: " + _currentStepsHh + " HHT: " + _halfHourTarget);
                        continueChecking = true; // check til hour is 8 am
                    }

                    if (continueChecking)
                    {
                        long minutesTillNextCheck = 60000 * 5;
                        _handler.Interval = minutesTillNextCheck;
                    }
                    break;
            }
        }

        private string GetContentFor30minsTarget()
        {
            string content;
            if (_currentStepsHh == 0)
            {
                content = "Nu ati facut pasi in ultima jumatate de ora.";
            }
            else
            {
                content = "Numar de pasi facuti in ultima jumatate de ora: " + _currentStepsHh;
            }
            return content;
        }

        private static string GetContentFor8pm()
        {
            string content;
            if (_dailyTarget <= TotalDailySteps) // stepsFromSensor)
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
            if (_dailyTarget <= TotalDailySteps) //stepsFromSensor)
            {
                content = "Felicitari! Target zilnic atins.";
            }
            else
            {
                long dif = _dailyTarget - TotalDailySteps; //stepsFromSensor;
                content = "Pasi necesari pana la target: " + dif;
            }

            return content;
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
                long hoursTillNextCheck = 3600000 * 2; //delay 120 minutes
                Log.Error("TrackerActivityService", "interval: " + hoursTillNextCheck);
                _handler.Interval = hoursTillNextCheck;
            }
            else
            {
                _currentStepsHh = 0; // reset
                //delay 30 mins ( 5 for test )
                long minutesTillNextCheck = 60000 * 5; // here will be * 30
                Log.Error("TrackerActivityService", "interval: " + minutesTillNextCheck);
                _handler.Interval = minutesTillNextCheck;
            }
        }

        public override IBinder OnBind(Intent intent){ return null; }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("TrackerActivityService", "started");
            StepCounterSensor sensor = new StepCounterSensor(this);
            sensor.SetListener(this);
            InitStepsTarget();

            _handler.Elapsed += OnTimedEvent;

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
                _dailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception e) //daily target not setted, default is 5000
            {
                _dailyTarget = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", _dailyTarget + "");
            }
            _halfHourTarget = _dailyTarget / 15;
            _currentStepsHh = 0;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if(intent == null)
            {
                OnCreate();
            }
            _handler.Start();
            return StartCommandResult.RedeliverIntent;
        }

        public void OnStepCounterSensorChanged(long count)
        {
            Log.Error("TrackerActivityService", "stepsFromSensor: " + count);
            var hour = DateTime.Now.Hour;
            KeepCountingInRange(count, hour);
            Log.Error("TrackerActivityService", "TotalDailySteps: " + TotalDailySteps);
            _stepsFromSensor = count;
        }

        private void KeepCountingInRange(long count, int hour)
        {
            if (hour >= 1)
            {
                if (_stepsFromSensor != count)
                {
                    _currentStepsHh++;
                    TotalDailySteps++;
                }
            }

            if (hour >= 23)
            {
                TotalDailySteps = 0;
            }
        }
    }
}