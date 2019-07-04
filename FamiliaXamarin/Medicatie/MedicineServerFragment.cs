using System;
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
using Familia;
using Familia.DataModels;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;

namespace Familia.Medicatie
{
    public class MedicineServerFragment : Android.Support.V4.App.Fragment
    {

        private MedicineServerAdapter _medicineServerAdapter;
        private List<MedicationSchedule> _medications;
        private SQLiteConnection _db;
       

        private void setupRecycleView(View view)
        {   _medications = new List<MedicationSchedule>();
            RecyclerView rvMedSer = view.FindViewById<RecyclerView>(Resource.Id.rv_medser);
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvMedSer.SetLayoutManager(layoutManager);
            _medicineServerAdapter = new MedicineServerAdapter();
            rvMedSer.SetAdapter(_medicineServerAdapter);
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
    }
}