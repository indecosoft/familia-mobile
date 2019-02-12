using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;

namespace FamiliaXamarin.Medicatie
{
    public class MedicineFragment : Android.Support.V4.App.Fragment ,View.IOnClickListener, IOnBoalaClickListener, CustomDialogDeleteDisease.ICustomDialogDeleteDiseaseListener
    {

//        private ProgressBarDialog _progressBarDialog;
        public static string IdBoala = "id_boala";
        private DiseaseAdapter _boalaAdapter;
        private List<MedicationSchedule> _medications;
        private SQLiteConnection _db;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_medicine, container, false);
            view.FindViewById(Resource.Id.btn_add_disease).SetOnClickListener(this);
            setupRecycleView(view);

            GetData();
            
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
            _db = new SQLiteConnection(Path.Combine(path, numeDB));
            _db.CreateTable<MedicineRecords>();
            
          

            return view;
        }


        private async void GetData()
        {
            await Task.Run(async () => {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("IdClient", Activity)}", Utils.GetDefaults("Token", Activity));

                    if (res != null)
                    {
                        Log.Error("RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;
                        _medications = ParseResultFromUrl(res);
                        for(var ms = 0; ms <= _medications.Count; ms++)
                        {
                            Log.Error("MSSSSSTRING", _medications[ms].Timestampstring);
                            var am = (AlarmManager)Activity.GetSystemService(Context.AlarmService);
                            var i = new Intent(Activity, typeof(AlarmBroadcastReceiverServer));

                            i.PutExtra(AlarmBroadcastReceiverServer.Uuid, _medications[ms].Uuid);
                            i.PutExtra(AlarmBroadcastReceiverServer.Title, _medications[ms].Title);
                            i.PutExtra(AlarmBroadcastReceiverServer.Content, _medications[ms].Content);
                            i.SetAction(AlarmBroadcastReceiverServer.ActionReceive);
                            var random = new System.Random();
                            var id = CurrentTimeMillis() * random.Next();
                            //var pi = PendingIntent.GetBroadcast(Activity, id, i, PendingIntentFlags.OneShot);
                            var pi = PendingIntent.GetBroadcast(Activity, id, i, PendingIntentFlags.UpdateCurrent);
                            //var pi = PendingIntent.GetBroadcast(Activity, id, i, PendingIntentFlags.UpdateCurrent);
                            if (am == null) continue;
                            var date = parseTimestampStringToDate(_medications[ms]);

                            Calendar calendar = Calendar.Instance;
                            Calendar setcalendar = Calendar.Instance;

                            setcalendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
                            Log.Error("DATE YEAR:", date.Year.ToString());
                            if (setcalendar.Before(calendar)) continue;

                            am.SetInexactRepeating(AlarmType.RtcWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
                        }
                    }
                    else
                    {
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
                    var obj = (JSONObject) results.Get(i);
                    var uuid = obj.GetString("uuid");
                    var timestampString = obj.GetString("timestamp");
                    var title = obj.GetString("title");
                    var content = obj.GetString("content");
                    var postpone = Convert.ToInt32(obj.GetString("postpone"));
                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone));
                    //Log.Error("MEDICATIONSTRING", timestampString);
                }

                return medicationScheduleList;
            }

            return null;
        }
        
        
        

        public override void OnResume()
        {
            base.OnResume();
            SetListForAdapter();
        }

        private void SetListForAdapter()
        {
            _boalaAdapter.setBoli(Storage.GetInstance().GetListOfDiseasesFromFile(Activity));
            _boalaAdapter.NotifyDataSetChanged();
        }


        private void setupRecycleView(View view)
        {
            RecyclerView rvBoli = view.FindViewById<RecyclerView>(Resource.Id.rv_notes);
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvBoli.SetLayoutManager(layoutManager);
            _boalaAdapter = new DiseaseAdapter();
            _boalaAdapter.SetListenerBoala(this);
            rvBoli.SetAdapter(_boalaAdapter);
        }
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_add_disease:
                    var intent = new Intent(Activity, typeof(DiseaseActivity));
                    StartActivity(intent);
                    break;
            }
        }


        public void OnBoalaClick(Disease boala)
        {
            Intent intent = new Intent(Application.Context, typeof(DiseaseActivity));
            intent.PutExtra(IdBoala, boala.Id);
            StartActivity(intent);
        }

        public void OnBoalaDelete(Disease boala)
        {
            CustomDialogDeleteDisease cddb = new CustomDialogDeleteDisease(Activity);
            cddb.SetListener(this);
            cddb.SetBoala(boala);
            cddb.Show();
            cddb.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }


        public void OnYesClicked(string result, Disease boala)
        {
            if (result.Equals("yes"))
            {
                Storage.GetInstance().removeBoala(Activity, boala);
                List<Medicine> meds = boala.ListOfMedicines;
                CancelAlarms(meds);

                _boalaAdapter.removeBoala(boala);
                _boalaAdapter.NotifyDataSetChanged();

            }
        }

        private void CancelAlarms(List<Medicine> meds)
        {
            foreach (var med in meds)
            {
                if (med.Alarms != null)
                {
                    foreach (var alarm in med.Alarms)
                    {
                        AlarmManager am = (AlarmManager) Context.GetSystemService(Context.AlarmService);

                        Intent i = new Intent(Activity, typeof(AlarmBroadcastReceiver));
                        PendingIntent pi =
                            PendingIntent.GetActivity(Context, alarm, i, PendingIntentFlags.UpdateCurrent);
                        am.Cancel(pi);
                    }
                }
            }
        }
    }
}