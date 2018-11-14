using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;


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
            Log.Error("Service:", "STARTED");
            _socketClient.Connect(Constants.WebSocketAddress, Constants.WebSocketPort, this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Location Service", "Started");


            var notification = new NotificationCompat.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("Ruleaza in fundal")
                .SetSmallIcon(Resource.Drawable.logo)
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(ServiceRunningNotificationId, notification);
            return StartCommandResult.Sticky;
        }


    }
}