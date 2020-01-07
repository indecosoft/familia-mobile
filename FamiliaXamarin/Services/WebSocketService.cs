using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.WebSocket;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace Familia.Services {
    [Service]
    class WebSocketService : Service {
        readonly IWebSocketClient _socketClient = new WebSocketClient();
        readonly IWebSocketClient webSocketLocation = new WebSocketLocation();
        private const int ServiceRunningNotificationId = 10000;
        private readonly ChargerReceiver charger = new ChargerReceiver();
        public override IBinder OnBind(Intent intent) => throw new NotImplementedException();

        public override void OnDestroy() {
            base.OnDestroy();
            UnregisterReceiver(charger);
        }

        public override async void OnCreate() {
            base.OnCreate();
            Log.Error("Service:", "WebSocketService STARTED");

            try {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                    const string channelId = "my_channel_01";
                    var channel = new NotificationChannel(channelId, "WebSocket",
                        NotificationImportance.Default) { Importance = NotificationImportance.Low };

                    ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);

                    using var notification = new NotificationCompat.Builder(this, channelId)
                        .SetContentTitle("Familia")
                        .SetContentText("Ruleaza in fundal")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(ServiceRunningNotificationId, notification);
                }
                RegisterReceiver(charger, new IntentFilter(Intent.ActionHeadsetPlug));
                bool ok = int.TryParse(Utils.GetDefaults("UserType"), out var type);
                if (ok) {
                    if (type != 2)
                        await _socketClient.ConnectAsync(Constants.WebSocketAddress, Constants.WebSocketPort, this);
                }
                webSocketLocation.Connect(Constants.WebSocketLocationAddress, Constants.WebSocketPort, this);
                await Task.Run(async () => {
                    try {
                        var result = await GetData();
                        if (result == "{}") throw new Exception("Error geting configuration from GIS");
                        Utils.SetDefaults("IdPersoana", new JSONObject(result).GetInt("idPersoana").ToString());
                    } catch (Exception ex) {
                        Log.Error("Configuration Error", ex.Message);
                    }

                });

            } catch (Exception e) {
                Console.WriteLine(e);
                //throw;
            }
        }


        private static async Task<string> GetData() {
            if (!Utils.CheckNetworkAvailability()) return "{}";
            return await WebServices.Get(
                    $"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetDeviceIdentificator(Application.Context)}") ?? "{}";
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) => StartCommandResult.Sticky;
    }
}