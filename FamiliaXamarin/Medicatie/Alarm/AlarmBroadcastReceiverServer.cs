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
using Random = Java.Util.Random;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiverServer : BroadcastReceiver
    {
        public static readonly String UUID = "uuid";
        public static readonly String TITLE = "title";
        public static readonly String CONTENT = "content";
        static readonly String OK = "OK";
        public static readonly String ACTION_OK = "actionOk";
        public static readonly String ACTION_RECEIVE = "actionReceive";
        public static int notifyId = Constants.NotifId;

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Error("ACTIONSARTONRECEIVE", ""+ action);
            if (action != null)
            {
                Log.Error("ACTION", action);
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

                    notifyId += 1;

                    Intent okIntent = new Intent(context, typeof(AlarmBroadcastReceiverServer));
                    okIntent.PutExtra(UUID, uuid);
                    okIntent.PutExtra("notifyId", notifyId);
                    okIntent.SetAction(ACTION_OK);


                    PendingIntent piNotification =
                        PendingIntent.GetBroadcast(context, notifyId, okIntent, PendingIntentFlags.UpdateCurrent);



                    NotificationCompat.Builder mBuilder =
                        new NotificationCompat.Builder(context, Channel)
                            .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                            .SetWhen(DateTime.Now.Millisecond)
                            .SetContentTitle(title)
                            .SetContentText(content)
                            .SetAutoCancel(true)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .AddAction(Resource.Drawable.account, OK, piNotification)
                            .SetOngoing(true);

                    NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
                    
                    notificationManager.Notify(notifyId, mBuilder.Build());

                }

                else 
                if (ACTION_OK.Equals(action))
                {
                    Toast.MakeText(context, "Action ok!!!", ToastLength.Long).Show();
//                    long timestamp = CurrentTimeMillis();
//                    Date date = new Date(timestamp);
                    DateTime now = DateTime.Now;
                    String uuid = intent.GetStringExtra(UUID);

                    //TODO post to server
                    SendData(uuid, now, context);
                    NotificationManagerCompat.From(context).Cancel(intent.GetIntExtra("notifyId", 0));

                    
                }
            }
        }
        public int CurrentTimeMillis()
        {
            return (int)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void SendData(string uuid, DateTime date, Context context)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var res = await Tasks.Tasks.PostMedicine($"{Constants.PublicServerAddress}/api/medicine", uuid, date, Utils.GetDefaults("Token", context));

            Log.Error("#################", res);
        }

        void createNotificationChannel(string mChannel, string mTitle, string mContent)
        {
            // Create the NotificationChannel, but only on API 26+ because
            // the NotificationChannel class is new and not in the support library
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = mTitle;
                string description = mContent;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable XA0001 // Find issues with Android API usage
                var importance = NotificationManager.ImportanceDefault;
#pragma warning restore XA0001 // Find issues with Android API usage
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable IDE0017 // Simplify object initialization
                NotificationChannel channel = new NotificationChannel(mChannel, mTitle, importance);
#pragma warning restore IDE0017 // Simplify object initialization
                channel.Description = description;
                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
#pragma warning disable XA0001 // Find issues with Android API usage
                notificationManager.CreateNotificationChannel(channel);
#pragma warning restore XA0001 // Find issues with Android API usage

            }
        }
    }
}