﻿using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia;
using Familia.DataModels;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using FamiliaXamarin.Services;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;

namespace Familia.Medicatie
{
    public class MedicineServerFragment : Android.Support.V4.App.Fragment, IOnMedSerListener
    {

        private MedicineServerAdapter _medicineServerAdapter;
        private List<MedicationSchedule> _medications;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;
        private CardView cwEmpty;



        private void setupRecycleView(View view)
        {   _medications = new List<MedicationSchedule>();
            RecyclerView rvMedSer = view.FindViewById<RecyclerView>(Resource.Id.rv_medser);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvMedSer.SetLayoutManager(layoutManager);
            _medicineServerAdapter = new MedicineServerAdapter();
            rvMedSer.SetAdapter(_medicineServerAdapter);
            _medicineServerAdapter.SetListener(this);
            _medicineServerAdapter.setMedsList(_medications);
            _medicineServerAdapter.NotifyDataSetChanged();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
           
            View view = inflater.Inflate(Resource.Layout.fragment_medicine_server, container, false);

            setupRecycleView(view);

           GetData();


            Log.Error("GATA GET DATA", "..");


//            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
//            var numeDB = "devices_data.db";
//            _db = new SQLiteConnection(Path.Combine(path, numeDB));
//            _db.CreateTable<MedicineRecords>();
          


            return view;
        }

        public  override void OnResume()
        {
            base.OnResume();
//            _medications = await Storage.GetInstance().readMedSer();
//            _medicineServerAdapter.setMedsList(_medications);
//            _medicineServerAdapter.NotifyDataSetChanged();
//            Storage.GetInstance().saveMedSer(_medications);
        }

        private async void GetData()
        {

            //open loadin
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            
            dialog.Show();
            await Task.Run(async () => {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("IdClient")}", Utils.GetDefaults("Token"));

                    if (res != null)
                    {
                        Log.Error("RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;

                        _medications = ParseResultFromUrl(res);
                       
                        for (var ms = 0; ms <= _medications.Count; ms++)
                        {
                            Log.Error("MSSSSSTRING", _medications[ms].Timestampstring);
                            var am = (AlarmManager)Activity.GetSystemService(Context.AlarmService);


                            var i = new Intent(Activity, typeof(AlarmBroadcastReceiverServer));

                            i.PutExtra(AlarmBroadcastReceiverServer.Uuid, _medications[ms].Uuid);
                            i.PutExtra(AlarmBroadcastReceiverServer.Title, _medications[ms].Title);
                            i.PutExtra(AlarmBroadcastReceiverServer.Content, _medications[ms].Content);
                            i.PutExtra(AlarmBroadcastReceiverServer.Postpone, _medications[ms].Postpone);

                            i.SetAction(AlarmBroadcastReceiverServer.ActionReceive);
                            var random = new System.Random();
                            var id = CurrentTimeMillis() * random.Next();
                            var pi = PendingIntent.GetBroadcast(Activity, id, i, PendingIntentFlags.UpdateCurrent);

                            if (am == null) continue;

                            var date = parseTimestampStringToDate(_medications[ms]);
                            _medications[ms].Timestampstring = date.ToString();
                            Storage.GetInstance().saveMedSer(_medications);
                            Calendar calendar = Calendar.Instance;
                            Calendar setcalendar = Calendar.Instance;

                            setcalendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
                            Log.Error("DATE YEAR:", date.Year.ToString(), date.Month.ToString(), date.Day.ToString());
                            if (setcalendar.Before(calendar)) continue;
                            Log.Error("A trecut de if", ":)");
                            am.SetInexactRepeating(AlarmType.RtcWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
                        }
                    }
                    else
                    {
                        _medications = await Storage.GetInstance().readMedSer();
                        _medicineServerAdapter.setMedsList(_medications);
                        _medicineServerAdapter.NotifyDataSetChanged();

                        Activity.RunOnUiThread(() =>
                        {
                            Log.Error("RESULT_FOR_MEDICATIE", "nu se poate conecta la server");
                            Toast.MakeText(Activity, "Nu se poate conecta la server", ToastLength.Short).Show();
                        });

                        
                    }
                }
                catch (Exception e)
                {
                    Log.Error("AlarmError", e.Message);
                }


            });

            dialog.Dismiss();

            _medicineServerAdapter.setMedsList(_medications);
            _medicineServerAdapter.NotifyDataSetChanged();
            Storage.GetInstance().saveMedSer(_medications);


            if (_medications.Count == 0)
            {
                cwEmpty.Visibility = ViewStates.Visible;
            }
            else
            {
                cwEmpty.Visibility = ViewStates.Gone;
            }

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
                Log.Error("EROARE", "nu intra in try");
            }
            return date.ToLocalTime();
        }


