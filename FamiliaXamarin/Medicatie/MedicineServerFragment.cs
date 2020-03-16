using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using Familia.Helpers;
using Familia.Medicatie.Data;
using Familia.Medicatie.Entities;
using Familia.Services;
using Java.Text;
using Org.Json;
using SQLite;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Fragment = Android.Support.V4.App.Fragment;
using TimeZone = Java.Util.TimeZone;

namespace Familia.Medicatie
{
    public class MedicineServerFragment : Fragment, IOnMedSerListener
    {

        private MedicineServerAdapter _medicineServerAdapter;
        private List<MedicationSchedule> _medications;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;
        private CardView cwEmpty;
        private int countReq;
        private NetworkingData networking =  new NetworkingData();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Error("CREATE VIEW", "MEDICINE SERVER FRAGMENT");
            View view = inflater.Inflate(Resource.Layout.fragment_medicine_server, container, false);
            setupRecycleView(view);
            GetData();
            return view;
        }

        public void OnMedSerClick(MedicationSchedule med)
        {
            var alert = new AlertDialog.Builder(Activity);
            var medDate = Convert.ToDateTime(med.Timestampstring);
            DateTime currentDate = DateTime.Now;

            if (medDate < currentDate)
            {
                alert.SetMessage("Pentru afectiunea " + med.Title + ", medicamentul " + med.Content + " se va marca administrat.");
                alert.SetPositiveButton("Da", async (senderAlert, args) =>
                {
                    DateTime now = DateTime.Now;
                    JSONArray mArray = new JSONArray().Put(new JSONObject().Put("uuid", med.Uuid)
                        .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));

                    if (await SendMedicationTask(mArray, med, now))
                    {
                        Toast.MakeText(Context, "Medicament administrat.", ToastLength.Long).Show();
                        _medications.Remove(med);
                        _medicineServerAdapter.removeItem(med);
                        _medicineServerAdapter.NotifyDataSetChanged();
                    }
                    cwEmpty.Visibility = _medications.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
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
            var rvMedSer = view.FindViewById<RecyclerView>(Resource.Id.rv_medser);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;
            var layoutManager = new LinearLayoutManager(Activity);
            rvMedSer.SetLayoutManager(layoutManager);

            _medications = new List<MedicationSchedule>();
            _medicineServerAdapter = new MedicineServerAdapter(_medications);
            rvMedSer.SetAdapter(_medicineServerAdapter);
            _medicineServerAdapter.SetListener(this);

            countReq = 0;
            if (rvMedSer != null)
            {
                try {
                    rvMedSer.HasFixedSize = true;
                    var onScrollListener = new MedicineServerRecyclerViewOnScrollListener(layoutManager);
                    onScrollListener.LoadMoreEvent += async (sender, e) =>
                    {
                        countReq++;
                        Log.Error("MEDICATION SERVER", _medications.Count + "");
                        if (countReq == 1)
                        {
                            if (_medicineServerAdapter.getList().Count <= 7) return;
                            var newItems = await GetMoreData(_medicineServerAdapter.getList().Count);
                            if (newItems.Count != 0)
                            {
                                Log.Error("MEDICATION SERVER", "new items: " + newItems.Count);
                                try
                                {
                                    for (var ms = 0; ms <= newItems.Count; ms++)
                                    {
                                        DateTime date = parseTimestampStringToDate(newItems[ms]);
                                        newItems[ms].Timestampstring = date.ToString();
                                        _medicineServerAdapter.AddItem(newItems[ms]);
                                    }
                                    _medicineServerAdapter.NotifyDataSetChanged();
                                    Log.Error("MEDICINE SERVER", "new items : " + newItems.Count + " list count: " + _medications.Count);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("ERRRRR MEDICINE SERVER", ex.Message);
                                }
                            }
                        }
                    };
                    rvMedSer.AddOnScrollListener(onScrollListener);
                    rvMedSer.SetLayoutManager(layoutManager);
                }
                catch (Exception e) {
                    Log.Error("ERRRRR MEDICINE SERVER scroll listener", e.Message);
                }
            }
        }

        #region life cycle
        public override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE SERVER", "on pause called, count: " + _medications.Count);
        }

        public override void OnResume()
        {
            base.OnResume();
            Log.Error("MEDICINE SERVER", "on resume called, count: " + _medications.Count);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("MEDICINE SERVER", "on destroy called, count: " + _medications.Count);

        }

        #endregion

        private async void GetData()
        {
            var dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            try { 
            
            dialog.Show();
            Log.Error("MSF NetworkingData", "task getting data..");
            var dataMedicationSchedules = await networking.ReadFutureDataTask(0);
            Log.Error("MSF NetworkingData", "task data received");

                if (dataMedicationSchedules == null)
                {
                    Log.Error("MSF FUTURE", "list is null");
                }
                else {
                    Log.Error("MSF FUTURE", "list is NOT null");
                }

           
                Activity.RunOnUiThread(() =>
            {
                Log.Error("MSF NetworkingData", "uiThread");
                if (dataMedicationSchedules != null && dataMedicationSchedules.Count != 0)
                {
                    _medications.Clear();
                    _medications = new List<MedicationSchedule>(dataMedicationSchedules);
                    _medicineServerAdapter.setMedsList(_medications);
                    _medicineServerAdapter.NotifyDataSetChanged();
                    cwEmpty.Visibility = _medicineServerAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                }
                else
                {
                    cwEmpty.Visibility = _medicineServerAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                }
                dialog.Dismiss();
            });

            }


            catch (Exception e)
            {

                Log.Error("MedicationServerFragment", "FUTURE ERR " + e.Message);
                dialog.Dismiss();
            }
        }
        private async Task<List<MedicationSchedule>> GetMoreData(int size)
        {
            return new List<MedicationSchedule>(await networking.ReadFutureDataTask(size));
        }

        #region parse data
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
            }
            catch (ParseException e)
            {
                e.PrintStackTrace();
            }
            return date.ToLocalTime();
        }
        #endregion
        #region send data to medication service
        public async Task<bool> SendMedicationTask(JSONArray mArray, MedicationSchedule med, DateTime now)
        {
            var isOk = false;
            await Task.Run(async () =>
            {
                if (await SendData(Context, mArray))
                {
                    isOk = true;
                }
                else
                {
                    AddMedicine(_db, med.Uuid, now);
                    Storage.GetInstance().removeMedSer(med.Uuid);
                    Log.Error("MEDICATION SERVER", "Medication service started");
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
            string result = await WebServices.WebServices.Post(
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
            await db.InsertAsync(new MedicineRecords {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        #endregion
    }
}