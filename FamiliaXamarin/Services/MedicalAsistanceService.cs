using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Location;

namespace FamiliaXamarin.Services
{
    [Service]
    class MedicalAsistanceService :Service
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

            _isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(this);


            if (!_isGooglePlayServicesInstalled) return;
            _locationRequest = new LocationRequest()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(0)
                .SetFastestInterval(0);
            _locationCallback = new MedicalAsistanceFusedLocationProviderCallback(this);

            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            RequestLocationUpdatesButtonOnClick();
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

            Log.Info("MedicalAsistent Service", "Started");

            var notification = new NotificationCompat.Builder(this)
                .SetContentTitle("Familia")
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