using System.Linq;
using Android.Content;
using Android.Gms.Location;
using Android.Util;

namespace FamiliaXamarin
{
    public class FusedLocationProviderCallback : LocationCallback
    {
        readonly Context _activity;

        public FusedLocationProviderCallback(Context activity)
        {
            _activity = activity;
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }


        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                Utils.SetDefaults("Latitude", location.Latitude.ToString(), _activity);
                Utils.SetDefaults("Longitude", location.Longitude.ToString(), _activity);
                Log.Debug("Latitude ", location.Latitude.ToString());
                Log.Debug("Longitude", location.Longitude.ToString());
            }
            else
            {
                Utils.SetDefaults("Latitude", null, _activity);
                Utils.SetDefaults("Longitude", null, _activity);
            }
        }
    }
}