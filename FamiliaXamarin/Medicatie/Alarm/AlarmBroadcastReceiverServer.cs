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
        private const string Ok = "OK";
        private const string ActionOk = "actionOk";
        public const string ActionReceive = "actionReceive";
        private static int NotifyId = Constants.NotifId;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;

        public override async void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            if (action == null) return;
            Log.Error("ACTION", action);
            var now = DateTime.Now;
            var notificationManager =
                NotificationManagerCompat.From(context);
            if (ActionReceive.Equals(action))
            {
                if (string.IsNullOrEmpty(Utils.GetDefaults("Token", context))) return;
                
                var path =
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var nameDb = "devices_data.db";
                _db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
                await _db.CreateTableAsync<MedicineRecords>();


                var uuid = intent.GetStringExtra(Uuid);
                var title = intent.GetStringExtra(Title);
                var content = intent.GetStringExtra(Content);
                // var channel = uuid;
                const string channel = "channelabsolut";

                CreateNotificationChannel(channel, title, content);

                NotifyId += 1;

                var okIntent = new Intent(context, typeof(AlarmBroadcastReceiverServer));
                okIntent.PutExtra(Uuid, uuid);
                okIntent.PutExtra("notifyId", NotifyId);
                okIntent.SetAction(ActionOk);

                var piNotification = PendingIntent.GetBroadcast(context, DateTime.Now.Millisecond,
                    okIntent, PendingIntentFlags.OneShot);

                var mBuilder =
                    new NotificationCompat.Builder(context, channel)
                        .SetSmallIcon(Resource.Drawable.logo)
                        .SetContentTitle(title)
                        .SetContentText(content)
                        .SetAutoCancel(true)
                        .SetPriority(NotificationCompat.PriorityHigh)
                        .AddAction(Resource.Drawable.account, Ok, piNotification)
                        .SetOngoing(true);

                notificationManager.Notify(NotifyId, mBuilder.Build());
            }
            else
            {
                if (!ActionOk.Equals(action)) return;
                var uuid = intent.GetStringExtra(Uuid);
                var mArray = new JSONArray().Put(new JSONObject().Put("uuid", uuid)
                    .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

                var path =
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                const string nameDb = "devices_data.db";
                _db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
                await _db.CreateTableAsync<MedicineRecords>();
                NotificationManagerCompat.From(context)
                    .Cancel(intent.GetIntExtra("notifyId", 0));
                await Task.Run(async () =>
                {
                    if (await SendData(context, mArray))
                    {
                        var running = IsServiceRunning(typeof(MedicationService), context);
                        if (running)
                        {
                            Log.Error("SERVICE", "Medication service is running");
                            context.StopService(_medicationServiceIntent);
                        }
                    }
                    else
                    {
                        AddMedicine(_db, uuid, now);
                        Log.Error("SERVICE", "Medication service started");
                        _medicationServiceIntent =
                            new Intent(context, typeof(MedicationService));
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            context.StartForegroundService(_medicationServiceIntent);
                        }
                        else
                        {
                            context.StartService(_medicationServiceIntent);
                        }
                    }
                });
            }
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
                Utils.GetDefaults("Token", context));
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
    }
}