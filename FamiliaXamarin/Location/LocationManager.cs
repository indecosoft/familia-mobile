using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using Familia.Helpers;
using Org.Json;

namespace Familia.Location {
    public sealed class LocationManager : ILocationEvents {

        private static readonly Lazy<LocationManager>
        Lazy =
        new Lazy<LocationManager>(() => new LocationManager());

        public static LocationManager Instance => Lazy.Value;
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private LocationCallback _locationCallback;
        private LocationRequest _locationRequest;
        private bool _isRequestingLocationUpdates;
        public delegate void LocationEventHandler(object source , LocationEventArgs args);
        public event LocationEventHandler LocationRequested;

        public async Task StartRequestingLocation(int miliseconds = 1000 * 60 * 30) {
            Log.Error("Starting location" , miliseconds.ToString());
            if (!Utils.CheckIfLocationIsEnabled()) {
                Toast.MakeText(Application.Context , "Nu aveti locatia activata" , ToastLength.Long)?.Show();
                return;
            }

            if (!Utils.IsGooglePlayServicesInstalled(Application.Context)) return;
            if (_fusedLocationProviderClient is null || _locationRequest is null) {
                _locationRequest = new LocationRequest()
               .SetPriority(LocationRequest.PriorityHighAccuracy);
                _locationCallback = new FusedLocationProviderCallback(this);

                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Application.Context);
            }
            await ChangeInterval(miliseconds);
        }
        public async Task ChangeInterval(int miliseconds = 1000 * 60 * 30) {
            if (_locationRequest != null && _fusedLocationProviderClient != null) {
                _locationRequest.SetFastestInterval(miliseconds);
                _locationRequest.SetInterval(miliseconds);
                _locationRequest.SetMaxWaitTime(miliseconds + 10000);
                await RequestLocationUpdates();
            }
        }
        private async Task RequestLocationUpdates() {
            if (ContextCompat.CheckSelfPermission(Application.Context , Manifest.Permission.AccessFineLocation) !=
                    Permission.Granted) return;
            await _fusedLocationProviderClient.RequestLocationUpdatesAsync(_locationRequest , _locationCallback);
            _isRequestingLocationUpdates = true;
        }

        public async Task StopRequestionLocationUpdates() {
            if (_isRequestingLocationUpdates) {
                _isRequestingLocationUpdates = false;
                await _fusedLocationProviderClient.RemoveLocationUpdatesAsync(_locationCallback);
            }
        }

        public void OnLocationRequested(LocationEventArgs args) {
            OnLocationRequested(args.Location);
            if (!Utils.CheckNetworkAvailability()) return;
            JSONObject obj = new JSONObject().Put("latitude" , args.Location.Latitude).Put("longitude" , args.Location.Longitude);
            JSONObject finalObj = new JSONObject().Put("idUser" , Utils.GetDefaults("Id")).Put("location" , obj);
            try {
                Task.Run(() => {
                    _ = WebServices.WebServices.Post("/api/updateLocation" , finalObj , Utils.GetDefaults("Token"));
                    Utils.SetDefaults("Latitude" , args.Location.Latitude.ToString());
                    Utils.SetDefaults("Longitude" , args.Location.Longitude.ToString());
                    Log.Debug("Latitude " , args.Location.Latitude.ToString());
                    Log.Debug("Longitude" , args.Location.Longitude.ToString());
                });
            } catch (Exception e) {
                Log.Error("Error sending location to server" , e.Message);
            }
        }

        private void OnLocationRequested(Android.Locations.Location location) {
            LocationRequested?.Invoke(this , new LocationEventArgs {
                Location = location
            });
        }

    }
}
