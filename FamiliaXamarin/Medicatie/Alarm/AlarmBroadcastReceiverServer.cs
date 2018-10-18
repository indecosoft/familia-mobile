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
using Java.Util;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiverServer : BroadcastReceiver
    {
        public static readonly String UUID = "uuid";
        public static readonly String TITLE = "title";
        public static readonly String CONTENT = "content";
        private static readonly String OK = "OK";
        public static readonly String ACTION_OK = "actionOk";
        public static readonly String ACTION_RECEIVE = "actionReceive";


        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            //TODO de ce nu intra pe else  
            if (ACTION_RECEIVE.Equals(action))
            {
                Toast.MakeText(context, "ALARM SERVER!!!", ToastLength.Long).Show();
                Log.Error("VINE ALARMA DIN SERVICE", "DADADAD");
                var uuid = intent.GetStringExtra(UUID);
                var title = intent.GetStringExtra(TITLE);
                var content = intent.GetStringExtra(CONTENT);
                var Channel = uuid;

                createNotificationChannel(Channel, title, content);

                Intent okIntent = new Intent();
                okIntent.PutExtra(UUID, uuid);
                okIntent.SetAction(ACTION_OK);

                PendingIntent piNotification = PendingIntent.GetBroadcast(context, 12345, okIntent, PendingIntentFlags.OneShot);


                NotificationCompat.Builder mBuilder =
                    new NotificationCompat.Builder(context, Channel)
                        .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                        .SetWhen(DateTime.Now.Millisecond)
                        .SetContentTitle(title)
                        .SetContentText(content)
                        .SetAutoCancel(true)
                        .SetPriority(NotificationCompat.PriorityDefault)
                        .AddAction(Resource.Drawable.account, OK,
                            piNotification);

                NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);

                notificationManager.Notify(Constants.NotifIdServer + 1, mBuilder.Build());

            }
            else if (ACTION_OK.Equals(action))
            {

                long timestamp = DateTime.Now.Millisecond;
                Date date = new Date(timestamp);
                String uuid = intent.GetStringExtra(UUID);

                //TODO post to server
                SendData(uuid, date, context);
            }
        }

        private async void SendData(string uuid, Date date, Context context)
        {
            var res = await Tasks.Tasks.PostMedicine($"{Constants.PublicServerAddress}/api/medicine", uuid, date, Utils.GetDefaults("Token", context));

            Log.Error("#################", res);
        }

        private void createNotificationChannel(string mChannel, string mTitle, string mContent)
        {
            // Create the NotificationChannel, but only on API 26+ because
            // the NotificationChannel class is new and not in the support library
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = mTitle;
                string description = mContent;
                var importance = NotificationManager.ImportanceDefault;
                NotificationChannel channel = new NotificationChannel(mChannel, mTitle, importance);
                channel.Description = description;
                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

            }
        }
    }
}