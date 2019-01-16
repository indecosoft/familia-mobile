﻿using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Location;

namespace FamiliaXamarin.Services
{
    [Service]
    internal class LocationService : Service 
    {
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private LocationCallback _locationCallback;
        private LocationRequest _locationRequest;

        private bool _isGooglePlayServicesInstalled;
        private bool _isRequestingLocationUpdates;
        private const int ServiceRunningNotificationId = 10000;


        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
        public override void OnCreate()
        {
            Log.Info("Service", "OnCreate: the service is initializing.");
            try
            {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    string CHANNEL_ID = "my_channel_01";
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Channel human readable title",
                        NotificationImportance.Default);

                    ((NotificationManager)GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);

                    Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                        .SetContentTitle("Familia")
                        .SetContentText("Ruleaza in fundal")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(ServiceRunningNotificationId, notification);
                }

                _isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(this);

                if (!_isGooglePlayServicesInstalled) return;
                _locationRequest = new LocationRequest()
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetInterval(1000 * 60 * 30)
                    .SetFastestInterval(1000 * 60 * 30);
                _locationCallback = new FusedLocationProviderCallback(this);

                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                RequestLocationUpdatesButtonOnClick();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            //
            //
            //            var notification = new NotificationCompat.Builder(ApplicationContext)
            //                .SetContentTitle("Familia")
            //                .SetContentText("Ruleaza in fundal")
            //                .SetSmallIcon(Resource.Drawable.logo)
            //                .SetOngoing(true)
            //                .Build();
            //
            //            // Enlist this instance of the service as a foreground service
            //            StartForeground(ServiceRunningNotificationId, notification);


        }

        private async void RequestLocationUpdatesButtonOnClick()
        {
            // No need to request location updates if we're already doing so.
            if (_isRequestingLocationUpdates)
            {
                StopRequestionLocationUpdates();
                _isRequestingLocationUpdates = false;
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) !=
                    Permission.Granted) return;
                await StartRequestingLocationUpdates();
                _isRequestingLocationUpdates = true;
            }
        }

        private async Task StartRequestingLocationUpdates()
        {
            await _fusedLocationProviderClient.RequestLocationUpdatesAsync(_locationRequest, _locationCallback);
        }

        private async void StopRequestionLocationUpdates()
        {

            if (_isRequestingLocationUpdates)
            {
                await _fusedLocationProviderClient.RemoveLocationUpdatesAsync(_locationCallback);
            }
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("Location Service", "Started");

            return StartCommandResult.Sticky;
        }
    }
}