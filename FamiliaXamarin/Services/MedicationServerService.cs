using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Helpers;
using Familia.Medicatie.Alarm;
using Familia.Medicatie.Data;
using Familia.Medicatie.Entities;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using Exception = System.Exception;
using Random = System.Random;
using TimeZone = Java.Util.TimeZone;

namespace Familia.Services
{
    [Service]
    class MedicationServerService : Service
    {
        private readonly Handler _handler = new Handler();
        private int _refreshTime = 1000;
        private readonly Runnable _runnable;
        private List<MedicationSchedule> _medications;
        private string Log_Tag = "MedicationSS";

        private async void HandlerRunnable()
        {
            if (Utils.CheckNetworkAvailability())
            {
                if (int.Parse(Utils.GetDefaults("UserType")) == 3)
                {
                    Log.Error(Log_Tag, "server call");
                    _medications = new List<MedicationSchedule>();
                    GetData();
                }
                else
                {
                    Log.Error(Log_Tag, "another type of user");
                }
                _handler.PostDelayed(_runnable, _refreshTime * 3600 * 24);
            }
            else
            {
                Log.Error(Log_Tag, "Operation Aborted because Network is disabled");
                _handler.PostDelayed(_runnable, _refreshTime * 10);
            }
        }

        private async void GetData()
        {

            await Task.Run(async () =>
            {
                try
                {
                    await cancelPendingIntentsForMedicationSchedule();//_medications
                    string res = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("Id")}", Utils.GetDefaults("Token"));

                    if (res != null)
                    {
                        Log.Error(Log_Tag, "RESULT_FOR_MEDICATIE" + res);
                        if (res.Equals("[]")) return;
                        _medications = ParseResultFromUrl(res);
                        Log.Error(Log_Tag, "COUNT MEDICATIE " + _medications.Count);
                    }
                    else
                    {
                        Log.Error(Log_Tag, "res is null");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(Log_Tag, "Alarm error " + e.Message);
                }
            });

            Log.Error(Log_Tag, _medications.Count + " _med list med count");

            if (_medications.Count != 0)
            {

                Log.Error(Log_Tag, "current count of medications from server " + _medications.Count);
                Log.Error(Log_Tag, "start showing what it's in medication list after received and parsed from server");
                foreach (MedicationSchedule item in _medications)
                {
                    Log.Error(Log_Tag, item.Title + ", " + item.Timestampstring + ", idNotification " + item.IdNotification + ", " + item.Postpone + ", UUID: " + item.Uuid);
                }
                Log.Error(Log_Tag, "the list is finished");



                var list = new List<MedicationSchedule>();

                for (var ms = 0; ms < _medications.Count; ms++)
                {
                    Log.Error(Log_Tag, _medications.Count + " in for");
                    bool x = await Storage.GetInstance().isHere(_medications[ms].Uuid);
                    Log.Error(Log_Tag, "is here? " + x);
                    if (x == false)
                    {
                        list.Add(_medications[ms]);
                        Log.Error(Log_Tag, "count of meds" + _medications.Count);
                        Log.Error(Log_Tag, "setting alarm for an uuid that does not exist in local db");
                        SetupAlarm(ms, _medications[ms].IdNotification);
                    }
                    else
                    {
                        Log.Error(Log_Tag, "so the uuid is here and start setting the alarm for them");
                        Log.Error(Log_Tag, "the item is: " + _medications[ms].Timestampstring + ", idNotification " + _medications[ms].IdNotification + ", " + _medications[ms].Postpone + ", UUID: " + _medications[ms].Uuid);
                        
                        var medDate = Convert.ToDateTime(_medications[ms].Timestampstring);
                        DateTime currentDate = DateTime.Now;

                        if (medDate >= currentDate)
                        {
                            MedicationSchedule medObj = await Storage.GetInstance().getElementByUUID(_medications[ms].Uuid);
                            if (medObj.IdNotification == 0)
                            {
                                Log.Error(Log_Tag, "so the uuid is 0, I'm updating the PI'S id and set up alarm for the uuid " + _medications[ms].Uuid);
                                SetupAlarm(ms, _medications[ms].IdNotification);
                                Log.Error(Log_Tag, "UPDATED PI'S ID " + _medications[ms].IdNotification);
                                await Storage.GetInstance().removeMedSer(_medications[ms].Uuid);
                                await Storage.GetInstance().insertElementMedSer(_medications[ms]);
                               Log.Error(Log_Tag, "remove & insert element for pi's id != 0 and uuid already exists");

                            }
                            else
                            {
                                Log.Error(Log_Tag, "set alarm for uuid that is in local db, pi's id  " + medObj.IdNotification + " uuid " + medObj.Uuid);
                                SetupAlarm(ms, medObj.IdNotification);
                            }

                            Log.Error(Log_Tag, "setup alarm ");
                        }
                    }
                }

                await Storage.GetInstance().saveMedSer(list);
                Log.Error(Log_Tag, "saving this list.. ");
                foreach (var item in list) {
                    Log.Error(Log_Tag, "pi id " + item.IdNotification + " timestamp " + item.Timestampstring + " title  " + item.Title + " content " + item.Content + " uuit " + item.Uuid);
                }

                //test

                Log.Error(Log_Tag, "start reading what is saved in MedicineServerRecords .. ");
                var testList = await Storage.GetInstance().readMedSer();
                foreach (var item in testList)
                {
                    Log.Error(Log_Tag, "pi id " + item.IdNotification + " timestamp " + item.Timestampstring + " title  " + item.Title + " content " + item.Content + " uuit " + item.Uuid);
                }
                Log.Error(Log_Tag, "read is finished");

            }
            await Storage.GetInstance().deleteStinkyItems(_medications);
        }


