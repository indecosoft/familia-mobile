using System.Linq;
using Android.Content;
using Android.Gms.Location;
using Android.Util;
using FamiliaXamarin.Helpers;
using Org.Json;

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


        public async override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                Utils.SetDefaults("Latitude", location.Latitude.ToString(), _activity);
                Utils.SetDefaults("Longitude", location.Longitude.ToString(), _activity);
                JSONObject obj = new JSONObject().Put("latitude", location.Latitude).Put("longitude", location.Longitude);
                JSONObject finalObj = new JSONObject().Put("idUser", Utils.GetDefaults("IdClient", _activity)).Put("location", obj);
                string p = await WebServices.Post(Constants.PublicServerAddress + "/api/updateLocation", finalObj,
                    Utils.GetDefaults("Token", _activity));
                Log.Debug("Latitude ", location.Latitude.ToString());
                Log.Debug("Longitude", location.Longitude.ToString());
            }
            else
            {
                Utils.SetDefaults("Latitude", null, _activity);
                Utils.SetDefaults("Longitude", null, _activity);
//                JSONObject obj = new JSONObject().Put("latitude", null).Put("longitude", null);
//                JSONObject finalObj = new JSONObject().Put("idUser", Utils.GetDefaults("IdClient", _activity)).Put("location", obj);
//                string p = await WebServices.Post(Constants.PublicServerAddress + "/api/updateLocation", finalObj,
//                    Utils.GetDefaults("Token", _activity));
            }
        }
    }
}