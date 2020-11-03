using Android.App;
using Android.Content;
using Android.Util;
using AndroidX.Core.App;
using Familia.Devices.Alarm;

namespace Familia.Helpers {
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class AlarmDeviceReceiver : BroadcastReceiver {
        public static readonly string INTERVAL_CONTENT = "INTERVAL_CONTENT";
        public static readonly string CHANNEL_NAME_ALARM_DEVICE = "measuring devices channel";
        public static readonly string CHANNEL_TITLE_ALARM_DEVICE = "Measuring devices";
        public static readonly string CHANNEL_NAME_BLOODPRESSURE = "BloodPressure Channel";
        public static readonly string TITLE_BLOODPRESSURE = "Tensiune";
        public static readonly string TITLE_GLUCOSE = "Glicemie";
        public static readonly string CONTENT_GLUCOSE = "Vă rugăm să vă măsurați glicemia.";
        public static readonly string CONTENT_BLOOD_PRESSURE = "Vă rugăm să vă măsurați tensiunea.";


        public override void OnReceive(Context context, Intent intent) {
            string content = intent.GetStringExtra(INTERVAL_CONTENT);
            string interval = intent.GetStringExtra("IntervalMilis");
            Log.Error("PPPAAAAAAAAAAAAAAAAAA", "receiver " + content + ", " + interval);
            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            CreateNotificationChannel(CHANNEL_NAME_ALARM_DEVICE, CHANNEL_TITLE_ALARM_DEVICE, CHANNEL_TITLE_ALARM_DEVICE);
            intent= new Intent(context, typeof(MainActivity));
            switch (content) {
                case Constants.IntervalGlucose:
                    intent.PutExtra("extra_health_device", "HealthDevicesFragment");
                    BuildNotification(context, Constants.GlucoseNotifId, CHANNEL_NAME_ALARM_DEVICE,
                        TITLE_GLUCOSE, CONTENT_GLUCOSE, intent);
                    break;
                case Constants.IntervalBloodPressure:
                    intent.PutExtra("extra_health_device", "HealthDevicesFragment");
                    BuildNotification(context, Constants.BloodPressureNotifId, CHANNEL_NAME_ALARM_DEVICE,
                        TITLE_BLOODPRESSURE, CONTENT_BLOOD_PRESSURE, intent);
                    break;
            }

            ConfigReceiver.LaunchAlarm(context, interval, content);

        }

        private static void CreateNotificationChannel(string mChannel, string mTitle, string mContent) {
            string description = mContent;

            var channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.High) {
                    Description = description
                };

            var notificationManager =
                (NotificationManager)Application.Context.GetSystemService(
                    Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);

        }

        private static void BuildNotification(Context context, int notifyId, string channel, string title, string content, Intent intent) {

            Log.Error("PPPAAAAAAAAAAAAAAAAAA", "build notification for " + title + " with id: " + notifyId);

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