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
        public async override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            Log.Error("ACTIONSARTONRECEIVE", "" + action);
            if (action != null)
            {
                Log.Error("ACTION", action);
                if (ActionReceive.Equals(action))
                {
                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var numeDB = "devices_data.db";
                    _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
                    await _db.CreateTableAsync<MedicineRecords>();


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
                    JSONArray mArray = new JSONArray().Put(new JSONObject().Put("uuid", uuid).Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var numeDB = "devices_data.db";
                    _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
                    await _db.CreateTableAsync<MedicineRecords>();
                    if (!await SendData(mArray, context))
                    {
                        AddMedicine(_db, uuid, now);
                    }
                    else
                    {
                        var myList = await EvaluateQuery(_db, "Select * FROM MedicineRecords");

                        JSONArray jsonList = new JSONArray();
                        foreach (var el in myList)
                        {
                            JSONObject element = new JSONObject().Put("uuid", el.Uuid).Put("date", el.DateTime);
                            jsonList.Put(element);
                        }

                        if (Utils.CheckNetworkAvailability())
                        {
                            string result = await WebServices.Post($"{Constants.PublicServerAddress}/api/medicine", jsonList, Utils.GetDefaults("Token", context));
                            //var table = await EvaluateQuery(_db, "DROP TABLE MedicineRecords");
                            await _db.DropTableAsync<MedicineRecords>();
                        }

                    }



                    NotificationManagerCompat.From(context).Cancel(intent.GetIntExtra("notifyId", 0));


                }
            }
        }

        private async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {

            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        private async Task<IEnumerable<MedicineRecords>> EvaluateQuery(SQLiteAsyncConnection db, string query)
        {
            return await db.QueryAsync<MedicineRecords>(query);
        }
        public int CurrentTimeMillis()
        {
            return (int)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        async Task<bool> SendData(JSONArray mArray, Context context)
        {
            //using (var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("uuid", uuid), new KeyValuePair<string, string>("date", date.ToString("yyyy-MM-dd HH:mm:ss")) })))

            var res = await WebServices.Post($"{Constants.PublicServerAddress}/api/medicine", mArray, Utils.GetDefaults("Token", context));

            Log.Error("#################", "" + res);
            if (res != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void createNotificationChannel(string mChannel, string mTitle, string mContent)
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