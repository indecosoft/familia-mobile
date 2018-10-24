using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Org.Json;

namespace FamiliaXamarin
{
    [Service]
    class WebSocketService : Service
    {
        //private NotificationManager _notificationManager;
        private readonly IWebSocketClient _socketClient = new WebSocketClient();
        public const int ServiceRunningNotificationId = 10000;

        Context Ctx;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            //Utils.CreateChannels();
            Log.Error("Service:", "STARTED");
            //        WebSoketClientClass.ChatSocket.on("start chat", onStartChat);
            //        WebSoketClientClass.ChatSocket.on("join room", onJoinRoom);
            //        WebSoketClientClass.ChatSocket.on("send room", onSendRoom);
            _socketClient.Connect(Constants.WebSocketAddress, Constants.WebSocketPort, this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Location Service", "Started");

            var notification = new Notification.Builder(this)
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