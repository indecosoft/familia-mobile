using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using Familia.DataModels;
using Familia.Helpers;
using Java.Util;
using Org.Json;

namespace Familia.Devices.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class ConfigReceiver : BroadcastReceiver
    {
        public const int IdPendingIntent = 88888;
        const string Log_Tag = "ConfigReceiver Tag";
        string intervalBloodPressure;
        string intervalGlucose;

        public override async void OnReceive(Context context, Intent intent)
        {

            Log.Error(Log_Tag, "received ");
            try {
                var date = getDate();
                var calendar = Calendar.Instance;
                calendar.Set(date.Year, date.Month - 1 , date.Day, date.Hour, date.Minute, 0);
                Log.Error(Log_Tag, "Calendar for NEXT GET CONFIG " + calendar.Time.ToString());
                recallConfigReceiver(context, calendar);
            }
            catch (System.Exception e) { Log.Error(Log_Tag, "error recall configreceiver " + e.Message); }
            

           await Task.Run(async () =>
            {
                try
                {
                    CancelAlarms(context);

                    string data = await GetData();
                    if (data != null)
                    {
                        var obj = new JSONObject(data);
                        Log.Error(Log_Tag, obj.ToString());

                        try
                        {
                            intervalBloodPressure = obj.GetJSONObject("bloodPressureSystolic").GetString("interval");
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(Log_Tag, "Err " + e.Message + " / no interval for bloodPressureSystolic");
                            intervalBloodPressure = null;
                        }


                        try
                        {
                            intervalGlucose = obj.GetJSONObject("bloodGlucose").GetString("interval");
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(Log_Tag, "Err " + e.Message + " / no interval for bloodGlucose");
                            intervalGlucose = null;
                        }

                        await UpdateDeviceConfig(intervalBloodPressure, intervalGlucose);
                    }
                    else
                    {

                        var obj = await GetDataFromLocalDb();
                        if (obj != null)
                        {
                            intervalBloodPressure = obj.IntervalBloodPresure;
                            intervalGlucose = obj.IntervalGlucose;
                        }
                    }

                    if (intervalBloodPressure != null)
                    {
                        LaunchAlarm(context, intervalBloodPressure, Constants.IntervalBloodPressure);
                    }

                    if (intervalGlucose != null)
                    {
                        LaunchAlarm(context, intervalGlucose, Constants.IntervalGlucose);
                    }

                }
                catch (System.Exception ex)
                {
                    Log.Error(Log_Tag, "Err " + ex.Message);
                }
            });
        }

        private static void CancelAlarms(Context context)
        {
            var am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var intent = new Intent(Application.Context, typeof(AlarmDeviceReceiver));
            PendingIntent piBloodPressure = PendingIntent.GetBroadcast(Application.Context, Constants.BloodPressureNotifId, intent, 0);
            PendingIntent piGlucose = PendingIntent.GetBroadcast(Application.Context, Constants.GlucoseNotifId, intent, 0);

            am.Cancel(piBloodPressure);
            piBloodPressure.Cancel();
            am.Cancel(piGlucose);
            piGlucose.Cancel();
        }

        private static DateTime getDate()
        {
            DateTime dt = DateTime.Now;
            Log.Error(Log_Tag, "date " + dt.Day + ", " + dt.Month + ", " + dt.Year + ", " + dt.Hour);
            var milisec = dt.Hour * 3600000 + dt.Minute * 60000 + dt.Second * 1000;
            var remainingTime = 24 * 3600000 - milisec;
            var newdt = dt.AddMilliseconds(remainingTime + 8 * 3600 * 1000);
            Log.Error(Log_Tag, "new date " + newdt.Day + ", " + newdt.Month + ", " + newdt.Year + ", " + newdt.Hour + ", " + newdt.Minute + ", " + newdt.Millisecond);
            return newdt;
        }

        private static async Task<string> GetData()
        {
            if (!Utils.CheckNetworkAvailability()) return null;
            string result = await WebServices.WebServices.Get($"{Constants.Config}/{Utils.GetDeviceIdentificator(Application.Context)}");
            if (result == null) return null;
            Log.Error(Log_Tag, "RESULT_FROM_GIS " + result);
            return result;
        }

        private static async Task<bool> UpdateDeviceConfig(string intervalBlood,
            string intervalGlucose)
        {
            var db = await SqlHelper<DeviceConfigRecords>.CreateAsync();
            await db.QueryValuations($"delete from DeviceConfigRecords");
            Log.Error(Log_Tag, "data deleted");

            await db.Insert(new DeviceConfigRecords
            {
                IntervalBloodPresure = intervalBlood,
                IntervalGlucose = intervalGlucose
            });
            Log.Error(Log_Tag, "data inserted");

            var list = await db.QueryValuations("select * from DeviceConfigRecords");
            foreach (var x in list)
            {
                Log.Error(Log_Tag, x.IntervalBloodPresure + ", " + x.IntervalGlucose + ", " + x.Id);
            }
            return true;

        }

        private static async Task<DeviceConfigRecords> GetDataFromLocalDb() {

            var db = await SqlHelper<DeviceConfigRecords>.CreateAsync();
            var list = (List<DeviceConfigRecords>)await db.QueryValuations("select * from DeviceConfigRecords");
            foreach (var x in list)
            {
                Log.Error(Log_Tag, x.IntervalBloodPresure + ", " + x.IntervalGlucose + ", " + x.Id);
            }
           
            return list[0];
        }

        private void recallConfigReceiver(Context context, Calendar calendar)
        {
            var am = (AlarmManager)context.GetSystemService(Context.AlarmService);

            var pi = PendingIntent.GetBroadcast(context,
                IdPendingIntent,
                new Intent(Application.Context, typeof(ConfigReceiver)),
                PendingIntentFlags.UpdateCurrent);

            am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pi);

            Log.Error(Log_Tag, "milisec " + calendar.TimeInMillis);

        }


        private static DateTime getDateAfterMinutes(int minutes)
        {
            DateTime dt = DateTime.Now;
            Log.Error(Log_Tag, "date " + dt.Day + ", " + dt.Month + ", " + dt.Year + ", " + dt.Hour);
            
            var newdt = dt.AddMilliseconds(minutes * 60000); 
            Log.Error(Log_Tag, "new date " + newdt.Day + ", " + newdt.Month + ", " + newdt.Year + ", " + newdt.Hour + ", " + newdt.Minute + ", " + newdt.Millisecond);
            return newdt;
        }

        public static void LaunchAlarm(Context context, string interval, string content)
        {
            var date = getDateAfterMinutes(int.Parse(interval));
            var calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, 0);

            Log.Error(Log_Tag, "Alarm hour:" + date.Hour + " minute: " + date.Minute);
            if (date.Hour > 20) {
                return;
            }

            var am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var i = new Intent(context, typeof(AlarmDeviceReceiver));
            i.PutExtra(AlarmDeviceReceiver.INTERVAL_CONTENT, content);
            i.PutExtra("IntervalMilis", interval);
            PendingIntent pi;

            if (content.Equals("INTERVAL_GLUCOSE"))
            {
                pi = PendingIntent.GetBroadcast(context, Constants.GlucoseNotifId, i, PendingIntentFlags.UpdateCurrent);
            }
            else
            {
                pi = PendingIntent.GetBroadcast(context, Constants.BloodPressureNotifId, i,
                    PendingIntentFlags.UpdateCurrent);
            }

            am.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pi);

        }
    }
}