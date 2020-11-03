using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Android.Util;
using Familia.DataModels;
using Familia.Helpers;
using Familia.Medicatie.Alarm;
using Org.Json;
using SQLite;
using Environment = System.Environment;

namespace Familia.Services
{
    [Service]
    class MedicationService : Service
    {
        private SQLiteAsyncConnection _db;
        private const int ServiceRunningNotificationId = 20000;
        Timer aTimer = new Timer(2000);
        private int interval = 2000, counter;


        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("Medication Service", "Stopped");
            aTimer.Stop();
        }
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            await Task.Run(async () =>
            {
                if (await SendData(this))
                {
                    Log.Error("Medication Service", "S-a conectat la server");
                    StopSelf();
                }
                else
                {
                    Log.Error("Medication Service", "Nu am ce trimite la server");
                    counter++;
                    if (counter == 5)
                    {
                        interval = 10000;
                        counter = 0;
                        aTimer.Interval = interval;
                    }

                }
            });
           

           

           
        }
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Medication Service:", "STARTED");

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
           
        }


        async Task<bool> SendData(Context context)
        {
            try {

                aTimer.Stop();
                await _db.CreateTableAsync<MedicineRecords>();
                var myList = await EvaluateQuery(_db, "Select * FROM MedicineRecords");
                Log.Error("Medication Service:", " count from myList" + myList.Count());
                JSONArray jsonList = new JSONArray();
                if (myList.Count() == 0) return false;
                foreach (var el in myList)
                {
                    JSONObject element = new JSONObject().Put("uuid", el.Uuid).Put("date", el.DateTime);
                    jsonList.Put(element);
                }
                Log.Error("Medication Service:", "sending object.. " + jsonList.ToString());
                if (Utils.CheckNetworkAvailability())
                {
                    Log.Error("Medication Service:", "network checked");
                    string result = await WebServices.WebServices.Post("/api/medicine", jsonList, Utils.GetDefaults("Token"));
                    Log.Error("Medication Service", result);
                    switch (result)
                    {
                        case "Done":
                        case "done":
                            Log.Error("Medication Service:", "response done. timer start");
                            //aTimer.Start();

                            // await _db.DropTableAsync<MedicineRecords>();
                            await _db.DeleteAllAsync<MedicineRecords>();
                            Log.Error("Medication Service:", "table deleted");
                            return true;
                        default:
                            aTimer.Start();
                            return false;

                    }
                }
                else
                {
                    aTimer.Start();
                    return false;
                }

            } catch (Exception e) {
                Log.Error("Medication Service ", e.Message);
                //Toast.MakeText(context, "Sqlite busy", ToastLength.Long).Show();
                return false;
            }
        
        }

      

        private async Task<IEnumerable<MedicineRecords>> EvaluateQuery(SQLiteAsyncConnection db, string query)
        {
            return await db.QueryAsync<MedicineRecords>(query);
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Medication Service", "Started");

            aTimer.Elapsed += OnTimedEvent;
            aTimer.Start();

            try
            {
              /*  string CHANNEL_ID = "my_channel_02";
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Medication",
                    NotificationImportance.Default);

                ((NotificationManager)GetSystemService(NotificationService))
                    .CreateNotificationChannel(channel);*/

                Notification notification = new NotificationCompat.Builder(this, App.SimpleChannelIdForServices)
                    .SetContentTitle("Familia")
                    .SetContentText("Se trimit date..")
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetOngoing(true)
                    .Build();

                StartForeground(App.SimpleNotificationIdForServices, notification);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return StartCommandResult.Sticky;   
        }
    }
}