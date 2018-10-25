using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Text;
using Java.Util;
using Org.Json;

namespace FamiliaXamarin.Medicatie
{
    public class MedicineFragment : Android.Support.V4.App.Fragment ,View.IOnClickListener, IOnBoalaClickListener, CustomDialogDeleteBoala.ICustomDialogDeleteBoalaListener
    {

#pragma warning disable 618
        private ProgressDialog _progressDialog;
#pragma warning restore 618

        public static string IdBoala = "id_boala";
        private DiseaseAdapter _boalaAdapter;
        private List<MedicationSchedule> medications;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_medicine, container, false);
            view.FindViewById(Resource.Id.btn_add_disease).SetOnClickListener(this);
            setupRecycleView(view);

#pragma warning disable 618
            _progressDialog = new ProgressDialog(Activity);
#pragma warning restore 618
            _progressDialog.SetTitle("Va rugam asteptati ...");
            _progressDialog.SetMessage("Preluare medicatie");
            _progressDialog.SetCancelable(false);

            
            _progressDialog.Show();
            GetData();


            return view;
        }

        private async void GetData()
        {
            //IWebServices webservices = new WebServices();
            await Task.Run(async () => {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("IdClient", Activity)}", Utils.GetDefaults("Token", Activity));
                    // var res = await webservices.Get($"{Constants.PublicServerAddress}/api/userMeds/15", Utils.GetDefaults("Token", Activity));
                    Log.Error("Result", "*******************************************************");
                    if (res != null)
                    {
                        if (res.Equals("[]")) return;
                        medications = ParseResultFromUrl(res);
                        //TODO setAlarm for each item of medications and parse the timestampString to a real timestamp
                        foreach (var ms in medications)
                        {
                            var am = (AlarmManager)Activity.GetSystemService(Context.AlarmService);
                            var i = new Intent(Activity, typeof(AlarmBroadcastReceiverServer));


                            i.PutExtra(AlarmBroadcastReceiverServer.UUID, ms.Uuid);
                            i.PutExtra(AlarmBroadcastReceiverServer.TITLE, ms.Title);
                            i.PutExtra(AlarmBroadcastReceiverServer.CONTENT, ms.Content);
                            i.SetAction(AlarmBroadcastReceiverServer.ACTION_RECEIVE);

                            var id = CurrentTimeMillis();
                            var pi = PendingIntent.GetBroadcast(Activity, id, i, PendingIntentFlags.OneShot);
                            if (am != null)
                            {
                                var date = parseTimestampStringToDate(ms);

                                Calendar calendar = Calendar.Instance;
                                Calendar setcalendar = Calendar.Instance;

                                setcalendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
                                //setcalendar.Time = date.;
                                Log.Error("Calendarul", setcalendar.ToString());

                                if (setcalendar.Before(calendar)) return;

                                am.SetInexactRepeating(AlarmType.RtcWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
                            }
                        }
                    }
                    else
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            Toast.MakeText(Activity, "Nu se poate conecta la server", ToastLength.Short).Show();

                        });
                    }
                }
                catch (Exception e)
                {
                    Log.Error("AlarmError", e.Message);
                }
               
               
            });
            _progressDialog.Dismiss();

            
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

        private readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public int CurrentTimeMillis()
        {
            return (int)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
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
                    Activity.StartActivity(typeof(DiseaseActivity));
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
            CustomDialogDeleteBoala cddb = new CustomDialogDeleteBoala(Activity);
            cddb.SetListener(this);
            cddb.SetBoala(boala);
            cddb.Show();
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
//        public override void OnMedicationScheduleLoaded(List<MedicationSchedule> msList)
//        {
//            foreach (var ms in  msList)
//            {
//
//                //TODO parseaza din timestamp-ul de pe ms ca sa iei ora si data pt setarea alarmei
// 
//
//            }
//        }

        
    }
}