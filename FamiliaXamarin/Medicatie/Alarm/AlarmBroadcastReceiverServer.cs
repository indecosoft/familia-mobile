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
using FamiliaXamarin.Helpers;
using Java.Util;
using Org.Json;
using Random = Java.Util.Random;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiverServer : BroadcastReceiver
    {
        public static readonly string Uuid = "uuid";
        public static readonly string Title = "title";
        public static readonly string Content = "content";
        static readonly string Ok = "OK";
        public static readonly string ActionOk = "actionOk";
        public static readonly string ActionReceive = "actionReceive";
        public static int NotifyId = Constants.NotifId;

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            Log.Error("ACTIONSARTONRECEIVE", ""+ action);
            if (action != null)
            {
                Log.Error("ACTION", action);
                if (ActionReceive.Equals(action))
                {
                    Toast.MakeText(context, "ALARM SERVER!!!", ToastLength.Long).Show();
                    Log.Error("VINE ALARMA DIN SERVICE", "DADADAD");
                    var uuid = intent.GetStringExtra(Uuid);
                    var title = intent.GetStringExtra(Title);
                    var content = intent.GetStringExtra(Content);
                    var channel = uuid;

                    createNotificationChannel(channel, title, content);

                    NotifyId += 1;

                    Intent okIntent = new Intent(context, typeof(AlarmBroadcastReceiverServer));
                    okIntent.PutExtra(Uuid, uuid);
                    okIntent.PutExtra("notifyId", NotifyId);
                    okIntent.SetAction(ActionOk);


                    PendingIntent piNotification =
                        PendingIntent.GetBroadcast(context, NotifyId, okIntent, PendingIntentFlags.UpdateCurrent);



                    NotificationCompat.Builder mBuilder =
                        new NotificationCompat.Builder(context, channel)
                            .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                            .SetWhen(DateTime.Now.Millisecond)
                            .SetContentTitle(title)
                            .SetContentText(content)
                            .SetAutoCancel(true)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .AddAction(Resource.Drawable.account, Ok, piNotification)
                            .SetOngoing(true);

                    NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
                    
                    notificationManager.Notify(NotifyId, mBuilder.Build());

                }

                else 
                if (ActionOk.Equals(action))
                {
                    Toast.MakeText(context, "Action ok!!!", ToastLength.Long).Show();
                    DateTime now = DateTime.Now;
                    string uuid = intent.GetStringExtra(Uuid);
                    //TODO verifica daca poate trimite la server, daca nu, salveaza intr-un fisier si trimite datele din fisier cand se poate

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

        async void SendData(string uuid, DateTime date, Context context)
        {
            //using (var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("uuid", uuid), new KeyValuePair<string, string>("date", date.ToString("yyyy-MM-dd HH:mm:ss")) })))
            JSONObject mObject = new JSONObject().Put("uuid", uuid).Put("date", date.ToString("yyyy-MM-dd HH:mm:ss"));
            var res = await WebServices.Post($"{Constants.PublicServerAddress}/api/medicine", mObject, Utils.GetDefaults("Token", context));

            Log.Error("#################", ""+res);
        }

        void createNotificationChannel(string mChannel, string mTitle, string mContent)
        {
                string name = mTitle;
                string description = mContent;
                //var importance = NotificationManager.ImportanceDefault;

                NotificationChannel channel =
                    new NotificationChannel(mChannel, mTitle, NotificationImportance.Default)
                    {
                        Description = description
                    };

                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

        }
    }
}