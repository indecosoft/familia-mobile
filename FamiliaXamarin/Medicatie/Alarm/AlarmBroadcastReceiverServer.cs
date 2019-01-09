using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Services;
using Java.IO;
using Java.Util;
using Org.Json;
using SQLite;
using Random = Java.Util.Random;
using TaskStackBuilder = Android.App.TaskStackBuilder;

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
        private SQLiteAsyncConnection _db;
        Intent _medicationServiceIntent;
        public async override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (action != null)
            {
                Log.Error("ACTION", action);
                if (ActionReceive.Equals(action))
                {
                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var numeDB = "devices_data.db";
                    _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
                    await _db.CreateTableAsync<MedicineRecords>();


                    var uuid = intent.GetStringExtra(Uuid);
                    var title = intent.GetStringExtra(Title);
                    var content = intent.GetStringExtra(Content);
                   // var channel = uuid;
                    var channel = "channelabsolut";

                    createNotificationChannel(channel, title, content);

                    NotifyId += 1;

                    Intent okIntent = new Intent(context, typeof(AlarmBroadcastReceiverServer));
                    okIntent.PutExtra(Uuid, uuid);
                    okIntent.PutExtra("notifyId", NotifyId);
                    okIntent.SetAction(ActionOk);

                   // PendingIntent piNotification = PendingIntent.GetBroadcast(context, NotifyId, okIntent, PendingIntentFlags.UpdateCurrent);
                    PendingIntent piNotification = PendingIntent.GetBroadcast(context, 2019, okIntent, PendingIntentFlags.OneShot);

                    NotificationCompat.Builder mBuilder =
                        new NotificationCompat.Builder(context, channel)
                            .SetSmallIcon(Resource.Drawable.logo)
                            .SetContentTitle(title)
                            .SetContentText(content)
                            .SetAutoCancel(true)
                            .SetPriority(NotificationCompat.PriorityHigh)
                            .AddAction(Resource.Drawable.account, Ok, piNotification)
                            .SetOngoing(true);

                    NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);

                    notificationManager.Notify(NotifyId, mBuilder.Build());

                } else if (ActionOk.Equals(action)) {
                    DateTime now = DateTime.Now;
                    string uuid = intent.GetStringExtra(Uuid);
                    JSONArray mArray = new JSONArray().Put(new JSONObject().Put("uuid", uuid).Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var numeDB = "devices_data.db";
                    _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
                    await _db.CreateTableAsync<MedicineRecords>();
                    NotificationManagerCompat.From(context).Cancel(intent.GetIntExtra("notifyId", 0));
                    await Task.Run(async () =>
                    {
                        if (await SendData(context, mArray))
                        {
                            bool running = IsServiceRunning(typeof(MedicationService), context);
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
                            _medicationServiceIntent = new Intent(context, typeof(MedicationService));
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
        }

        public bool IsServiceRunning(System.Type ClassTypeof, Context context)
        {
            ActivityManager manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ShortClassName == ClassTypeof.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        private async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {

            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        async Task<bool> SendData(Context context, JSONArray mArray)
        {
            if (Utils.CheckNetworkAvailability())
            {
                string result = await WebServices.Post($"{Constants.PublicServerAddress}/api/medicine", mArray, Utils.GetDefaults("Token", context));
                switch (result)
                {
                    case "Done":
                        return true;
                    case null:
                    case "Wrong data!":
                    default:
                        return false;
                    
                }

            }
            else return false;
        }

        private async Task<IEnumerable<MedicineRecords>> EvaluateQuery(SQLiteAsyncConnection db, string query)
        {
            return await db.QueryAsync<MedicineRecords>(query);
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
    }
}