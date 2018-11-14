using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Location;
using Android.Util;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace FamiliaXamarin.Location
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


        public override async void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                Utils.SetDefaults("Latitude", location.Latitude.ToString(), _activity);
                Utils.SetDefaults("Longitude", location.Longitude.ToString(), _activity);
                if (Utils.CheckNetworkAvailability())
                {
                    JSONObject obj = new JSONObject().Put("latitude", location.Latitude).Put("longitude", location.Longitude);
                    JSONObject finalObj = new JSONObject().Put("idUser", Utils.GetDefaults("IdClient", _activity)).Put("location", obj);
                    try
                    {
                        await Task.Run(async () =>
                        {
                            string p = await WebServices.Post(Constants.PublicServerAddress + "/api/updateLocation", finalObj, Utils.GetDefaults("Token", _activity));
                            Log.Debug("Latitude ", location.Latitude.ToString());
                            Log.Debug("Longitude", location.Longitude.ToString());
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Error("****************************", e.Message);
                    }
                    
                    
                }
                
            }

        }
    }
}