using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace FamiliaXamarin.Services
{
    [Service]
    class SmartBandService : Service
    {
        private const int ServiceRunningNotificationId = 10000;
        private string _token;
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "WebSocketService STARTED");

        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("SmartBand Service", "Started");

            var notification = new NotificationCompat.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("Ruleaza in fundal")
                .SetSmallIcon(Resource.Drawable.logo)
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(ServiceRunningNotificationId, notification);

            string refreshToken = Utils.GetDefaults("FitbitRefreshToken", this);
            Task.Run(() => {
                var dict = new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                };
                var response = WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                if (response != null)
                {
                    var obj = new JSONObject(response);
                    _token = obj.GetString("access_token");
                    var newRefreshToken = obj.GetString("refresh_token");
                    var userId = obj.GetString("user_id");
                    Utils.SetDefaults(GetString(Resource.String.smartband_device), _token, this);
                    Utils.SetDefaults("FitbitToken", _token, this);
                    Utils.SetDefaults("RitbitRefreshToken", newRefreshToken, this);
                    Utils.SetDefaults("FitbitUserId", userId, this);
                }

            });
            return StartCommandResult.Sticky;
        }

    }
}