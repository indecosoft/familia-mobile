using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using FamiliaXamarin.Services;
using Java.Text;
using Org.Json;
using SQLite;

namespace Familia.Medicatie
{
    class MedicineLostFragment : Android.Support.V4.App.Fragment, IOnMedLostListener
    {
        private MedicineLostAdapter _medicineLostAdapter;
        private List<MedicationSchedule> _medicationsLost;
        private SQLiteAsyncConnection _db;
        private Intent _medicationLostIntent;
        private CardView cwEmpty;
        private int countReq;


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Error("CREATE VIEW", "MEDICINE LOST FRAGMENT");
            View view = inflater.Inflate(Resource.Layout.fragment_medicine_lost, container, false);
            setupRecycleView(view);
            GetData();
            return view;
        }

        private async void GetData()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            dialog.Show();
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/0", Utils.GetDefaults("Token"));
                    if (res != null)
                    {
                        Log.Error("RESULT_FOR_MEDICATIE fragment", res);
                        if (res.Equals("[]")) return;
                        _medicationsLost = ParseResultFromUrl(res);
                        _medicineLostAdapter.NotifyDataSetChanged();
                        dialog.Dismiss();
                    }
                    else
                    {
                        _medicationsLost = await Storage.GetInstance().readMedSer();


                        Activity.RunOnUiThread(() =>
                        {
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
            _medicineLostAdapter.setMedsList(_medicationsLost);
            _medicineLostAdapter.NotifyDataSetChanged();
            cwEmpty.Visibility = _medicineLostAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;

            bool saved = await Storage.GetInstance().saveMedSer(_medicationsLost);
            if (saved)
            {
                try
                {
                    _medicineLostAdapter.NotifyDataSetChanged();
                    dialog.Dismiss();
                }
                catch (Exception ex)
                {
                    Log.Error("ERRRRRRRR", ex.Message);
                }
            }
        }


        public void OnMedLostClick(MedicationSchedule med)
        {
            Log.Error("MED", med.ToString());
            var alert = new Android.Support.V7.App.AlertDialog.Builder(Activity);
            var medDate = Convert.ToDateTime(med.Timestampstring);
            var currentDate = DateTime.Now;

            if (medDate < currentDate)
            {
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
                        _medicationsLost.Remove(med);
                        _medicineLostAdapter.removeItem(med);
                        _medicineLostAdapter.NotifyDataSetChanged();
                    }
                    cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
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


        private void setupRecycleView(View view)
        {
            _medicationsLost = new List<MedicationSchedule>();
            RecyclerView rvMedLost = view.FindViewById<RecyclerView>(Resource.Id.rv_medlost);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvMedLost.SetLayoutManager(layoutManager);
            _medicineLostAdapter = new MedicineLostAdapter(_medicationsLost);
            rvMedLost.SetAdapter(_medicineLostAdapter);
            _medicineLostAdapter.SetListener(this);
            _medicineLostAdapter.NotifyDataSetChanged();
            countReq = 0;

            if (rvMedLost != null)
            {
                rvMedLost.HasFixedSize = true;
                var onScrollListener = new MedicineServerRecyclerViewOnScrollListener(layoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    countReq++;
                    if (countReq == 1)
                    {

                        var newItems = await GetMoreData(_medicationsLost.Count);
                        if (newItems.Count != 0)
                        {
                            Log.Error("newItems COUNT", newItems.Count + "");
                            try
                            {
                                for (var ms = 0; ms <= newItems.Count; ms++)
                                {
                                    Log.Error("MSSSSSTRING", newItems[ms].Timestampstring);
                                    var date = parseTimestampStringToDate(newItems[ms]);
                                    newItems[ms].Timestampstring = date.ToString();
                                    _medicineLostAdapter.AddItem(newItems[ms]);
                                }
                                await Storage.GetInstance().saveMedSer(_medicineLostAdapter.getList());
                                Log.Error("MEDICATION COUNT lost new Items", newItems.Count + "");
                                Log.Error("MEDICATION COUNT lost after get more data", _medicationsLost.Count + "");
                            }
                            catch (Exception ex)
                            {
                                Log.Error("ERRRRR", ex.Message);
                            }
                        }
                    }
                };
                rvMedLost.AddOnScrollListener(onScrollListener);
                rvMedLost.SetLayoutManager(layoutManager);
            }
        }



        #region on pause & on resume

        public async override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE LOST FRAGMENT", "on pause called");
//            if (_medicationsLost.Count != 0) return;
//            _medicationsLost = await Storage.GetInstance().readMedSer();
//            //            _medicineServerAdapter.setMedsList(_medicationsLost);
//            _medicineLostAdapter.NotifyDataSetChanged();
//            cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        public override void OnResume()
        {
            base.OnResume();
            Log.Error("MEDICINE lost FRAGMENT", "on resume called");
            Log.Error("Medication lost Fragment", " " + _medicationsLost.Count);
        }

        #endregion

        private async Task<List<MedicationSchedule>> GetMoreData(int size)
        {
            var list = new List<MedicationSchedule>();
            Log.Error("MEDICATION COUNT lost", _medicationsLost.Count + " size" + size);

            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token"));//this should be here

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
            return list;
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

        #region send data to medication service




        public async Task<bool> SendMedicationTask(JSONArray mArray, MedicationSchedule med, DateTime now)
        {
            bool isOk = false;
            await Task.Run(async () =>
            {
//                if (await SendData(Context, mArray))
//                {
//                    isOk = true;
//                }
//                else
//                {
                    AddMedicine(_db, med.Uuid, now);
                    Storage.GetInstance().removeMedSer(med.Uuid);
                    Log.Error("SERVICE", "Medication lost service started");
                    _medicationLostIntent =
                        new Intent(Context, typeof(MedicationService));
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        Context.StartForegroundService(_medicationLostIntent);
                    }
                    else
                    {
                        Context.StartService(_medicationLostIntent);
                    }
                    isOk = false;
//                }
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

        private static async void AddMedicine(SQLiteAsyncConnection db, string uuid, DateTime now)
        {
            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        #endregion

    }
}