using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Services;
using Org.Json;
using SQLite;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiverServer : BroadcastReceiver
    {
        public const string Uuid = "uuid";
        public const string Title = "title";
        public const string Content = "content";
        public const string Postpone = "postpone";

        private const string Ok = "OK";
        private const string ActionOk = "actionOk";
        public const string ActionReceive = "actionReceive";
        private static int NotifyId = Constants.NotifId;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;
        public static readonly string FROM_SERVER = "from_server";
        public static readonly string MEDICATION_NAME = "med_name";


        public override async void OnReceive(Context context, Intent intent)
        {
//            var action = intent.Action;
//            if (action == null) return;
//            var now = DateTime.Now;
            //todo de tratat snooze 
            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            var uuid = intent.GetStringExtra(Uuid);
            var title = intent.GetStringExtra(Title);
            var content = intent.GetStringExtra(Content);
            var postpone = intent.GetIntExtra(Postpone, 5);

            

            const string channel = "channelabsolut";
            Log.Error("RECEIVER",  title + ", "+ content +", " + postpone);

            CreateNotificationChannel(channel, title, content);

            Random random = new Random();
            int randomNumber = random.Next(0, 5000);

            NotifyId += randomNumber;

            var alarmIntent = new Intent(context, typeof(AlarmActivity));
            alarmIntent.AddFlags(ActivityFlags.ClearTop);
            alarmIntent.PutExtra(Uuid, uuid);
            alarmIntent.PutExtra("notifyId", NotifyId);
            alarmIntent.PutExtra("message", FROM_SERVER);
            alarmIntent.PutExtra(MEDICATION_NAME, title);
            alarmIntent.PutExtra(Postpone, postpone);
            alarmIntent.PutExtra(Content, content);
            alarmIntent.SetFlags(ActivityFlags.NewTask);


            BuildNotification(context, NotifyId, channel, title, content, alarmIntent);




//            var notificationManager =
//                NotificationManagerCompat.From(context);


//            if (ActionReceive.Equals(action))
//            {
//                if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;
                

                //de aici pt sqlite
//                var path =
//                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
//                var nameDb = "devices_data.db";
//                _db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
//                await _db.CreateTableAsync<MedicineRecords>();
                //pana aici


//                var uuid = intent.GetStringExtra(Uuid);
//                var title = intent.GetStringExtra(Title);
//                var content = intent.GetStringExtra(Content);
                // var channel = uuid;
//                const string channel = "channelabsolut";

//                CreateNotificationChannel(channel, title, content);

//                NotifyId += 1;

//                var okIntent = new Intent(context, typeof(AlarmBroadcastReceiverServer));
//                var okIntent = new Intent(context, typeof(AlarmActivity));
//
//                okIntent.PutExtra(Uuid, uuid);
//                okIntent.PutExtra("notifyId", NotifyId);
//                okIntent.PutExtra("message", FROM_SERVER);
//                okIntent.PutExtra(MEDICATION_NAME, title);
//                okIntent.SetAction(ActionOk);
                
//                var piNotification = PendingIntent.GetBroadcast(context, DateTime.Now.Millisecond,
//                    okIntent, PendingIntentFlags.OneShot);

//                var mBuilder =
//                    new NotificationCompat.Builder(context, channel)
//                        .SetSmallIcon(Resource.Drawable.logo)
//                        .SetContentTitle(title)
//                        .SetContentText(content)
//                        .SetAutoCancel(true)
//                        .SetPriority(NotificationCompat.PriorityHigh)
//                        .AddAction(Resource.Drawable.account, Ok, piNotification)
//                        .SetOngoing(true);
//
//                notificationManager.Notify(NotifyId, mBuilder.Build());
//            }
//            else
//            {
//                if (!ActionOk.Equals(action)) return;

            //sqlite de aici
//                var uuid = intent.GetStringExtra(Uuid);
//                var mArray = new JSONArray().Put(new JSONObject().Put("uuid", uuid)
//                    .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));
//
//                var path =
//                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
//                const string nameDb = "devices_data.db";
//                _db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
//                await _db.CreateTableAsync<MedicineRecords>();
//                NotificationManagerCompat.From(context)
//                    .Cancel(intent.GetIntExtra("notifyId", 0));
//                await Task.Run(async () =>
//                {
//                    if (await SendData(context, mArray))
//                    {
//                        var running = IsServiceRunning(typeof(MedicationService), context);
//                        if (running)
//                        {
//                            Log.Error("SERVICE", "Medication service is running");
//                            context.StopService(_medicationServiceIntent);
//                        }
//                    }
//                    else
//                    {
//                        AddMedicine(_db, uuid, now);
//                        Log.Error("SERVICE", "Medication service started");
//                        _medicationServiceIntent =
//                            new Intent(context, typeof(MedicationService));
//                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
//                        {
//                            context.StartForegroundService(_medicationServiceIntent);
//                        }
//                        else
//                        {
//                            context.StartService(_medicationServiceIntent);
//                        }
//                    }
//                });

            //pana aici?


//            }
        }

        private static bool IsServiceRunning(Type classTypeof, Context context)
        {
            var manager = (ActivityManager) context.GetSystemService(Context.ActivityService);
#pragma warning disable 618
            return manager.GetRunningServices(int.MaxValue).Any(service =>
                service.Service.ShortClassName == classTypeof.ToString());
        }

        private static async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {
            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        private static async Task<bool> SendData(Context context, JSONArray mArray)
        {
            var result = await WebServices.Post(
                $"{Constants.PublicServerAddress}/api/medicine", mArray,
                Utils.GetDefaults("Token"));
            if (!Utils.CheckNetworkAvailability()) return false;
            switch (result)
            {
                case "Done":
                    return true;
                default:
                    return false;
            }
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

            //Log.Error("PPPAAAAAAAAAAAAAAAAAA", "build notification for " + title + " with id: " + notifyId);

            var piNotification = PendingIntent.GetActivity(context, notifyId, intent, PendingIntentFlags.UpdateCurrent);
            var mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentText(content)
                    .SetContentTitle(title)
                    .SetAutoCancel(false)
                    .SetContentIntent(piNotification)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetOngoing(true);



            var notificationManager = NotificationManagerCompat.From(context);

            notificationManager.Notify(notifyId, mBuilder.Build());
        }

    }
}