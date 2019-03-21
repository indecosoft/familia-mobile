using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;
using Environment = System.Environment;
using Exception = System.Exception;
using Object = System.Object;

namespace Familia.Services
{
    [Service]
    internal class SmartBandService : Service
    {
        private const int ServiceRunningNotificationId = 10000;
        private string _token;
        private readonly Handler _handler = new Handler();
        private int _refreshTime = 1000;
        private bool _started;
        private readonly Runnable _runnable;




        private void HandlerRunnable()
        {
            RefreshToken();
            SentData();
            if (_started)
            {
                _handler.PostDelayed(_runnable, _refreshTime * 5);
            }
        }

        public SmartBandService()
        {
            _runnable = new Runnable(HandlerRunnable);
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "SmartBandService STARTED");
            try
            {
                if (Build.VERSION.SdkInt <BuildVersionCodes.O) return;
                const string channelId = "my_channel_01";
                var channel = new NotificationChannel(channelId, "Channel human readable title",
                    NotificationImportance.Default);

                ((NotificationManager) GetSystemService(Context.NotificationService))
                    .CreateNotificationChannel(channel);

                var notification = new NotificationCompat.Builder(this, channelId)
                    .SetContentTitle("")
                    .SetContentText("").Build();

                StartForeground(1, notification);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags,
            int startId)
        {

            StartCommands();
            return StartCommandResult.Sticky;
        }
        private async void StartCommands()
        {
            Log.Error("SmartBand Service", "Started");

            if (string.IsNullOrEmpty(Utils.GetDefaults(GetString(Resource.String.smartband_device))))
            {
                StopSelf();
            }
            else
            {
                await RefreshToken();
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device));
                _started = true;
                SentData();
                //_handler.PostDelayed(_runnable, _refreshTime * 10);
            }
        }
        private async Task RefreshToken()
        {
            var refreshToken = Utils.GetDefaults("FitbitRefreshToken");
            await Task.Run(async () =>
            {
                try
                {
                    var storedToken = Utils.GetDefaults(GetString(Resource.String.smartband_device));
                    if (!string.IsNullOrEmpty(storedToken))
                    {

                        var dict = new Dictionary<string, string>
                        {
                            {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                        };
                        var response = await WebServices.Post("https://api.fitbit.com/oauth2/token",
                            dict);
                        if (response != null)
                        {
                            var obj = new JSONObject(response);
                            var token = obj.GetString("access_token");
                            var newRefreshToken = obj.GetString("refresh_token");
                            var userId = obj.GetString("user_id");
                            Utils.SetDefaults(GetString(Resource.String.smartband_device), 
                                token);
                            Utils.SetDefaults("FitbitToken", token);
                            Utils.SetDefaults("FitbitRefreshToken", newRefreshToken);
                            Utils.SetDefaults("FitbitUserId", userId);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("FitbitServiceError", e.Message);
                    StopSelf();
                }

                //SentData();
                //var a  = await GetSleepData();
            });
        }


        private async Task<IEnumerable<DevicesRecords>> QueryValuations(SQLiteAsyncConnection db,
            string query)
        {
            return await db.QueryAsync<DevicesRecords>(query);
        }
        private async Task<Dictionary<string, int>> GetSteps()
        {
            try
            {
                var data =
                    await WebServices.Get(
                        "https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
                if (!string.IsNullOrEmpty(data))
                {
                    Log.Error("Steps Result", data);

                    try
                    {
                        var fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary")
                            .GetInt("fairlyActiveMinutes");
                        var veryActiveMinutes = new JSONObject(data).GetJSONObject("summary")
                            .GetInt("veryActiveMinutes");
                        var activeMinutes = fairlyActiveMinutes + veryActiveMinutes;

                        var steps = new JSONObject(data).GetJSONObject("summary").GetInt("steps");

                        return new Dictionary<string, int>
                        {
                            {"Steps", steps},
                            {"Activity", activeMinutes},
                        };
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private async Task GetSleepData()
        {
            var data =
                await WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json",
                    _token);

            Log.Error("Smartband Token", _token);
            Log.Error("Sleep Result", data);
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                var jsonObject = new JSONObject(data);
 
                jsonObject.Put("imei", Utils.GetImei(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana"));

                await Task.Run(async () =>
                {
                    var response =
                        await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/sleep", jsonObject, Utils.GetDefaults("Token"));
                });

            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
                
            }
        }

        private async Task GetHeartRatePulse()
        {
            var data = await WebServices.Get(
                "https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json", _token);
            if (string.IsNullOrEmpty(data)) return;
            Log.Error("Heartrate Result", data);
            try
            {
                var bpm = ((JSONObject) new JSONObject(data).GetJSONArray("activities-heart")
                    .Get(0)).GetJSONObject("value").GetInt("restingHeartRate");
                var date = ((JSONObject)new JSONObject(data).GetJSONArray("activities-heart")
                    .Get(0)).GetString("dateTime");
                var jsonObject = new JSONObject();
                jsonObject.Put("imei", Utils.GetImei(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana"));
                jsonObject.Put("dateTime", date).Put("pulse", bpm);
                await Task.Run(async () =>
                {
                    var response =
                        await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/heartrate", jsonObject, Utils.GetDefaults("Token"));
                });

            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
                StopSelf();
            }
        }

        private async void SentData()
        {
            var sqlHelper = await SqlHelper<DevicesRecords>.CreateAsync();

            var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
            await GetSleepData();
            await GetHeartRatePulse();
            //var activity = await GetSteps();

           
            //if (!Utils.CheckNetworkAvailability())

            //    await sqlHelper.Insert(new DevicesRecords()
            //    {
            //        Imei = Utils.GetImei(this),
            //        DateTime = ft.Format(new Date()),
            //        SecondsOfSleep = sleep,
            //        MinutesOfActivity = activity["Activity"].ToString(),
            //        StepCounter = activity["Steps"]
            //    });
            //else
            //{
            //    JSONObject jsonObject;
            //    var jsonArray = new JSONArray();
            //    var list = await sqlHelper.QueryValuations("select * from DevicesRecords");

            //    foreach (var el in list)
            //    {
            //        //adaugare campuri suplimentare
            //        try
            //        {
            //            jsonObject = new JSONObject();
            //            jsonObject
            //                .Put("imei", el.Imei)
            //                .Put("dateTimeISO", el.DateTime)
            //                .Put("geolocation",
            //                    new JSONObject().Put("latitude", $"{el.Latitude}")
            //                        .Put("longitude", $"{el.Longitude}"))
            //                .Put("lastLocation", el.LastLocation)
            //                .Put("sendPanicAlerts", el.SendPanicAlerts)
            //                .Put("stepCounter", el.StepCounter)
            //                .Put("bloodPressureSystolic", el.BloodPresureSystolic)
            //                .Put("bloodPressureDiastolic", el.BloodPresureDiastolic)
            //                .Put("bloodPressurePulseRate", el.BloodPresurePulsRate)
            //                .Put("bloodGlucose", "" + el.BloodGlucose)
            //                .Put("oxygenSaturation", el.OxygenSaturation)
            //                .Put("sleepType", el.SleepType)
            //                .Put("sleepSeconds", el.SecondsOfSleep)
            //                .Put("dailyActivity", el.MinutesOfActivity)
            //                .Put("extension", el.Extension);
            //            jsonArray.Put(jsonObject);
            //        }
            //        catch (JSONException e)
            //        {
            //            e.PrintStackTrace();
            //        }
            //    }

            //    jsonObject = new JSONObject();
            //    jsonObject
            //        .Put("imei", Utils.GetImei(this))
            //        .Put("dateTimeISO", ft.Format(new Date()))
            //        .Put("geolocation", string.Empty)
            //        .Put("lastLocation", string.Empty)
            //        .Put("sendPanicAlerts", string.Empty)
            //        .Put("stepCounter", activity["Steps"])
            //        .Put("bloodPressureSystolic", string.Empty)
            //        .Put("bloodPressureDiastolic", string.Empty)
            //        .Put("bloodPressurePulseRate", string.Empty)
            //        .Put("bloodGlucose", string.Empty)
            //        .Put("oxygenSaturation", string.Empty)
            //        .Put("sleepType", string.Empty)
            //        .Put("sleepSeconds", string.Empty)
            //        .Put("dailyActivity", activity["Activity"].ToString())
            //        .Put("extension", string.Empty);
            //    jsonArray.Put(jsonObject);
            //    var result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
            //    if (result == "Succes!")
            //    {
            //        //Toast.MakeText(this, "Succes", ToastLength.Long).Show();
            //        await sqlHelper.DropTable();
 
                  
            //    }
            //}
        }
    }
}