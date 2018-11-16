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
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.DataModels;
using Org.Json;
using SQLite;

namespace FamiliaXamarin.Helpers
{

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Android.Content.Intent.ActionCloseSystemDialogs })]
    public class ChargerReceiver : BroadcastReceiver
    {
        public static readonly string INTERVAL_GLUCOSE = "INTERVAL_GLUCOSE";
        public static readonly string INTERVAL_BLOOD_PRESSURE = "INTERVAL_BLOOD_PRESSURE";
        private SQLiteAsyncConnection _db;
        public async  override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            Toast.MakeText(context, "MERGE CHARGER RECEIVER", ToastLength.Long).Show();
            if (action != null && action.Equals(Intent.ActionCloseSystemDialogs))
            {
                var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var numeDB = "devices_data.db";
                _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
                await _db.CreateTableAsync<DeviceConfigRecords>();


                await Task.Run(async () =>
                {
                    var data = await GetData(context);
                    var obj = new JSONObject(data);
                    var bloodPressure = obj.GetJSONObject("bloodPressureSystolic");
                    var intervalBloodPressure = bloodPressure.GetString("interval");
                    var bloodGlucose = obj.GetJSONObject("bloodGlucose");
                    var intervalGlucose = bloodGlucose.GetString("interval");
                    Log.Error("INTERVAL_BLOOD_PRESSURE", intervalBloodPressure);
                    Log.Error("INTERVAL_GLUCOSE", intervalGlucose);

                    AddDeviceConfig(_db, intervalBloodPressure, intervalGlucose);

                    LaunchAlarm(context, intervalGlucose, INTERVAL_GLUCOSE);
                    LaunchAlarm(context, intervalBloodPressure, INTERVAL_BLOOD_PRESSURE);
                });

                Toast.MakeText(context, "S-a facut GET", ToastLength.Long).Show();

            }
        }

        private void LaunchAlarm(Context context, string interval, string content)
        {
            //=int.Parse(interval) * 60000
            int intervalMilisec;
            bool a = int.TryParse(interval, out intervalMilisec);
            if (a)
                intervalMilisec *= 60000;
            else
                intervalMilisec = 60000;
            var am = (AlarmManager) context.GetSystemService(Context.AlarmService);
            var i = new Intent(context, typeof(AlarmDeviceReceiver));
            i.PutExtra(AlarmDeviceReceiver.INTERVAL_CONTENT, content);
            var random = new Random();
            var id = (DateTime.UtcNow).Millisecond * random.Next();
            var pi = PendingIntent.GetBroadcast(context, id, i, PendingIntentFlags.OneShot);

            if (am != null)
            {
                am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + intervalMilisec, intervalMilisec, pi);
            }

           
        }


        private async Task<string> GetData(Context context)
        {
            if (Utils.CheckNetworkAvailability())
            {
               var result = await WebServices.Get("https://gis.indecosoft.net/devices/get-device-config/864190030936193");
                //var result = await WebServices.Get($"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetImei(context)}", Utils.GetDefaults("Token", context));
                if (result != null)
                {
                    Log.Error("RESULT_FROM_GIS", result);
                    return result;
                }

                return null;

            }
            else return null;
        }


        private async void AddDeviceConfig(SQLiteAsyncConnection db, string intervalBlood, string intervalGlucose)
        {

            await db.InsertAsync(new DeviceConfigRecords()
            {
                IntervalBloodPresure = intervalBlood,
                 IntervalGlucose = intervalGlucose

            });
        }

    }
}