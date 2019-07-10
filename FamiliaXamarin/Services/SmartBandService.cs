﻿using System;
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
            if (Utils.CheckIfLocationIsEnabled())
            { 
        
                await RefreshToken();
                SentData();
                _handler.PostDelayed(_runnable, _refreshTime * 5);
                
            }
            else
            {

                Log.Error("SmartBand Service", "Operation Aborted because Location is disabled");
                _handler.PostDelayed(_runnable, _refreshTime * 10);
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
                const string channelId = "my_channel_01";
                var channel = new NotificationChannel(channelId, "Channel human readable title",
                    NotificationImportance.Default);

                ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);

                var notification = new NotificationCompat.Builder(this, channelId)
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
                _sqlHelper = await SqlHelper<SmartBandRecords>.CreateAsync();
                Log.Error("Before Token Refresh","aici");
                await RefreshToken();
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device));
                Log.Error("After + token", _token);

                _started = true;
                //SentData();
                _handler.PostDelayed(_runnable, _refreshTime * 5);
            }
        }
        private async Task RefreshToken()
        {
            var refreshToken = Utils.GetDefaults("FitbitRefreshToken");
//            await Task.Run(async () =>
//            {
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
            //});
        }
        private async Task GetSteps()
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

                        var jsonObject = new JSONObject();
                        //jsonObject.Put("imei", Utils.GetImei(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana"));
                        var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                        var date = DateTime.Now.ToString("yyyy-MM-dd");
                        jsonObject.Put("steps", steps).Put("activity", activeMinutes).Put("dateTime", date );
                        //await Task.Run(async () =>
                        //{
                        //        await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/activity", jsonObject, Utils.GetDefaults("Token"));
                        //});


                        if (!Utils.CheckNetworkAvailability())
                        {
                            await _sqlHelper.Insert(new SmartBandRecords { DataObject = jsonObject.ToString(), Type = "Activity" });
                        }
                        else
                        {
                            await Task.Run(async () =>
                            {
                                var recordEnumerable =
                                    await _sqlHelper.QueryValuations($"SELECT * FROM SmartBandRecords WHERE Type  = 'Activity'");
                                var jsonArray = new JSONArray();
                                foreach (var element in recordEnumerable)
                                {
                                    jsonArray.Put(new JSONObject(element.DataObject));
                                    await _sqlHelper.Delete(element);
                                }
                                //await _sqlHelper.QueryValuations($"DELETE FROM SmartBandRecords WHERE Type = 'Activity'");
                                jsonArray.Put(jsonObject);
                                var obj = new JSONObject();
                                obj.Put("imei", Utils.GetImei(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray).Put("latitude", Utils.GetDefaults("Latitude").Replace(',', '.')).Put("longitude",Utils.GetDefaults("Longitude").Replace(',', '.'));

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
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        //throw;
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
                                        Console.WriteLine(e);
                                        //throw;
                                    }
                                }
                            });
                        }


                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                        StopSelf();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
 
               

                if (!Utils.CheckNetworkAvailability())
                {
                    await _sqlHelper.Insert(new SmartBandRecords {DataObject = jsonObject.ToString(), Type = "Sleep"});
                }
                else
                {
                    Log.Error("Avem date de la fitbit", "aici");
//                    await Task.Run(async () =>
//                    {
                        var recordEnumerable =
                            await _sqlHelper.QueryValuations($"SELECT * FROM SmartBandRecords WHERE Type  = 'Sleep'");
                        var jsonArray = new JSONArray();
                        foreach (var element in recordEnumerable)
                        {
                            jsonArray.Put(new JSONObject(element.DataObject));
                            await _sqlHelper.Delete(element);
                        }
                    Log.Error("Din local storage", recordEnumerable.ToString());

                    //await _sqlHelper.QueryValuations($"DELETE FROM SmartBandRecords WHERE Type = 'Sleep'");
                    Log.Error("Array JSON before put", jsonArray.ToString());
                    jsonArray.Put(jsonObject);
                    Log.Error("Array JSON after put", jsonArray.ToString());
                    var obj = new JSONObject();
                    Log.Error("idClient", Utils.GetDefaults("IdClient"));
                    Log.Error("IdPersoana", Utils.GetDefaults("IdPersoana"));
                    Log.Error("Data", jsonArray.ToString());
                    Log.Error("Latitude", Utils.GetDefaults("Latitude")?.Replace(',', '.'));
                    Log.Error("Longitude", Utils.GetDefaults("Longitude")?.Replace(',', '.'));
                    obj.Put("imei", Utils.GetImei(this)).Put("idClient", Utils.GetDefaults("IdClient")).Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray).Put("latitude", Utils.GetDefaults("Latitude").Replace(',', '.')).Put("longitude", Utils.GetDefaults("Longitude").Replace(',', '.'));
                    Log.Error("se trimite la server", obj.ToString());
                    var result = await WebServices.Post($"{Constants.PublicServerAddress}/api/smartband/sleep", obj, Utils.GetDefaults("Token"));
                        if (!string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                var obj1 = new JSONObject(result);
                                if (obj1.GetString("data") != "done")
                                {
                                    await _sqlHelper.Insert(new SmartBandRecords {DataObject = jsonObject.ToString(), Type = "Sleep"});
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                //throw;
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
                                Console.WriteLine(e);
                                //throw;
                            }
                        }
                    //});
                }

            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
                StopSelf();
            }
        }

        private async void SentData()
        {
            //var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
            Log.Error("sleepData", "aici");
            await GetSleepData();
            Log.Error("stepsData", "aici");
            await GetSteps();
            Log.Error("Finish", "aici");
        }
    }
}