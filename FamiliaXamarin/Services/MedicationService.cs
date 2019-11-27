using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Org.Json;
using SQLite;

namespace FamiliaXamarin.Services
{
    [Service]
    class MedicationService : Service
    {
        private SQLiteAsyncConnection _db;
        private const int ServiceRunningNotificationId = 10000;
        Timer aTimer = new Timer(1000);
        private int interval = 1000, counter = 0;


        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("Service", "Stopped");
            aTimer.Stop();
        }
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            await Task.Run(async () =>
            {
                if (await SendData(this))
                {
                    Log.Error("Medication Service", "S-a conectat la server");
                    interval = 6000;
                    counter = 0;
//                    StopSelf();
                }
                else
                {
                    Log.Error("Medication Service", "Nu am ce trimite la server");
                    counter++;
                    if (counter == 5)
                    {
                        interval = 10000;
                        counter = 0;
                    }

                }
            });
           

            aTimer.Interval = interval;

           
        }
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "STARTED");

            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
           
        }


        async Task<bool> SendData(Context context)
        {
            aTimer.Stop();
            await _db.CreateTableAsync<MedicineRecords>();
            var myList = await EvaluateQuery(_db, "Select * FROM MedicineRecords");

            JSONArray jsonList = new JSONArray();
            if (myList.Count() == 0) return false;
            foreach (var el in myList)
            {
                JSONObject element = new JSONObject().Put("uuid", el.Uuid).Put("date", el.DateTime);
                jsonList.Put(element);
            }

            if (Utils.CheckNetworkAvailability())
            {
                string result = await WebServices.Post($"{Constants.PublicServerAddress}/api/medicine", jsonList, Utils.GetDefaults("Token"));
                Log.Error("Medication Service", result);
                switch (result)
                {
                    case "Done":
                    case "done":
                        aTimer.Start();
                        await _db.DropTableAsync<MedicineRecords>();
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
        }

      

        private async Task<IEnumerable<MedicineRecords>> EvaluateQuery(SQLiteAsyncConnection db, string query)
        {
            return await db.QueryAsync<MedicineRecords>(query);
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("Medication Service", "Started");

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();

            try
            {
                string CHANNEL_ID = "my_channel_01";
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Medication",
                    NotificationImportance.Default);

                ((NotificationManager)GetSystemService(NotificationService))
                    .CreateNotificationChannel(channel);
                Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                    .SetContentTitle("Familia")
                    .SetContentText("Ruleaza in fundal")
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetOngoing(true)
                    .Build();

                StartForeground(ServiceRunningNotificationId, notification);
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