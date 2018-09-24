using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using String = System.String;

namespace FamiliaXamarin
{
    [Service]
    class LocationService : Service, ILocationListener
    {
        private LocationManager _locationManager;
        private string _provider = LocationManager.GpsProvider;
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _locationManager.RemoveUpdates(this);
        }

        public override void OnCreate()
        {
            Log.Info("Service", "OnCreate: the service is initializing.");
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Location Service", "Started");
//            var locationCriteria = new Criteria {Accuracy = Accuracy.High, PowerRequirement = Power.High};
//
//            var locationProvider = _locationManager.GetBestProvider(locationCriteria, true);
//
//            if (locationProvider != null)
//            {
//                _locationManager.RequestLocationUpdates(locationProvider, 2000, 1, this);
//            }
//            else
//            {
//                //Log.Info(tag, "No location providers available");
//            }
//
            _locationManager = (LocationManager)GetSystemService(Context.LocationService);
            // For this example, this method is part of a class that implements ILocationListener, described below
            _locationManager.RequestLocationUpdates(_provider, 1000, 0, this);
            return StartCommandResult.NotSticky;
        }

        public void OnLocationChanged(Location location)
        {
            Toast.MakeText(this, "Provider: "+_provider+" Latitude: "+location.Latitude.ToString(CultureInfo.InvariantCulture) + "Longitude: " + location.Longitude.ToString(CultureInfo.InvariantCulture), ToastLength.Short).Show();
        }

        public void OnProviderDisabled(string provider)
        {
            if (provider == LocationManager.GpsProvider)
            {
                _provider = LocationManager.NetworkProvider;
            }
            else
            {
                _provider = LocationManager.GpsProvider;
            }
            _locationManager.RemoveUpdates(this);
            _locationManager.RequestLocationUpdates(_provider, 1000, 0, this);
            Toast.MakeText(this, "Provider Disabled: " + _provider , ToastLength.Short).Show();

        }

        public void OnProviderEnabled(string provider)
        {
            if (provider == LocationManager.GpsProvider)
            {
                _provider = LocationManager.NetworkProvider;
            }
            else
            {
                _provider = LocationManager.GpsProvider;
            }
            _locationManager.RemoveUpdates(this);
            _locationManager.RequestLocationUpdates(_provider, 1000, 0, this);
            Toast.MakeText(this, "Provider Enabled: " + _provider, ToastLength.Short).Show();
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            if (provider == LocationManager.NetworkProvider && (status == Availability.TemporarilyUnavailable || status == Availability.OutOfService))
            {
                _provider = LocationManager.GpsProvider;
                
            }
            else if(provider == LocationManager.GpsProvider && (status == Availability.TemporarilyUnavailable || status == Availability.OutOfService))
            {
                _provider = LocationManager.NetworkProvider;
            }
            _locationManager.RemoveUpdates(this);
            _locationManager.RequestLocationUpdates(_provider, 1000, 0, this);
            Log.Error("Provider", _provider + " Status: " + status);
            Toast.MakeText(this, "Provider Status: "  + _provider + " Status: " + status, ToastLength.Short).Show();
        }
    }
}