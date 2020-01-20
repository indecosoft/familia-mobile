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

using System.IO;
using Familia.Medicatie.Data;


namespace Familia.Medicatie
{
    class MedicineLostFragment : Android.Support.V4.App.Fragment, IOnMedLostListener
    {
        private MedicineLostAdapter _medicineLostAdapter;
        private List<MedicationSchedule> _medicationsLost;
        private SQLiteAsyncConnection _db;
        private Intent _medicationLostIntent;
        private CardView cwEmpty;
        private RecyclerView rvMedLost;
        private LinearLayoutManager layoutManager;
        private int countReq;

        private NetworkingData networking = new NetworkingData();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Error("CREATE VIEW", "MEDICINE LOST FRAGMENT");
            View view = inflater.Inflate(Resource.Layout.fragment_medicine_lost, container, false);
            setupRecycleView(view);
            switch (int.Parse(Utils.GetDefaults("UserType")))
            {
                case 3:
                    LoadType3();
                    break;
                case 4:
                    LoadType4();
                    break;
                default:
                    Log.Error("MEDICINE LOST", "wrong type");
                    break;
            }
            return view;
        }

        private void LoadType3()
        {
            SetScrollListener();

            GetData();
        }

        private void SetScrollListener()
        {
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
                        if (_medicineLostAdapter.getList().Count <= 7) return;

                        var arr = _medicineLostAdapter.getList().Where(c => !(c.Uuid.Contains("disease"))).ToList();

                        var newItems = await GetMoreData(arr.Count);
                        if (newItems.Count != 0)
                        {
                            Log.Error("MEDICINE LOST", "new items: " + newItems.Count);
                            try
                            {
                                for (var ms = 0; ms <= newItems.Count; ms++)
                                {
                                    var date = parseTimestampStringToDate(newItems[ms]);
                                    newItems[ms].Timestampstring = date.ToString();
                                    _medicineLostAdapter.AddItem(newItems[ms]);
                                }
                                _medicineLostAdapter.setMedsList(_medicineLostAdapter.getList().OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList());
                                _medicineLostAdapter.NotifyDataSetChanged();
                                Log.Error("MEDICINE LOST", "new items : " + newItems.Count + " list count: " + _medicationsLost.Count);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("ERRRRR MEDICINE LOST", ex.Message);
                            }
                        }
                    }
                };
                rvMedLost.AddOnScrollListener(onScrollListener);
                rvMedLost.SetLayoutManager(layoutManager);
            }
        }

        private async void LoadType4()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            dialog.Show();
            var listWithAllElements = new List<MedicationSchedule>();
            if (Storage.GetInstance().GetListOfDiseasesFromFile(Context) != null)
            {
                listWithAllElements = await Storage.GetInstance().GetPersonalMedicationConverted();
                listWithAllElements = listWithAllElements.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
            }
            else
            {
                Log.Error("MEDICINE LOST", "no of items in storage: 0 ");
            }

            Activity.RunOnUiThread(() =>
            {
                Log.Error("Type 4 lost", "uiThread");
                _medicationsLost.Clear();
                _medicationsLost = new List<MedicationSchedule>(listWithAllElements);
                _medicineLostAdapter.setMedsList(_medicationsLost);
                _medicineLostAdapter.NotifyDataSetChanged();
                cwEmpty.Visibility = _medicineLostAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                dialog.Dismiss();
            });
        }

        public void OnMedLostClick(MedicationSchedule med)
        {
            Log.Error("MEDICINE LOST", "med clicked: " + med.ToString());
            var alert = new Android.Support.V7.App.AlertDialog.Builder(Activity);
            var medDate = Convert.ToDateTime(med.Timestampstring);
            var currentDate = DateTime.Now;

            if (medDate < currentDate)
            {
                alert.SetMessage("Pentru afectiunea " + med.Title + ", medicamentul " + med.Content + " se va marca administrat.");
                alert.SetPositiveButton("Da", async (senderAlert, args) =>
                {
                    Log.Error("MEDICINE LOST", "DA clicked");
                    switch (int.Parse(Utils.GetDefaults("UserType")))
                    {
                        case 3:
                            //LoadType3();
                            bool isSent = false;
                            if (med.Uuid.Contains("disease"))
                            {
                                var isDeleted = await Storage.GetInstance().RemoveItemFromDBTask(med);
                                if (isDeleted)
                                {
                                    Log.Error("MEDICINE LOST STORAGE", "deleted succesfully");
                                    isSent = true;
                                }
                                else
                                {
                                    Log.Error("MEDICINE LOST STORAGE", "something went wrong on delete item");
                                }
                            }
                            else
                            {
                                var now = DateTime.Now;
                                var mArray = new JSONArray().Put(new JSONObject().Put("uuid", med.Uuid)
                                    .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));
                                isSent =  SendMedicationTask(mArray, med, now);
                            }

                            if (isSent)
                            {
                                Toast.MakeText(Context, "Medicament administrat.", ToastLength.Long).Show();
                                _medicineLostAdapter.removeItem(med);
                                _medicineLostAdapter.NotifyDataSetChanged();
                            }
                            cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                            break;
                        case 4:
                            // LoadType4();
                            bool isMarked = false;
                            var isAdministrated = await Storage.GetInstance().RemoveItemFromDBTask(med);
                            if (isAdministrated)
                            {
                                Log.Error("MEDICINE LOST STORAGE", "deleted succesfully");
                                isMarked = true;
                            }
                            else
                            {
                                Log.Error("MEDICINE LOST STORAGE", "something went wrong on delete item");
                            }

                            if (isMarked)
                            {
                                Toast.MakeText(Context, "Medicament administrat.", ToastLength.Long).Show();
                                _medicineLostAdapter.removeItem(med);
                                _medicineLostAdapter.NotifyDataSetChanged();
                            }
                            cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;

                            break;
                        default:
                            Log.Error("MEDICINE LOST", "wrong type");
                            break;
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

        private void setupRecycleView(View view)
        {
            rvMedLost = view.FindViewById<RecyclerView>(Resource.Id.rv_medlost);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;
            layoutManager = new LinearLayoutManager(Activity);
            rvMedLost.SetLayoutManager(layoutManager);

            _medicationsLost = new List<MedicationSchedule>();
            _medicineLostAdapter = new MedicineLostAdapter(_medicationsLost);
            rvMedLost.SetAdapter(_medicineLostAdapter);
            _medicineLostAdapter.SetListener(this);
            _medicineLostAdapter.NotifyDataSetChanged();
        }

        #region life cycle

        public override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE LOST LIFE CYCLE", "on pause called , count: " + _medicationsLost.Count);
//            switch (int.Parse(Utils.GetDefaults("UserType")))
//            {
//                case 3:
//                    LoadType3();
//                    break;
//                case 4:
//                    LoadType4();
//                    break;
//                default:
//                    Log.Error("MEDICINE LOST", "wrong type");
//                    break;
//            }
        }

        public override void OnResume()
        {
            base.OnResume();
            //            switch (int.Parse(Utils.GetDefaults("UserType")))
            //            {
            //                case 3:
            //                    LoadType3();
            //                    break;
            //                case 4:
            //                    LoadType4();
            //                    break;
            //                default:
            //                    Log.Error("MEDICINE LOST", "wrong type");
            //                    break;
            //            }
            Log.Error("MEDICINE LOST LIFE CYCLE", "on resume called, count: " + _medicationsLost.Count + " adapter count: " + _medicineLostAdapter.ItemCount);

            _medicineLostAdapter.NotifyDataSetChanged();

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("MEDICINE LOST LIFE CYCLE", "on destroy called , count: " + _medicationsLost.Count);
        }


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {

            Log.Error("MEDICINE LOST LIFE CYCLE", "on  OnViewCreated called , count: " + _medicationsLost.Count);

            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnViewStateRestored(Bundle savedInstanceState)
        {

            Log.Error("MEDICINE LOST LIFE CYCLE", "on  OnViewStateRestored called , count: " + _medicationsLost.Count);

            base.OnViewStateRestored(savedInstanceState);
        }

        public override void OnDestroyView()
        {
            Log.Error("MEDICINE LOST LIFE CYCLE", "on destroy view called , count: " + _medicationsLost.Count);

            base.OnDestroyView();
        }

        #endregion
        private async void GetData()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
            dialog.Show();
            Log.Error("NetworkingData lost", "task getting data..");
            var dataMedicationSchedules = await networking.ReadPastDataTask(0);
            Log.Error("NetworkingData lost", "task data received");

            //------------------------------------------------med pers
            var listWithAllElements = new List<MedicationSchedule>();
            if (Storage.GetInstance().GetListOfDiseasesFromFile(Context) != null)
            {
                listWithAllElements = await Storage.GetInstance().GetPersonalMedicationConverted();
                foreach (var item in dataMedicationSchedules)
                {
                    listWithAllElements.Add(item);
                }
                listWithAllElements = listWithAllElements.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
            }
            else
            {
                Log.Error("MEDICINE LOST", "no of items in storage: 0 ");
            }
            //----------------------------------------------------------------

            Activity.RunOnUiThread(() =>
            {
                Log.Error("NetworkingData lost", "uiThread");
                if (dataMedicationSchedules != null && dataMedicationSchedules.Count != 0)
                {
                    _medicationsLost.Clear();
                    _medicationsLost = new List<MedicationSchedule>(listWithAllElements);
                    _medicineLostAdapter.setMedsList(_medicationsLost);
                    _medicineLostAdapter.NotifyDataSetChanged();
                    cwEmpty.Visibility = _medicineLostAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                }
                else
                {
                    cwEmpty.Visibility = _medicineLostAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                }
                dialog.Dismiss();
            });
        }

        private static List<MedicationSchedule> ConvertPersonalMedicationListToMedicationSchedules(List<Disease> LD)
        {
            var listMedSchPersonal = new List<MedicationSchedule>();

            foreach (var item in LD)
            {
                foreach (var itemMed in item.ListOfMedicines)
                {
                    foreach (var itemHour in itemMed.Hours)
                    {
                        if (itemHour.HourName.Equals("24:00"))
                        {
                            itemHour.HourName = "23:59";
                        }
                        var tspan = TimeSpan.Parse(itemHour.HourName);
                        Log.Error("TIME SPAN: ", tspan.ToString());
                        var dtMed = new DateTime(itemMed.Date.Year, itemMed.Date.Month, itemMed.Date.Day, tspan.Hours, tspan.Minutes, tspan.Seconds);
                        Log.Error("MEDICINE LOST", "item med: " + dtMed);

                        var currentDate = DateTime.Now;
                        if (dtMed < currentDate)
                        {
                            TimeSpan difference = DateTime.Now.Subtract(dtMed);
                            var days = (int)difference.TotalDays + 1;
                            Log.Error("MEDICINE LOST DAYS", "days betweet 2 dates: " + days);
                            if (itemMed.NumberOfDays != 0)
                            {
                                days = days >= itemMed.NumberOfDays
                                    ? itemMed.NumberOfDays
                                    : itemMed.NumberOfDays - days;
                            }

                            Log.Error("MEDICINE LOST DAYS", "days: " + days);
                            var objMedSch = new MedicationSchedule("disease" + item.Id + "med" + itemMed.IdMed + "hour" + itemHour.Id, dtMed.ToString(), item.DiseaseName, itemMed.Name, 5, 0);
                            listMedSchPersonal.Add(objMedSch);
                            for (int j = 1; j < days; j++)
                            {
                                dtMed = dtMed.AddDays(1);
                                objMedSch = new MedicationSchedule("disease" + item.Id + "med" + itemMed.IdMed + "hour" + itemHour.Id, dtMed.ToString(), item.DiseaseName, itemMed.Name, 5, 0);
                                listMedSchPersonal.Add(objMedSch);
                            }
                        }
                    }
                }
            }
            return listMedSchPersonal.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
        }

        private async Task<List<MedicationSchedule>> GetMoreData(int size)
        {
            return new List<MedicationSchedule>(await networking.ReadPastDataTask(size));
        }

        #region parse data
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

                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone, 0));

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
        #endregion
        #region send data to medication service

        public bool SendMedicationTask(JSONArray mArray, MedicationSchedule med, DateTime now)
        {
            AddMedicine(_db, med.Uuid, now);
            Log.Error("MEDICINE LOST", "Medication service started");
            _medicationLostIntent = new Intent(Context, typeof(MedicationService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Context.StartForegroundService(_medicationLostIntent);
            }
            else
            {
                Context.StartService(_medicationLostIntent);
            }
            Storage.GetInstance().removeMedSer(med.Uuid);
            return true;
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
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var nameDb = "devices_data.db";
            db = new SQLiteAsyncConnection(Path.Combine(path, nameDb));
            await db.CreateTableAsync<MedicineRecords>();
            await db.InsertAsync(new MedicineRecords()
            {
                Uuid = uuid,
                DateTime = now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        #endregion
    }
}