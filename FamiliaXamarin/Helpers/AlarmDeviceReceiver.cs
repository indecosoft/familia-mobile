using Android.App;
using Android.Content;
using Android.Support.V4.App;

namespace Familia.Helpers {
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class AlarmDeviceReceiver : BroadcastReceiver {
        public static readonly string INTERVAL_CONTENT = "INTERVAL_CONTENT";
        public static readonly string CHANNEL_NAME_ALARM_DEVICE = "Alarm device channel";
        public static readonly string CHANNEL_NAME_BLOODPRESSURE = "BloodPressure Channel";
        public static readonly string TITLE_BLOODPRESSURE = "Tensiune";
        public static readonly string TITLE_GLUCOSE = "Glicemie";
        public static readonly string CONTENT_GLUCOSE = "Vă rugăm să vă măsurați glicemia.";
        public static readonly string CONTENT_BLOODPRESSURE = "Vă rugăm să vă măsurați tensiunea.";


        public override void OnReceive(Context context, Intent intent) {
            string content = intent.GetStringExtra(INTERVAL_CONTENT);
            string intervalMilis = intent.GetStringExtra("IntervalMilis");

            //Log.Error("PPPAAAAAAAAAAAAAAAAAA", "receiver " + content);

            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            CreateNotificationChannel(CHANNEL_NAME_ALARM_DEVICE, TITLE_GLUCOSE, CONTENT_GLUCOSE);


            if (content.Equals(Constants.IntervalGlucose)) {
                var intentGlucose = new Intent(context, typeof(MainActivity));
                intentGlucose.PutExtra("extra_health_device", "HealthDevicesFragment");
                //CreateNotificationChannel(CHANNEL_NAME_GLUCOSE, TITLE_GLUCOSE, CONTENT_GLUCOSE);
                BuildNotification(context, Constants.GlucoseNotifId, CHANNEL_NAME_ALARM_DEVICE,
                    TITLE_GLUCOSE, CONTENT_GLUCOSE, intentGlucose);
            } else {
                if (!content.Equals(Constants.IntervalBloodPressure)) return;
                var intentBloodPressure = new Intent(context, typeof(MainActivity));
                intentBloodPressure.PutExtra("extra_health_device", "HealthDevicesFragment");

                BuildNotification(context, Constants.BloodPressureNotifId, CHANNEL_NAME_ALARM_DEVICE,
                    TITLE_BLOODPRESSURE, CONTENT_BLOODPRESSURE, intentBloodPressure);
            }

        }

        private static void CreateNotificationChannel(string mChannel, string mTitle, string mContent) {
            string description = mContent;

            var channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.Default) {
                    Description = description
                };

            // Log.Error("PPPAAAAAAAAAAAAAAAAAA", "create channel for " + mTitle);
            var notificationManager =
                (NotificationManager)Application.Context.GetSystemService(
                    Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);

        }

        private static void BuildNotification(Context context, int notifyId, string channel, string title, string content, Intent intent) {

            //Log.Error("PPPAAAAAAAAAAAAAAAAAA", "build notification for " + title + " with id: " + notifyId);

            PendingIntent piNotification = PendingIntent.GetActivity(context, notifyId, intent, PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.water)
                    .SetContentText(content)
                    .SetContentTitle(title)
                    .SetAutoCancel(true)
                    .SetContentIntent(piNotification)
                    .SetPriority(NotificationCompat.PriorityHigh);

            if (title.Equals(TITLE_BLOODPRESSURE)) {
                mBuilder.SetSmallIcon(Resource.Drawable.heart);
            } else {
                mBuilder.SetSmallIcon(Resource.Drawable.water);
            }

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);

            notificationManager.Notify(notifyId, mBuilder.Build());
        }

    }
}