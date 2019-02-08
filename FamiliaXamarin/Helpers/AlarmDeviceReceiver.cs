using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Devices;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;

namespace FamiliaXamarin.Helpers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class AlarmDeviceReceiver : BroadcastReceiver
    {
        public static readonly string INTERVAL_CONTENT = "INTERVAL_CONTENT";
        public static readonly string CHANNEL_NAME_GLUCOSE = "Channel glucose";
        public static readonly string CHANNEL_NAME_BLOODPRESSURE = "Channel bloodpressure";
        public static readonly string TITLE_BLOODPRESSURE = "Tensiunea";
        public static readonly string TITLE_GLUCOSE = "Glicemie";
        public static readonly string CONTENT_GLUCOSE = "Va rugam sa va masurati glicemia.";
        public static readonly string CONTENT_BLOODPRESSURE = "Va rugam sa va masurati tensiunea.";

        public static int NotifyId = Constants.NotificationAlarmDevice;

        public override void OnReceive(Context context, Intent intent)
        {
            var content = intent.GetStringExtra(INTERVAL_CONTENT);

            Log.Error("INTERVAL_CONTENT", content);
            Toast.MakeText(context, content, ToastLength.Short).Show();

            if (content.Equals(Constants.IntervalGlucose))
            {
                var intentGlucose = new Intent(context, typeof(MainActivity));
                intentGlucose.PutExtra("extra", "HealthDevicesFragment");
                CreateNotificationChannel(CHANNEL_NAME_GLUCOSE, TITLE_GLUCOSE, CONTENT_GLUCOSE);
                BuildNotification(context, Constants.GlucoseNotifId, CHANNEL_NAME_GLUCOSE,
                    TITLE_GLUCOSE, CONTENT_GLUCOSE, intentGlucose);

            }

            if (!content.Equals(Constants.IntervalBloodPressure)) return;
            var intentBloodPressure = new Intent(context, typeof(MainActivity));
            intentBloodPressure.PutExtra("extra", "HealthDevicesFragment");
            CreateNotificationChannel(CHANNEL_NAME_BLOODPRESSURE, TITLE_BLOODPRESSURE,
                CONTENT_BLOODPRESSURE);
            BuildNotification(context, Constants.BloodPressureNotifId, CHANNEL_NAME_BLOODPRESSURE,
                TITLE_BLOODPRESSURE, CONTENT_BLOODPRESSURE, intentBloodPressure);


        }

        private static void CreateNotificationChannel(string mChannel, string mTitle, string mContent)
        {
            var description = mContent;

            var channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.Default)
                {
                    Description = description
                };

            var notificationManager =
                (NotificationManager) Application.Context.GetSystemService(
                    Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);

        }

        private static void BuildNotification(Context context, int notifyId, string channel, string title, string content, Intent intent)
        {

//            NotifyId += 1;
            var piNotification = PendingIntent.GetActivity(context, notifyId, intent,
                PendingIntentFlags.UpdateCurrent);
            var mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentTitle(title)
                    .SetContentText(content)
                    .SetAutoCancel(true)
                    .SetContentIntent(piNotification)
                    .SetPriority(NotificationCompat.PriorityHigh);

            var notificationManager = NotificationManagerCompat.From(context);

            notificationManager.Notify(NotifyId, mBuilder.Build());
        }

    }
}