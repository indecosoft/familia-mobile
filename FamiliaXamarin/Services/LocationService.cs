using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;

using Exception = System.Exception;
using Resource = Familia.Resource;

namespace FamiliaXamarin.Services {
    [Service]
    internal class LocationService : Service
    {
        private const int ServiceRunningNotificationId = 10000;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            Log.Info("Location Service", "OnCreate: the service is initializing.");
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    string CHANNEL_ID = "my_channel_01";
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Location",
                        NotificationImportance.Default)
                        { Importance = NotificationImportance.Low };

                    ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);

                    Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                        .SetContentTitle("Familia")
                        .SetContentText("Ruleaza in fundal")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(ServiceRunningNotificationId, notification);
                }
                _ = Familia.Location.LocationManager.Instance.StartRequestingLocation();
            }
            catch (Exception e)
            {
                Log.Error("Location Service ON CREATE ERROR", e.Message);
            }
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("Location Service", "Started");

            return StartCommandResult.Sticky;
        }
    }
}