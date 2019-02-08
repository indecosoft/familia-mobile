using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.DataModels;
using Org.Json;
using SQLite;

namespace FamiliaXamarin.Helpers
{

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionPowerConnected })]
    public class ChargerReceiver : BroadcastReceiver
    {

        private SQLiteAsyncConnection _db;
        public override async void OnReceive(Context context, Intent intent)
        {
           // Toast.MakeText(context, "IT IS WORKING", ToastLength.Long).Show();
            var action = intent.Action;
            if (action == null || !action.Equals(Intent.ActionPowerConnected)) return;
            var path =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
            await _db.CreateTableAsync<DeviceConfigRecords>();


            await Task.Run(async () =>
            {
                var data = await GetData();
                var obj = new JSONObject(data);
                var bloodPressure = obj.GetJSONObject("bloodPressureSystolic");
                var intervalBloodPressure = bloodPressure.GetString("interval");
                var bloodGlucose = obj.GetJSONObject("bloodGlucose");
                var intervalGlucose = bloodGlucose.GetString("interval");
                Log.Error("INTERVAL_BLOOD_PRESSURE", intervalBloodPressure);
                Log.Error("INTERVAL_GLUCOSE", intervalGlucose);

                AddDeviceConfig(_db, intervalBloodPressure, intervalGlucose);

                LaunchAlarm(context, intervalGlucose, Constants.IntervalGlucose);
                LaunchAlarm(context, intervalBloodPressure, Constants.IntervalBloodPressure);
            });
        }

        private static void LaunchAlarm(Context context, string interval, string content)
        {
            //=int.Parse(interval) * 60000
            if (int.TryParse(interval,  out var intervalMilisec))
                intervalMilisec *= 60000;
            else
                intervalMilisec = 60000;
            var am = (AlarmManager) context.GetSystemService(Context.AlarmService);
            var i = new Intent(context, typeof(AlarmDeviceReceiver));
            i.PutExtra(AlarmDeviceReceiver.INTERVAL_CONTENT, content);
            i.PutExtra("IntervalMilis", interval);
            var random = new Random();
            var id = (DateTime.UtcNow).Millisecond * random.Next();
            var pi = PendingIntent.GetBroadcast(context, id, i, PendingIntentFlags.OneShot);

            am?.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup,
                SystemClock.ElapsedRealtime() + intervalMilisec, intervalMilisec , pi);
          //  Toast.MakeText(context, intervalMilisec, ToastLength.Short);


        }


        private static async Task<string> GetData()
        {
            if (!Utils.CheckNetworkAvailability()) return null;
            var result =
                await WebServices.Get(
                    "https://gis.indecosoft.net/devices/get-device-config/864190030936193");
//               var result = await WebServices.Get(
//                   $"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetImei(Application.Context)}",
//                   Utils.GetDefaults("Token", context));
            if (result == null) return null;
            Log.Error("RESULT_FROM_GIS", result);
            return result;
        }


        private static async void AddDeviceConfig(SQLiteAsyncConnection db, string intervalBlood, string intervalGlucose)
        {

            await db.InsertAsync(new DeviceConfigRecords()
            {
                IntervalBloodPresure = intervalBlood,
                 IntervalGlucose = intervalGlucose

            });
        }

    }
}