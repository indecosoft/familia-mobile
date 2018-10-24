﻿using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Gms.Location;
using Android.Support.V4.Content;

namespace FamiliaXamarin
{
    [Service]
    class LocationService : Service 
    {
        FusedLocationProviderClient fusedLocationProviderClient;
        LocationCallback locationCallback;
        LocationRequest locationRequest;

        const long ONE_MINUTE = 60 * 1000;
        const long FIVE_MINUTES = 5 * ONE_MINUTE;
        const long TWO_MINUTES = 2 * ONE_MINUTE;
        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;
        public const int ServiceRunningNotificationId = 10000;


        public override IBinder OnBind(Intent intent)
        {
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //_locationManager.RemoveUpdates(this);
        }

        public override void OnCreate()
        {
            Log.Info("Service", "OnCreate: the service is initializing.");
            
            isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(this);
            
            

            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                                  .SetPriority(LocationRequest.PriorityHighAccuracy)
                                  .SetInterval(0)
                                  .SetFastestInterval(0);
                locationCallback = new FusedLocationProviderCallback(this);

                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                RequestLocationUpdatesButtonOnClick();
            }

        }
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RequestLocationUpdatesButtonOnClick()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            // No need to request location updates if we're already doing so.
            if (isRequestingLocationUpdates)
            {
                StopRequestionLocationUpdates();
                isRequestingLocationUpdates = false;
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    await StartRequestingLocationUpdates();
                    isRequestingLocationUpdates = true;
                }
            }
        }
        async Task StartRequestingLocationUpdates()
        {
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void StopRequestionLocationUpdates()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {

            if (isRequestingLocationUpdates)
            {
                await fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
            }
        }
        async Task GetLastLocationFromDevice()
        {
            // This method assumes that the necessary run-time permission checks have succeeded.
            //getLastLocationButton.SetText(Resource.String.getting_last_location);
            Location location = await fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // Seldom happens, but should code that handles this scenario
            }
            else
            {
                // Do something with the location 
                Log.Debug("Sample", "The latitude is " + location.Latitude);
            }
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Location Service", "Started");

#pragma warning disable CS0618 // Type or member is obsolete
            var notification = new Notification.Builder(this)
#pragma warning restore CS0618 // Type or member is obsolete
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("Ruleaza in fundal")
                .SetSmallIcon(Resource.Drawable.logo)
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(ServiceRunningNotificationId, notification);
            return StartCommandResult.Sticky;
        }
    }
}