        public int CurrentTimeMillis()
        {
            return (DateTime.UtcNow).Millisecond;
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
                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone));
                    Log.Error("MEDICATIONSTRING", timestampString);
                }

                return medicationScheduleList;
            }

            return null;
        }

        public void OnMedSerClick(MedicationSchedule med)
        {
            Log.Error("MED", med.ToString());
            var alert = new Android.Support.V7.App.AlertDialog.Builder(Activity);
            var medDate = Convert.ToDateTime(med.Timestampstring);
            var currentDate = DateTime.Now;

            if (medDate < currentDate)
            {
                alert.SetTitle("Doriti sa marcati medicamentul ca fiind administrat?");
                alert.SetMessage(med.Title + ", " + med.Content);
                alert.SetPositiveButton("Da", async (senderAlert, args) => {

                    var now = DateTime.Now;
                    var mArray = new JSONArray().Put(new JSONObject().Put("uuid", med.Uuid)
                        .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

                    bool isSent = await SendMedicationTask(mArray, med, now);
                    if (isSent)
                    {
                        Toast.MakeText(Context, "Medicament administrat.", ToastLength.Long).Show();
                        _medications.Remove(med);
                        _medicineServerAdapter.removeItem(med);
                        _medicineServerAdapter.NotifyDataSetChanged();
                    }
                    //                Toast.MakeText(Context, "....", ToastLength.Long).Show();
                    if (_medications.Count == 0)
                    {
                        cwEmpty.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        cwEmpty.Visibility = ViewStates.Gone;
                    }

                });

                alert.SetNegativeButton("Nu", (senderAlert, args) => {
                });
            }
            else
            {
                alert.SetTitle("Acest medicament nu se poate marca ca fiind administrat!");
                alert.SetMessage(med.Title + ", " + med.Content);
                alert.SetPositiveButton("Ok", async (senderAlert, args) => {

                });

//                alert.SetNegativeButton("Nu", (senderAlert, args) => {
//                });
            }





            Dialog dialog = alert.Create();
            dialog.Show();
        }


        public async Task<bool> SendMedicationTask(JSONArray mArray, MedicationSchedule med, DateTime now)
        {
            bool isOk = false;
            await  Task.Run(async () =>
            {
                bool isSent = await SendData(Context, mArray);
                Log.Error("RESULT", isSent + " !");
                if (isSent)
                {
                    var running = IsServiceRunning(typeof(MedicationService), Context);
                    if (running)
                    {
                        Log.Error("SERVICE", "Medication service is running");
                        Context.StopService(_medicationServiceIntent);
                    }

                    isOk = true;
                }
                else
                {
                    AddMedicine(_db, med.Uuid, now);
                    Log.Error("SERVICE", "Medication service started");
                    _medicationServiceIntent =
                        new Intent(Context, typeof(MedicationService));
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        Context.StartForegroundService(_medicationServiceIntent);
                    }
                    else
                    {
                        Context.StartService(_medicationServiceIntent);
                    }

                    isOk = false;
                }
            });

            return isOk;
        }

        private static async Task<bool> SendData(Context context, JSONArray mArray)
        {
            var result = await WebServices.Post(
                $"{Constants.PublicServerAddress}/api/medicine", mArray,
                Utils.GetDefaults("Token"));
            Log.Error("RESULT POST: ", result);
            if (!Utils.CheckNetworkAvailability()) return false;

            Log.Error("RESULT POST: ", "a trcut de network");

            switch (result)
            {
                case "done":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsServiceRunning(Type classTypeof, Context context)
        {
            var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
#pragma warning disable 618
            return manager.GetRunningServices(int.MaxValue).Any(service =>
                service.Service.ShortClassName == classTypeof.ToString());
        }

        private static async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {
            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}