using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace FamiliaXamarin
{
    [Service]
    class WebSocketService : Service
    {
        //private NotificationManager _notificationManager;
        readonly IWebSocketClient _socketClient = new WebSocketClient();
        public const int ServiceRunningNotificationId = 10000;

        public override IBinder OnBind(Intent intent)
        {
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
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

#pragma warning disable CS0618 // Type or member is obsolete
            var notification = new Notification.Builder(this)
#pragma warning restore CS0618 // Type or member is obsolete
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