using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin.Services
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

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "WebSocketService STARTED");

            try
            {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    string CHANNEL_ID = "my_channel_01";
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Channel human readable title",
                        NotificationImportance.Default);

                    ((NotificationManager)GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);

                    Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }

        
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