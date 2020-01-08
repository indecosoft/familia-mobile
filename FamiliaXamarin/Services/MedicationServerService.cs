using System;
using System.Collections.Generic;
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
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using Exception = System.Exception;

namespace Familia.Services
{
    [Service]
    class MedicationServerService : Service
    {
        private const int ServiceRunningNotificationId = 10000;
        private readonly Handler _handler = new Handler();
        private int _refreshTime = 1000;
        private readonly Runnable _runnable;
        private List<MedicationSchedule> _medications;

        private async void HandlerRunnable()
        {
            if (Utils.CheckNetworkAvailability())
            {
                if (int.Parse(Utils.GetDefaults("UserType")) == 3)
                {
                    Log.Error("MedicationServer Service", "server call");
                    _medications = new List<MedicationSchedule>();
                    GetData();
                }
                else
                {
                    Log.Error("MedicationServer Service", "another type of user");
                }
                _handler.PostDelayed(_runnable, _refreshTime * 3600 * 24);
            }
            else
            {
                Log.Error("MedicationServer Service", "Operation Aborted because Network is disabled");
                _handler.PostDelayed(_runnable, _refreshTime * 10);
            }
        }

        private async void GetData()
        {

            await Task.Run(async () => {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("Id")}", Utils.GetDefaults("Token"));

                    if (res != null)
                    {

                        Log.Error("if", "aici");
                        Log.Error("RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;
                        _medications = ParseResultFromUrl(res);
                        Log.Error("COUNT MEDICATIE", _medications.Count + "");
                    }
                }
                catch (Exception e)
                {
                    Log.Error("AlarmError", e.Message);
                }
            });
           
            Log.Error("MedicationServer Service", _medications.Count + "_med list med count");

            if (_medications.Count != 0)
            {
                var list = new List<MedicationSchedule>();
                Log.Error("MedicationServer Service", _medications.Count + " in if");

                for (var ms = 0; ms < _medications.Count; ms++)
                {
                    Log.Error("MedicationServer Service", _medications.Count + " in for");
                    bool x = await Storage.GetInstance().isHere(_medications[ms].Uuid);
                    Log.Error("MedicationServerService", x + " ");
                    if (x == false)
                    {   list.Add(_medications[ms]);
                        Log.Error("MedicationServer Service", _medications.Count + " in if storage false");
                        SetupAlarm(ms, _medications[ms].IdNotification);
                    }
                    else
                    {
                        var medDate = Convert.ToDateTime(_medications[ms].Timestampstring);
                        var currentDate = DateTime.Now;

                        if (medDate >= currentDate)
                        {
                            var medObj = await Storage.GetInstance().getElementByUUID(_medications[ms].Uuid);
                            if (medObj.IdNotification == 0)
                            {
                                SetupAlarm(ms, _medications[ms].IdNotification);
                            }
                            else
                            {
                                SetupAlarm(ms, medObj.IdNotification);
                            }

                            Log.Error("MedicationServer Service", "setup alarm ");
                        }
                    }
                }
               await Storage.GetInstance().saveMedSer(list);
            }
        }

        private List<MedicationSchedule> ParseResultFromUrl(string res)
        {
            if (res != null)
            {
                var medicationScheduleList = new List<MedicationSchedule>();
                var results = new JSONArray(res);

                for (var i = 0; i < results.Length(); i++)
                {
                    var obj = (JSONObject)results.Get(i);
                    var uuid = obj.GetString("uuid");
                    var timestampString = obj.GetString("timestamp");
                    var title = obj.GetString("title");
                    var content = obj.GetString("content");
                    var postpone = Convert.ToInt32(obj.GetString("postpone"));

                    var random = new System.Random();
                    var id = CurrentTimeMillis() * random.Next();

                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone, id));
                    Log.Error("MEDICATIONSTRING", timestampString);
                }
                return medicationScheduleList;
            }
            return null;
        }


        private void SetupAlarm(int ms, int id)
        {
            Log.Error("MSSSSSTRING", _medications[ms].Timestampstring);
            var am = (AlarmManager)this.GetSystemService(AlarmService);
            var i = new Intent(this, typeof(AlarmBroadcastReceiverServer));

            i.PutExtra(AlarmBroadcastReceiverServer.Uuid, _medications[ms].Uuid);
            i.PutExtra(AlarmBroadcastReceiverServer.Title, _medications[ms].Title);
            i.PutExtra(AlarmBroadcastReceiverServer.Content, _medications[ms].Content);
            i.PutExtra(AlarmBroadcastReceiverServer.Postpone, _medications[ms].Postpone);

            i.SetAction(AlarmBroadcastReceiverServer.ActionReceive);
            var pi = PendingIntent.GetBroadcast(this, id, i, PendingIntentFlags.UpdateCurrent);

            if (am == null) return;

            var date = parseTimestampStringToDate(_medications[ms]);

            _medications[ms].Timestampstring = date.ToString();
            Calendar calendar = Calendar.Instance;
            Calendar setcalendar = Calendar.Instance;

            setcalendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
            Log.Error("DATE ", date.Year + ", " + date.Month + ", " + date.Day + ", " + date.Second);
            if (setcalendar.Before(calendar)) return;
            am.SetInexactRepeating(AlarmType.RtcWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
        }

        private DateTime parseTimestampStringToDate(MedicationSchedule ms)
        {
            DateFormat utcFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'")
            {
                TimeZone = Java.Util.TimeZone.GetTimeZone("UTC")
            };
            DateTime date = new DateTime();
            try
            {
                date = DateTime.Parse(ms.Timestampstring);
                DateFormat pstFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS")
                {
                    TimeZone = Java.Util.TimeZone.GetTimeZone("PST")
                };
                Log.Error("TIMESTAMPSTRING", date.ToLocalTime().ToString());
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
            Log.Error("Service:", "MedicationServer Service STARTED");
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
                const string channelId = "my_channel_01";
                var channel = new NotificationChannel(channelId, "Medication",
                    NotificationImportance.Low);
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
                Log.Error("MedicationServer Service Error occurred", e.Message);
            }
    }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _handler.PostDelayed(_runnable, _refreshTime * 5);
            return StartCommandResult.Sticky;
        }
    }
}