﻿using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Math = System.Math;
using String = System.String;

namespace FamiliaXamarin
{
    [Service]
    class DistanceCalculator : Service
    {
        double Latitude, Longitude, CurrentLongitude, CurrentLatitude;
        bool started;
        Handler handler = new Handler();
        NotificationManager mNotificationManager;
        int Verifications;
        int RefreshTime = 1000;
        static DistanceCalculator _ctx;
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public override IBinder OnBind(Intent intent)
        {
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
        }

        public override void OnCreate()
        {
            CreateChannels();
            Longitude = double.Parse(Utils.GetDefaults("ConsultLong", this));
            Latitude = double.Parse(Utils.GetDefaults("ConsultLat", this));
            started = true;
            handler.PostDelayed(runnable, 0);
            _ctx = this;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var notification = new Notification.Builder(this)
#pragma warning restore CS0618 // Type or member is obsolete
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("Ruleaza in fundal")
                .SetSmallIcon(Resource.Drawable.logo)
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            return StartCommandResult.Sticky;
        }

        void CreateChannels()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                NotificationChannel androidChannel = new NotificationChannel("ANDROID_CHANNEL_ID", "ANDROID_CHANNEL_NAME", NotificationImportance.High);
#pragma warning restore XA0001 // Find issues with Android API usage
                androidChannel.EnableLights(true);
                androidChannel.EnableVibration(true);
                androidChannel.LightColor = Android.Resource.Color.HoloRedLight;
                androidChannel.LockscreenVisibility = NotificationVisibility.Private;

#pragma warning disable XA0001 // Find issues with Android API usage
                GetManager().CreateNotificationChannel(androidChannel);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
        }
        NotificationManager GetManager()
        {
            if (mNotificationManager == null)
            {
                mNotificationManager = (NotificationManager)GetSystemService(NotificationService);
            }
            return mNotificationManager;
        }
        static NotificationCompat.Builder GetAndroidChannelNotification(String title, String body)
        {
            Intent intent = new Intent(_ctx, typeof(MainActivity));
            //Intent rejectintent = new Intent(this, MenuActivity.class);

            PendingIntent acceptIntent = PendingIntent.GetActivity(_ctx, 1, intent, PendingIntentFlags.OneShot);
            //PendingIntent rejectIntent = PendingIntent.getActivity(this, 1, rejectintent, PendingIntent.FLAG_ONE_SHOT);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
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
            else
            {
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
        }

        Runnable runnable = new Runnable(() =>
        {
            if (!Utils.GetDefaults("ActivityStart", _ctx).Equals(""))
            {
                _ctx.CurrentLongitude = double.Parse(Utils.GetDefaults("Longitude", _ctx));
                _ctx.CurrentLatitude = double.Parse(Utils.GetDefaults("Latitude", _ctx));


                double distance = Utils.HaversineFormula(_ctx.Latitude, _ctx.Longitude, _ctx.CurrentLatitude, _ctx.CurrentLongitude);
                Log.Error("Distance", "" + distance);
                Log.Debug("ConsultLatitude ", _ctx.Latitude.ToString());
                Log.Debug("ConsultLongitude", _ctx.Longitude.ToString());
                Log.Debug("CurrentLatitude ", _ctx.CurrentLatitude.ToString());
                Log.Debug("CurrentLongitude", _ctx.CurrentLongitude.ToString());
                //Toast.MakeText(_ctx, "" + distance + " metri", ToastLength.Short).Show();
                if (distance > 180 && distance < 220)
                {
                    Log.Warn("Distance warning", "mai mult de 200 metri.Esti la " + Math.Round(distance) + " metrii distanta");
                    //Toast.makeText(DistanceCalculator.this, "" + distance, Toast.LENGTH_SHORT).show();
                    _ctx.Verifications = 0;
                    NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Ai plecat de la pacient? Esti la " + Math.Round(distance) + " metrii distanta");

                    _ctx.GetManager().Notify(100, nb.Build());
                    _ctx.RefreshTime = 15000;
                }
                else if (distance > 220)
                {
                    _ctx.RefreshTime = 60000;
                    //Toast.MakeText(_ctx, "" + distance + " metri " + _ctx.Verifications, ToastLength.Short).Show();

                    if (_ctx.Verifications == 15)
                    {
                        NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Vizita a fost anulata automat!");
                        _ctx.GetManager().Notify(100, nb.Build());
                        _ctx.Verifications = 0;

                        //trimitere date la server
                        Utils.SetDefaults("ActivityStart", "", _ctx);
                        _ctx.StopSelf();

                    }
                    else
                    {
                        NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment", "Vizita va fi anulata automat deoarece te afli la " + Math.Round(distance) + " metrii distanta de pacient! Mai ai " + (15 - _ctx.Verifications) + " minute sa te intorci!");
                        _ctx.GetManager().Notify(100, nb.Build());
                        _ctx.Verifications++;
                    }

                }
                else
                {
                    //                    //Toast.makeText(DistanceCalculator.this, Math.round(distance) + " metri. Inca esti bine ;)", Toast.LENGTH_SHORT).show();
                    _ctx.Verifications = 0;
                }
                if (_ctx.started)
                {
                    _ctx.handler.PostDelayed(_ctx.runnable, _ctx.RefreshTime);
                }
            }
            else
            {
                _ctx.started = false;
            }
        });

    }
}