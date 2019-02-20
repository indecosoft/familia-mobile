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
using Familia;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.Lang;
using Java.Text;
using Java.Util;
using Java.Util.Concurrent;
using Org.Json;
using SQLite;
using Environment = System.Environment;
using Exception = System.Exception;
using Math = System.Math;

namespace FamiliaXamarin.Services
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
                _handler.PostDelayed(_runnable, _refreshTime*5);
            }
        }

        private SQLiteAsyncConnection _db;

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
            Log.Error("Service:", "WebSocketService STARTED");
            try
            {

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    string CHANNEL_ID = "my_channel_01";
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Channel human readable title",
                        NotificationImportance.Default);

                    ((NotificationManager)GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);

                    Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                        .SetContentTitle("")
                        .SetContentText("").Build();

                    StartForeground(1, notification);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Error("SmartBand Service", "Started");

            RefreshToken();
            _started = true;
            _handler.PostDelayed(_runnable, _refreshTime*10);



            return StartCommandResult.Sticky;
        }

        private void RefreshToken()
        {
            string refreshToken = Utils.GetDefaults("FitbitRefreshToken", this);
            Task.Run(async () =>
            {
                try
                {
                    string storedToken = Utils.GetDefaults("FitbitToken", this);
                    if (!string.IsNullOrEmpty(storedToken))
                    {
                        //  var tokenStateDict = new Dictionary<string, string>
                        // {
                        //     {"token", storedToken}
                        // };
                        _token = storedToken;
                        //var responseTokenState = await WebServices.Post("https://api.fitbit.com/1.1/oauth2/introspect", tokenStateDict);
                        //if (responseTokenState != null && new JSONObject(responseTokenState).GetBoolean("active") == false)
                        //{
                        var dict = new Dictionary<string, string>
                            {
                                {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                            };
                        var response = await WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                        if (response != null)
                        {
                            var obj = new JSONObject(response);
                            _token = obj.GetString("access_token");
                            var newRefreshToken = obj.GetString("refresh_token");
                            var userId = obj.GetString("user_id");
                            Utils.SetDefaults(GetString(Resource.String.smartband_device), _token, this);
                            Utils.SetDefaults("FitbitToken", _token, this);
                            Utils.SetDefaults("RitbitRefreshToken", newRefreshToken, this);
                            Utils.SetDefaults("FitbitUserId", userId, this);
                        }
                        // }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("FitbitServiceError", e.Message);
                }

                //SentData();
                //var a  = await GetSleepData();

            });
        }



        private async Task<IEnumerable<DevicesRecords>> QueryValuations(SQLiteAsyncConnection db, string query)
        {
            return await db.QueryAsync<DevicesRecords>(query);
        }
        private async void AddGlucoseRecord(SQLiteAsyncConnection db, Dictionary<string,int> activity, string sleep, string pulse)
        {
            var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
            await db.InsertAsync(new DevicesRecords()
            {
                Imei = Utils.GetImei(this),
                DateTime = ft.Format(new Date()),
                SecondsOfSleep = sleep,
                MinutesOfActivity = activity["Activity"].ToString(),
                StepCounter = activity["Steps"]
            });

        }
        private async Task<Dictionary<string, int>> GetSteps()
        {
            try
            {
                var data = await WebServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
                if (!string.IsNullOrEmpty(data))
                {
                    Log.Error("Steps Result", data);

                    try
                    {
                        var fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("fairlyActiveMinutes");
                        var veryActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("veryActiveMinutes");
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

        private async Task<string> GetSleepData()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);
            Log.Error("Sleep Result", data);
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                    JSONObject jsonObject;
                    var jsonArray = new JSONArray();
                    var array = new JSONObject(data).GetJSONArray("sleep").GetJSONObject(0);
                    var listOfSleepTypes = array.GetJSONObject("levels").GetJSONArray("data");
                    for (int i = 0; i < listOfSleepTypes.Length(); i++)
                    {
                        var a = listOfSleepTypes.GetJSONObject(i);
                      
                            jsonObject = new JSONObject();
                            jsonObject
                                .Put( "imei", Utils.GetImei(this))
                                .Put("dateTimeISO", ft.Format(new Date()))
                                .Put("geolocation", string.Empty)
                                .Put("lastLocation", string.Empty)
                                .Put("sendPanicAlerts", string.Empty)
                                .Put("stepCounter", string.Empty)
                                .Put("bloodPressureSystolic", string.Empty)
                                .Put("bloodPressureDiastolic", string.Empty)
                                .Put("bloodPressurePulseRate", string.Empty)
                                .Put("bloodGlucose", string.Empty)
                                .Put("oxygenSaturation", string.Empty)
                                .Put("sleepType", a.GetString("level"))
                                .Put("sleepSeconds", a.GetInt("seconds"))
                                .Put("dailyActivity", string.Empty)
                                .Put("extension", string.Empty);
                            jsonArray.Put(jsonObject);
                    }

                    await Task.Run(async () =>
                    {
                        var response = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                    });

//                    var h = (int)TimeUnit.Minutes.ToHours(totalMinutesAsleep);
//                    var min = (int)(((float)totalMinutesAsleep / 60 - h) * 100) * 60 / 100;
//                    return $"{h} hr {min} min";
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }

            return null;
        }

        private async Task<string> GetHeartRatePulse()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Heartrate Result", data);
                try
                {
                    var bpm = ((JSONObject)new JSONObject(data).GetJSONArray("activities-heart").Get(0)).GetJSONObject("value").GetInt("restingHeartRate");
                    var bmpText = $"{bpm} bpm";

                        return bmpText;
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }

            return null;
        }
        private async void SentData()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            const string numeDb = "devices_data.db";
            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDb));

            var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
            var sleep = await GetSleepData();
            await _db.CreateTableAsync<DevicesRecords>();
            var activity = await GetSteps();
           
            var pulse = await GetHeartRatePulse();
            if (!Utils.CheckNetworkAvailability())
                AddGlucoseRecord(_db, activity, sleep, pulse);
            else
            {
                JSONObject jsonObject;
                var jsonArray = new JSONArray();
                var list = await QueryValuations(_db, "select * from DevicesRecords");

                foreach (var el in list)
                {
                    //adaugare campuri suplimentare
                    try
                    {
                        jsonObject = new JSONObject();
                        jsonObject
                            .Put("imei", el.Imei)
                            .Put("dateTimeISO", el.DateTime)
                            .Put("geolocation", new JSONObject().Put("latitude", $"{el.Latitude}").Put("longitude", $"{el.Longitude}"))
                            .Put("lastLocation", el.LastLocation)
                            .Put("sendPanicAlerts", el.SendPanicAlerts)
                            .Put("stepCounter", el.StepCounter)
                            .Put("bloodPressureSystolic", el.BloodPresureSystolic)
                            .Put("bloodPressureDiastolic", el.BloodPresureDiastolic)
                            .Put("bloodPressurePulseRate", el.BloodPresurePulsRate)
                            .Put("bloodGlucose", "" + el.BloodGlucose)
                            .Put("oxygenSaturation", el.OxygenSaturation)
                            .Put("sleepType", el.SleepType)
                            .Put("sleepSeconds", el.SecondsOfSleep)
                            .Put("dailyActivity", el.MinutesOfActivity)
                            .Put("extension", el.Extension);
                        jsonArray.Put(jsonObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                }
                jsonObject = new JSONObject();
                jsonObject
                    .Put("imei", Utils.GetImei(this))
                    .Put("dateTimeISO", ft.Format(new Date()))
                    .Put("geolocation", string.Empty)
                    .Put("lastLocation", string.Empty)
                    .Put("sendPanicAlerts", string.Empty)
                    .Put("stepCounter", activity["Steps"])
                    .Put("bloodPressureSystolic", string.Empty)
                    .Put("bloodPressureDiastolic", string.Empty)
                    .Put("bloodPressurePulseRate", string.Empty)
                    .Put("bloodGlucose", string.Empty)
                    .Put("oxygenSaturation", string.Empty)
                    .Put("sleepType", string.Empty)
                    .Put("sleepSeconds", string.Empty)
                    .Put("dailyActivity", activity["Activity"].ToString())
                    .Put("extension", string.Empty);
                jsonArray.Put(jsonObject);
                string result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                if (result == "Succes!")
                {
                    //Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                    await _db.DropTableAsync<DevicesRecords>();
                }
                else
                {
                    //Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                }

            }

        }
    }
}