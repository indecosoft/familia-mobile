using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Location;
using Familia.Medicatie.Alarm;

namespace Familia.Services {
    [Service]
    internal class LocationService : Service {

        public override IBinder OnBind(Intent intent) {
            throw new NotImplementedException();
        }

        public override void OnCreate() {
            Log.Info("Location Service", "OnCreate: the service is initializing.");
            try {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {

                    Notification notification = new NotificationCompat.Builder(this, App.NonStopChannelIdForServices)
                        .SetContentTitle("Familia")
                        .SetContentText("Utilizeaza locatia")
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetOngoing(true)
                        .Build();

                    StartForeground(App.NonstopNotificationIdForServices, notification);
                }

                Task.Run(async () => await LocationManager.Instance.StartRequestingLocation());
            } catch (Exception e) {
                Log.Error("Location Service ON CREATE ERROR", e.Message);
            }
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
            Log.Info("Location Service", "Started");

            return StartCommandResult.Sticky;
        }
    }
}