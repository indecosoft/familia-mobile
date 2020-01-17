using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.DataModels;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.SmartBand;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Alarm;
using Java.IO;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;
using Console = System.Console;
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
        private SqlHelper<SmartBandRecords> _sqlHelper;




        private async void HandlerRunnable()
        {
            if (Utils.CheckIfLocationIsEnabled() && Utils.CheckNetworkAvailability())
            {

                await RefreshToken();
                SentData();
                _handler.PostDelayed(_runnable, _refreshTime * 3600 * 6);

            }
            else
            {

                Log.Error("SmartBand Service", "Operation Aborted because Location or Network is disabled");
                _handler.PostDelayed(_runnable, _refreshTime * 3600 * 3);
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
                if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
                /*const string channelId = "my_channel_01";
                var channel = new NotificationChannel(channelId, "Smartband",
                    NotificationImportance.Default)
                { Importance = NotificationImportance.Low };
                ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);
                */


                var notification = new NotificationCompat.Builder(this, App.NonStopChannelIdForServices)
                    .SetContentTitle("Familia")
                    .SetContentText("Ruleaza in fundal")
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetOngoing(true)
                    .Build();


                StartForeground(App.NonstopNotificationIdForServices, notification);
            }
            catch (Exception e)
            {
                Log.Error("SmartBand Service Error occurred", e.Message);
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
                Log.Error("SmartBand Service", "no data about smartband device");
                StopSelf();
            }
            else
            {
                Log.Error("SmartBand Service", "data about smartband device exists");
                _sqlHelper = await SqlHelper<SmartBandRecords>.CreateAsync();
                await RefreshToken();
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device));

                _started = true;
                _handler.PostDelayed(_runnable, _refreshTime * 5);
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
                        Log.Error("SmartBand Service", "inside refreshToken method. data about smartband device exits");
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
                    Log.Error("SmartBand Service Error occurred", e.Message);
                    StopSelf();
                }

            });
        }
        private async Task GetSteps()
        {
            try
            {
                await Task.Run(async () =>
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

                                var jsonObject = new JSONObject();
                                var date = DateTime.Now.ToString("yyyy-MM-dd");
                                jsonObject.Put("steps", steps).Put("activity", activeMinutes).Put("dateTime", date);
  
                                if (!Utils.CheckNetworkAvailability())
                                {
                                    await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Activity" });
                                }
                                else
                                {
                                    var recordEnumerable =
                                        await _sqlHelper.QueryValuations($"SELECT * FROM SmartBandRecords WHERE Type  = 'Activity'");
                                    var jsonArray = new JSONArray();
                                    foreach (var element in recordEnumerable)
                                    {
                                        jsonArray.Put(new JSONObject(element.DataObject));
                                        await _sqlHelper.Delete(element);
                                    }
                                    jsonArray.Put(jsonObject);
                                    var obj = new JSONObject();
                                    obj.Put("imei", Utils.GetDeviceIdentificator(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray).Put("latitude", Utils.GetDefaults("Latitude").Replace(',', '.')).Put("longitude", Utils.GetDefaults("Longitude").Replace(',', '.'));

                                    var result = await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/activity", obj, Utils.GetDefaults("Token"));

                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        try
                                        {
                                            var obj1 = new JSONObject(result);
                                            if (obj1.GetString("data") != "done")
                                            {
                                                await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Activity" });
                                            }
                                            else
                                            {
                                                await _sqlHelper.QueryValuations($"DELETE FROM SmartBandRecords WHERE Type = 'Activity'");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error("SmartBand Service Error occurred", e.Message);
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Activity" });
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error("SmartBand Service Error occurred", e.Message);
                                        }
                                    }
                                }
                            }
                            catch (JSONException e)
                            {
                                Log.Error("SmartBand Service Error occurred", e.Message);
                                StopSelf();
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                Log.Error("SmartBand Service Error occurred", e.Message);
            }
        }

        private async Task GetSleepData()
        {
            await Task.Run(async () =>
            {
                var data =
                await WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json",
                    _token);

                if (string.IsNullOrEmpty(data)) return;
                try
                {
                    var jsonObject = new JSONObject(data);
                    if (!Utils.CheckNetworkAvailability())
                    {
                        await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Sleep" });
                    }
                    else
                    {
                        var recordEnumerable =
                            await _sqlHelper.QueryValuations($"SELECT * FROM SmartBandRecords WHERE Type  = 'Sleep'");
                        var jsonArray = new JSONArray();
                        foreach (var element in recordEnumerable)
                        {
                            jsonArray.Put(new JSONObject(element.DataObject));
                            await _sqlHelper.Delete(element);
                        }
                        jsonArray.Put(jsonObject);
                        var obj = new JSONObject();
                        obj.Put("imei", Utils.GetDeviceIdentificator(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray).Put("latitude", Utils.GetDefaults("Latitude").Replace(',', '.')).Put("longitude", Utils.GetDefaults("Longitude").Replace(',', '.'));
                        var result = await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/sleep", obj, Utils.GetDefaults("Token"));
                        if (!string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                var obj1 = new JSONObject(result);
                                if (obj1.GetString("data") != "done")
                                {
                                    await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Sleep" });
                                }
                                else
                                {
                                    await _sqlHelper.QueryValuations($"DELETE FROM SmartBandRecords WHERE Type = 'Sleep'");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error("SmartBand Service Error occurred", e.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Sleep" });
                            }
                            catch (Exception e)
                            {
                                Log.Error("SmartBand Service Error occurred", e.Message);
                            }
                        }
                    }

                }
                catch (JSONException e)
                {
                    Log.Error("SmartBand Service Error occurred", e.Message);
                    StopSelf();
                }
            });
            
        }

        private async void SentData()
        {
            if (!Utils.CheckNetworkAvailability()) return;
            await GetSleepData();
            await GetSteps();

        }
    }
}