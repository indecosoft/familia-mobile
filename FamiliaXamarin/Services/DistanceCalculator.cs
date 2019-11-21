using System;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;
using Familia;
using FamiliaXamarin.Helpers;
using Java.Lang;
using Exception = System.Exception;
using Math = System.Math;
using String = System.String;

namespace FamiliaXamarin.Services
{
    [Service]
    internal class DistanceCalculator : Service
    {
        private double _latitude, _longitude, _currentLongitude, _currentLatitude;
        private bool _started;
        private readonly Handler _handler = new Handler();
        private NotificationManager _mNotificationManager;
        private int _verifications;
        private int _refreshTime = 1000;
        private static DistanceCalculator _ctx;
        private const int ServiceRunningNotificationId = 10001;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            CreateChannels();
            _longitude = double.Parse(Utils.GetDefaults("ConsultLong"));
            _latitude = double.Parse(Utils.GetDefaults("ConsultLat"));
            _started = true;
            _handler.PostDelayed(_runnable, 0);
            _ctx = this;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {

                    string CHANNEL_ID = "my_channel_01";
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Distance Calculator",
                        NotificationImportance.Default);

                    ((NotificationManager)GetSystemService(NotificationService))
                        .CreateNotificationChannel(channel);

                    Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                        .SetContentTitle("Familia")
                        .SetContentText("Asistenta la domiciliu in curs de desfasurare")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(ServiceRunningNotificationId, notification);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            // Enlist this instance of the service as a foreground service
            return StartCommandResult.Sticky;
        }

        private void CreateChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
            var androidChannel = new NotificationChannel("ANDROID_CHANNEL_ID", "ANDROID_CHANNEL_NAME", NotificationImportance.High);
            androidChannel.EnableLights(true);
            androidChannel.EnableVibration(true);
            androidChannel.LightColor = Android.Resource.Color.HoloRedLight;
            androidChannel.LockscreenVisibility = NotificationVisibility.Private;

            GetManager().CreateNotificationChannel(androidChannel);
        }

        private NotificationManager GetManager()
        {
            return _mNotificationManager ??
                   (_mNotificationManager = (NotificationManager) GetSystemService(NotificationService));
        }

        private static NotificationCompat.Builder GetAndroidChannelNotification(string title, string body)
        {
            Intent intent = new Intent(_ctx, typeof(MainActivity));
            //Intent rejectintent = new Intent(this, MenuActivity.class);

            PendingIntent acceptIntent = PendingIntent.GetActivity(_ctx, 1, intent, PendingIntentFlags.OneShot);
            //PendingIntent rejectIntent = PendingIntent.getActivity(this, 1, rejectintent, PendingIntent.FLAG_ONE_SHOT);

                return new NotificationCompat.Builder(_ctx.ApplicationContext, "ANDROID_CHANNEL_ID")
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetSmallIcon(Resource.Drawable.alert)
                    .SetStyle(new NotificationCompat.BigTextStyle()
                        .BigText(body))
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetContentIntent(acceptIntent)
                    .SetAutoCancel(true);

        }

        private readonly Runnable _runnable = new Runnable(() =>
        {
            try
            {
                if (!Utils.GetDefaults("ActivityStart").Equals(""))
                {
                    _ctx._currentLongitude = double.Parse(Utils.GetDefaults("Longitude"));
                    _ctx._currentLatitude = double.Parse(Utils.GetDefaults("Latitude"));
                    Log.Error("consultLog", _ctx._longitude.ToString());
                    Log.Error("consultLat", _ctx._latitude.ToString());

                    var distance = Utils.HaversineFormula(_ctx._latitude, _ctx._longitude, _ctx._currentLatitude, _ctx._currentLongitude);
                    Log.Error("Distance", "" + distance);
//                    Toast.MakeText(_ctx, "" + distance + " metri", ToastLength.Short).Show();
                    if (distance > 180 && distance < 220)
                    {
                        Log.Warn("Distance warning", "mai mult de 200 metri.Esti la " + Math.Round(distance) + " metrii distanta");
                        //Toast.makeText(DistanceCalculator.this, "" + distance, Toast.LENGTH_SHORT).show();
                        _ctx._verifications = 0;
                        NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Ai plecat de la pacient? Esti la " + Math.Round(distance) + " metrii distanta");

                        _ctx.GetManager().Notify(100, nb.Build());
                        _ctx._refreshTime = 15000;
                    }
                    else if (distance > 220)
                    {
                        _ctx._refreshTime = 60000;
                        //Toast.MakeText(_ctx, "" + distance + " metri " + _ctx._verifications, ToastLength.Short).Show();

                        if (_ctx._verifications == 15)
                        {
                            NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Vizita a fost anulata automat!");
                            _ctx.GetManager().Notify(1000, nb.Build());
                            _ctx._verifications = 0;

                            //trimitere date la server
                            Utils.SetDefaults("ActivityStart", "");
                            _ctx.StopService(new Intent(_ctx, typeof(MedicalAsistanceService)));
                            _ctx.StopSelf();

                        }
                        else
                        {
                            NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Vizita va fi anulata automat deoarece te afli la " + Math.Round(distance) + " metrii distanta de pacient! Mai ai " + (15 - _ctx._verifications) + " minute sa te intorci!");
                            _ctx.GetManager().Notify(1000, nb.Build());
                            _ctx._verifications++;
                        }
                    }
                    else
                    {
                        //                    //Toast.makeText(DistanceCalculator.this, Math.round(distance) + " metri. Inca esti bine ;)", Toast.LENGTH_SHORT).show();
                        _ctx._verifications = 0;
                    }
                    if (_ctx._started)
                    {
                        _ctx._handler.PostDelayed(_ctx._runnable, _ctx._refreshTime);
                    }
                }
                else
                {
                    _ctx._started = false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error occurred in {nameof(DistanceCalculator)} service: ", e.Message);
                _ctx._started = false;
                _ctx.StopSelf();

            }
          
        });

    }
}