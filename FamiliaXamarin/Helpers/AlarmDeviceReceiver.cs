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

namespace FamiliaXamarin.Helpers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class AlarmDeviceReceiver : BroadcastReceiver
    {
        public static readonly string INTERVAL_CONTENT = "INTERVAL_CONTENT";
        public static readonly string CHANNEL_NAME_GLUCOSE = "Channel glucose";
        public static readonly string CHANNEL_NAME_BLOODPRESSURE = "Channel bloodpressure";
        public static readonly string TITLE_BLOODPRESSURE = "Puls";
        public static readonly string TITLE_GLUCOSE = "Glucoza";
        public static readonly string CONTENT_GLUCOSE = "Trebuie sa-ti masori glucoza";
        public static readonly string CONTENT_BLOODPRESSURE = "Trebuie sa-ti masori pulsul";

        public static int NotifyId = Constants.NotificationAlarmDevice;

        public override void OnReceive(Context context, Intent intent)
        {
            var content = intent.GetStringExtra(INTERVAL_CONTENT);

            Log.Error("INTERVAL_CONTENT", content);
            Toast.MakeText(context, content, ToastLength.Short).Show();

            if (content.Equals(ChargerReceiver.INTERVAL_GLUCOSE))
            {
                createNotificationChannel(CHANNEL_NAME_GLUCOSE, TITLE_GLUCOSE, CONTENT_GLUCOSE);
                BuildNotification(context, CHANNEL_NAME_GLUCOSE, TITLE_GLUCOSE, CONTENT_GLUCOSE);

            }

            if (content.Equals(ChargerReceiver.INTERVAL_BLOOD_PRESSURE))
            {
                createNotificationChannel(CHANNEL_NAME_BLOODPRESSURE, TITLE_BLOODPRESSURE, CONTENT_BLOODPRESSURE);
                BuildNotification(context, CHANNEL_NAME_BLOODPRESSURE, TITLE_BLOODPRESSURE, CONTENT_BLOODPRESSURE);
            }


        }

        private void createNotificationChannel(string mChannel, string mTitle, string mContent)
        {
            string name = mTitle;
            string description = mContent;

            NotificationChannel channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.Default)
                {
                    Description = description
                };

            var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);

        }

        private void BuildNotification(Context context, string channel, string title, string content)
        {

            NotifyId += 1;

            Intent okIntent = new Intent(context, typeof(MainActivity));


            PendingIntent piNotification = PendingIntent.GetBroadcast(context, NotifyId, okIntent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentTitle(title)
                    .SetContentText(content)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetAutoCancel(true);



            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);

            notificationManager.Notify(NotifyId, mBuilder.Build());
        }

    }
}