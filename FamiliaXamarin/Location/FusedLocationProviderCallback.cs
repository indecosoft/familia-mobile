using System.Linq;
using Android.Gms.Location;
using Android.Util;

namespace Familia.Location {
	public class FusedLocationProviderCallback : LocationCallback {
		private readonly ILocationEvents _locationEvents;

		public FusedLocationProviderCallback(ILocationEvents listener) {
			_locationEvents = listener;
		}

		public override void OnLocationAvailability(LocationAvailability locationAvailability) {
			Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}",
				locationAvailability.IsLocationAvailable);
		}

		public override void OnLocationResult(LocationResult result) {
			if (!result.Locations.Any()) return;
			_locationEvents.OnLocationRequested(new LocationEventArgs {Location = result.Locations.First()});
		}
	}
}