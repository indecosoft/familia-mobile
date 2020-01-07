using System.Linq;
using Android.Gms.Location;
using Android.Util;
using Familia.Helpers;

namespace FamiliaXamarin.Location {
    public class FusedLocationProviderCallback : LocationCallback {
        private readonly ILocationEvents locationEvents;
        public FusedLocationProviderCallback(ILocationEvents listener) {
            locationEvents = listener;
        }
        public override void OnLocationAvailability(LocationAvailability locationAvailability) {
            Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }
        public override void OnLocationResult(LocationResult result) {
            if (!result.Locations.Any() || result.Locations is null) return;
            locationEvents.OnLocationRequested(this, new Familia.Location.LocationEventArgs {
                Location = result.Locations.First()
            });
        }
    }
}