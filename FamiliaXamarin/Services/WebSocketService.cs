using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace Familia.Services
{
    [Service]
    class WebSocketService : Service
    {
        //private NotificationManager _notificationManager;
        readonly IWebSocketClient _socketClient = new WebSocketClient();
        private const int ServiceRunningNotificationId = 10000;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override async void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "WebSocketService STARTED");

            try
            {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    const string channelId = "my_channel_01";
                    var channel = new NotificationChannel(channelId, "Channel human readable title",
                        NotificationImportance.Default);

                    ((NotificationManager)GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);

                    var notification = new NotificationCompat.Builder(this, channelId)
                        .SetContentTitle("Familia")
                        .SetContentText("Ruleaza in fundal")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(ServiceRunningNotificationId, notification);
                }
                var charger = new ChargerReceiver();
                RegisterReceiver(charger, new IntentFilter(Intent.ActionHeadsetPlug));

                _socketClient.Connect(Constants.WebSocketAddress, Constants.WebSocketPort, this);

                await Task.Run(async () =>
                {
                    try
                    {
                        var data = await GetData();
                        var obj = new JSONObject(data);
                        var idPersoana = obj.GetInt("idPersoana");
                        Utils.SetDefaults("IdPersoana", idPersoana.ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Configuration Error", "Error getting the configuration file from GIS");
                    }

                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }

        
        }


        private static async Task<string> GetData()
        {
            if (!Utils.CheckNetworkAvailability()) return null;
            var result =
                await WebServices.Get(
                    $"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetImei(Application.Context)}");
            //               var result = await WebServices.Get(
            //                   $"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetImei(Application.Context)}",
            //                   Utils.GetDefaults("Token", context));
            if (result == null) return null;
            Log.Error("RESULT_FROM_GIS", result);
            return result;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("WebSocket Service", "Started");

//            var notification = new NotificationCompat.Builder(this)
//                .SetContentTitle(Resources.GetString(Resource.String.app_name))
//                .SetContentText("Ruleaza in fundal")
//                .SetSmallIcon(Resource.Drawable.logo)
//                .SetOngoing(true)
//                .Build();
//
//            // Enlist this instance of the service as a foreground service
//            StartForeground(ServiceRunningNotificationId, notification);
            return StartCommandResult.Sticky;
        }


    }
}