        private async Task cancelPendingIntentsForMedicationSchedule()//List<MedicationSchedule> _medications
        {
            Log.Error(Log_Tag, "start canceling pi");
            NetworkingData networking = new NetworkingData();
            List<MedicationSchedule> list = new List<MedicationSchedule>(await networking.ReadListFromDbFutureDataTask());


            foreach (MedicationSchedule item in list)
            {
                Log.Error(Log_Tag, "canceling pi .. " + item.IdNotification);
                var am = (AlarmManager)Application.Context.GetSystemService(AlarmService);
                var intent = new Intent(BaseContext, typeof(AlarmBroadcastReceiverServer));
                PendingIntent pi = PendingIntent.GetBroadcast(Application.Context, item.IdNotification, intent, 0);
                //pi.Cancel();

                if (pi == null)
                {
                    Log.Error(Log_Tag, "pi is null");
                }
                else
                {

                    Log.Error(Log_Tag, "pi is not null");
                }

                am.Cancel(pi);
                pi.Cancel();
               // networking.removeMedSer(item.Uuid);
            }

        }

        private List<MedicationSchedule> ParseResultFromUrl(string res)
        {
            var random = new System.Random(1);
            var id = CurrentTimeMillis();
            if (res != null)
            {
                var medicationScheduleList = new List<MedicationSchedule>();
                var results = new JSONArray(res);

                for (var i = 0; i < results.Length(); i++)
                {
                    var obj = (JSONObject)results.Get(i);
                    string uuid = obj.GetString("uuid");
                    string timestampString = obj.GetString("timestamp");
                    string title = obj.GetString("title");
                    string content = obj.GetString("content");
                    var postpone = Convert.ToInt32(obj.GetString("postpone"));

                    id += 10;

                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone, id));
                    Log.Error(Log_Tag, "MEDICATIONSTRING " + timestampString);
                }
                return medicationScheduleList;
            }
            return null;
        }


        private void SetupAlarm(int ms, int id)
        {
            Log.Error(Log_Tag, "setupAlarm method" + _medications[ms].Timestampstring);
            var am = (AlarmManager)Application.Context.GetSystemService(AlarmService);
            var i = new Intent(this, typeof(AlarmBroadcastReceiverServer));

            i.PutExtra(AlarmBroadcastReceiverServer.Uuid, _medications[ms].Uuid);
            i.PutExtra(AlarmBroadcastReceiverServer.Title, _medications[ms].Title);
            i.PutExtra(AlarmBroadcastReceiverServer.Content, _medications[ms].Content);
            i.PutExtra(AlarmBroadcastReceiverServer.Postpone, _medications[ms].Postpone);
            i.PutExtra(AlarmBroadcastReceiverServer.IdPendingIntent, _medications[ms].IdNotification);

            i.SetAction(AlarmBroadcastReceiverServer.ActionReceive);
            PendingIntent pi = PendingIntent.GetBroadcast(Application.Context, id, i, PendingIntentFlags.UpdateCurrent);

            if (am == null) return;

            DateTime date = parseTimestampStringToDate(_medications[ms]);

            _medications[ms].Timestampstring = date.ToString();
            var calendar = Calendar.Instance;
            var setcalendar = Calendar.Instance;

            setcalendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
            Log.Error(Log_Tag, "Date " + date.Year + ", " + date.Month + ", " + date.Day + ", " + date.Second);
            if (setcalendar.Before(calendar)) return;
            // am.SetInexactRepeating(AlarmType.RtcWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
            am.SetExact(AlarmType.RtcWakeup, setcalendar.TimeInMillis, pi);
        }

        private DateTime parseTimestampStringToDate(MedicationSchedule ms)
        {
            DateFormat utcFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'")
            {
                TimeZone = TimeZone.GetTimeZone("UTC")
            };
            var date = new DateTime();
            try
            {
                date = DateTime.Parse(ms.Timestampstring);
                DateFormat pstFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS")
                {
                    TimeZone = TimeZone.GetTimeZone("PST")
                };
                Log.Error(Log_Tag, "timestampstring parsed" + date.ToLocalTime().ToString());
            }
            catch (ParseException e)
            {
                e.PrintStackTrace();
            }
            return date.ToLocalTime();
        }


        public int CurrentTimeMillis()
        {
            return (DateTime.UtcNow).Millisecond;
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
        public MedicationServerService()
        {
            _runnable = new Runnable(HandlerRunnable);
        }
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Error("Service:", "MedicationServer Service STARTED - MedicationSS");
            try
            {

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
                Log.Error(Log_Tag, "error occured " + e.Message);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _handler.PostDelayed(_runnable, _refreshTime * 5);
            return StartCommandResult.Sticky;
        }
    }
}