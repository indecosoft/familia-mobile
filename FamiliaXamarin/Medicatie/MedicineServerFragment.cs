using System;
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
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;
using Exception = System.Exception;

namespace Familia.Medicatie
{
    public class MedicineServerFragment : Android.Support.V4.App.Fragment, IOnMedSerListener
    {

        private MedicineServerAdapter _medicineServerAdapter;
        private List<MedicationSchedule> _medications;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;
        private CardView cwEmpty;
        private int countReq;


        private void setupRecycleView(View view)
        {
            _medications = new List<MedicationSchedule>();
            RecyclerView rvMedSer = view.FindViewById<RecyclerView>(Resource.Id.rv_medser);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvMedSer.SetLayoutManager(layoutManager);
            _medicineServerAdapter = new MedicineServerAdapter(_medications);
            rvMedSer.SetAdapter(_medicineServerAdapter);
            _medicineServerAdapter.SetListener(this);
//            _medicineServerAdapter.setMedsList(_medications);
            _medicineServerAdapter.NotifyDataSetChanged();
            countReq = 0;

            if (rvMedSer != null)
            {
                rvMedSer.HasFixedSize = true;
                var onScrollListener = new MedicineServerRecyclerViewOnScrollListener(layoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    countReq++;
                    Log.Error("Request Count", countReq + "");
                    Log.Error("MEDICATION COUNT", _medications.Count + "");
                    if (countReq == 1)
                    {

                    var newItems = await GetMoreData(_medications.Count);
                        if (newItems.Count != 0 )
                        {
                            Log.Error("newItems COUNT", newItems.Count + "");
                            try
                            {
                                for (var ms = 0; ms <= newItems.Count; ms++)
                                {
                                    Log.Error("MSSSSSTRING", newItems[ms].Timestampstring);
                                    var date = parseTimestampStringToDate(newItems[ms]);
                                    newItems[ms].Timestampstring = date.ToString();
                                    Log.Error("MEDICATION COUNT IN FOR", _medications.Count + "");
                                    _medicineServerAdapter.AddItem(newItems[ms]);

                                    Log.Error("MEDICATION COUNT adapter in FOR", _medicineServerAdapter.getList().Count + "");

                                    //_medications.Add(newItems[ms]);
                                }
//                                _medicineServerAdapter.NotifyDataSetChanged();
                                Storage.GetInstance().saveMedSer(_medicineServerAdapter.getList());

                                Log.Error("MEDICATION COUNT new Items", newItems.Count + "");
                                Log.Error("MEDICATION COUNT after get more data", _medications.Count + "");
                                Log.Error("MEDICATION COUNT adapter", _medicineServerAdapter.ItemCount + "");
                            }
                            catch (Exception ex)
                            {
                                Log.Error("ERRRRR", ex.Message);
                            }
                        }
                    }
                };
                rvMedSer.AddOnScrollListener(onScrollListener);
                rvMedSer.SetLayoutManager(layoutManager);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Error("CREATE VIEW", "MEDICINE SERVER FRAGMENT");
            View view = inflater.Inflate(Resource.Layout.fragment_medicine_server, container, false);
            setupRecycleView(view);

            GetData();
            return view;
        }

        private async Task<List<MedicationSchedule>> GetMoreData(int size)
        {
            var list = new List<MedicationSchedule>();
            Log.Error("MEDICATION COUNT", _medications.Count + " size" + size);
            Log.Error("Request Count task", countReq + "");

            await Task.Run(async () =>
            {
                try
                {
                    Log.Error("MEDICATION COUNT task", _medications.Count + " size" + size);
//                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token"));
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/medicineList/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token")); //this should be here

                    if (!string.IsNullOrEmpty(res))
                    {
                        countReq = 0;
                        Log.Error("RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;
                        list = ParseResultFromUrl(res);
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
            Log.Error("AFTER AWAIT", list.Count + "");
//            _medicineServerAdapter.NotifyDataSetChanged();

            return list;
        }

        private async void GetData()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            dialog.Show();
            await Task.Run(async () =>
            {
                try
                {
                    //var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/userMeds/{Utils.GetDefaults("IdClient")}", Utils.GetDefaults("Token"));
//                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/0", Utils.GetDefaults("Token"));
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/medicineList/{Utils.GetDefaults("IdClient")}/0", Utils.GetDefaults("Token")); //this should be here
                    if (res != null)
                    {
                        Log.Error("RESULT_FOR_MEDICATIE fragment", res);
                        if (res.Equals("[]")) return;
                        _medications = ParseResultFromUrl(res);
                        Log.Error("COUNT MEDICATIE fragment", _medications.Count + "");
//                        _medicineServerAdapter.setMedsList(_medications);
                        _medicineServerAdapter.NotifyDataSetChanged();
                        dialog.Dismiss();
                    }
                    else
                    {
                        _medications = await Storage.GetInstance().readMedSer();
                       

                        Activity.RunOnUiThread(() =>
                        {
                            Log.Error("RESULT_FOR_MEDICATIE fragment", "nu se poate conecta la server");
                            Toast.MakeText(Activity, "Nu se poate conecta la server", ToastLength.Short).Show();
                            dialog.Dismiss();

                        });
                    }
                }
                catch (Exception e)
                {
                    Log.Error("AlarmError", e.Message);
                }
            });

            Log.Error("DUPA CE IESE DIN TOATE 1", "" + _medications.Count);
            _medicineServerAdapter.setMedsList(_medications);

            Log.Error("MEDICINE SERFER FRAGMENT", _medicineServerAdapter.getList().Count + "");
            _medicineServerAdapter.NotifyDataSetChanged();

            cwEmpty.Visibility = _medicineServerAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;

            bool saved = await Storage.GetInstance().saveMedSer(_medications);
            if (saved)
            {
                Log.Error("FRAGMENT MEDICATIE SERVER", "SAVED");
                Log.Error("MEDICINE SERFER FRAGMENT", _medicineServerAdapter.getList().Count + "");
                try
                {
                    _medicineServerAdapter.NotifyDataSetChanged();
                    dialog.Dismiss();
                }
                catch (Exception ex)
                {
                    Log.Error("ERRRRRRRR", ex.Message);
                }
            }
        }

        public async override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE server FRAGMENT", "on pause called");
//            if (_medications.Count != 0) return;
//            _medications = await Storage.GetInstance().readMedSer();
////            _medicineServerAdapter.setMedsList(_medications);
//            _medicineServerAdapter.NotifyDataSetChanged();
//            cwEmpty.Visibility = _medications.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        public override void OnResume()
        {
            base.OnResume();
            Log.Error("MEDICINE server FRAGMENT", "on resume called");
            Log.Error("MEDICINE server FRAGMENT", " " + _medications.Count);
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
                //                alert.SetTitle("Pentru afectiunea " + med.Title + ", medicamentul " + med.Content + " se va marca administrat.");
                alert.SetMessage("Pentru afectiunea " + med.Title + ", medicamentul " + med.Content + " se va marca administrat.");
                alert.SetPositiveButton("Da", async (senderAlert, args) =>
                {

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
                    if (_medications.Count == 0)
                    {
                        cwEmpty.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        cwEmpty.Visibility = ViewStates.Gone;
                    }

                });

                alert.SetNegativeButton("Nu", (senderAlert, args) => { });
            }
            else
            {
                alert.SetMessage("Pentru afectiunea " + med.Title + ", medicamentul " + med.Content + " nu se poate marca administrat.");
                alert.SetPositiveButton("Ok", (senderAlert, args) => { });
            }

            Dialog dialog = alert.Create();
            dialog.Show();
        }


        public async Task<bool> SendMedicationTask(JSONArray mArray, MedicationSchedule med, DateTime now)
        {
            bool isOk = false;
            await Task.Run(async () =>
           {
               if (await SendData(Context, mArray))
               {
                    //                    var running = IsServiceRunning(typeof(MedicationService), Context);
                    //                    if (running)
                    //                    {
                    //                        Log.Error("SERVICE", "Medication service is running");
                    //                        Context.StopService(_medicationServiceIntent);
                    //                    }
                    isOk = true;
               }
               else
               {
                   AddMedicine(_db, med.Uuid, now);
                   Storage.GetInstance().removeMedSer(med.Uuid);
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
            if (!Utils.CheckNetworkAvailability()) return false;
            switch (result)
            {
                case "Done":
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