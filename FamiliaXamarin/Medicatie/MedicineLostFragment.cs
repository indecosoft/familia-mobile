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
                            try
                            {
                                if (_medicationsLost.Count <= 7) return;
                                var newItems = await GetMoreData(_medicationsLost.Count);
                                if (newItems.Count != 0)
                                {
                                    Log.Error("MEDICINE LOST", "new items: " + newItems.Count);
                                    try
                                    {
                                        for (var ms = 0; ms <= newItems.Count; ms++)
                                        {
                                            Log.Error("MSSSSSTRING", newItems[ms].Timestampstring);
                                            var date = parseTimestampStringToDate(newItems[ms]);
                                            newItems[ms].Timestampstring = date.ToString();
                                            _medicineLostAdapter.AddItem(newItems[ms]);
                                        }
                                        _medicineLostAdapter.NotifyDataSetChanged();
                                        await Storage.GetInstance().saveMedSer(_medicineLostAdapter.getList());
                                        Log.Error("MEDICINE LOST","new items : " + newItems.Count + " list count: " + _medicationsLost.Count);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("ERRRRR MEDICINE LOST", ex.Message);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(" MEDICINE LOST ERRRRR", ex.Message);
                            }
                        }
                    };
                    rvMedLost.AddOnScrollListener(onScrollListener);
                    rvMedLost.SetLayoutManager(layoutManager);
            }

            GetData();
        }

        private async void LoadType4()
        {
            if (Storage.GetInstance().GetListOfDiseasesFromFile(Context) != null)
            {
                var LD = Storage.GetInstance().GetDiseases();
                Log.Error("MEDICINE LOST STORAGE", "no. of items in storage LD: " + LD.Count);
                var listWithAllElements = ConvertPersonalMedicationListToMedicationSchedules(LD);

                await Storage.GetInstance().readMedSer();
                var storage = Storage.GetInstance().getMSList();

                Log.Error("MEDICINE LOST STORAGE", "no. of items in storage med sch " + storage.Count);

                if (storage.Count != 0)
                {
                    foreach (var storageItem in storage)
                    {   
                        var str = storageItem.Uuid.Split("hour");
                        Log.Error("MEDICINE LOST STORAGE", str[0]);
                        foreach (var itemList in listWithAllElements)
                        {
                            if (itemList.Uuid.Contains(str[0]))
                            {
                                Log.Error("MEDICINE LOST STORAGE", "item exists");
                            }
                            else
                            {
                                Log.Error("MEDICINE LOST STORAGE", "item saved");

                                Storage.GetInstance().saveElementMedSer(itemList);
                            }
                        }
                    }
                }
                else
                {
                    Log.Error("MEDICINE LOST STORAGE", "saved all");

                    await Storage.GetInstance().saveMedSer(listWithAllElements);
                }


                Log.Error("MEDICINE LOST STORAGE", "current storage count" + Storage.GetInstance().getMSList().Count);

                _medicineLostAdapter.setMedsList(Storage.GetInstance().getMSList());
                _medicineLostAdapter.NotifyDataSetChanged();
                Log.Error("MEDICINE LOST", "no. of items in storage adapter: " + _medicineLostAdapter.getList().Count);
                cwEmpty.Visibility = _medicineLostAdapter.getList().Count == 0 ? ViewStates.Visible : ViewStates.Gone;

                foreach (var item in listWithAllElements)
                {
                    Log.Error("MEDICINE LOST personal item", item.ToString());
                }
            }
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
                            var now = DateTime.Now;
                            var mArray = new JSONArray().Put(new JSONObject().Put("uuid", med.Uuid)
                                .Put("date", now.ToString("yyyy-MM-dd HH:mm:ss")));
                            bool isSent = SendMedicationTask(mArray, med, now);
                            if (isSent)
                            {
                                Toast.MakeText(Context, "Medicament administrat.", ToastLength.Long).Show();
                                //                        _medicationsLost.Remove(med);
                                _medicineLostAdapter.removeItem(med);
                                _medicineLostAdapter.NotifyDataSetChanged();
                            }
                            cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
                            break;
                        case 4:
                            // LoadType4();
                            _medicineLostAdapter.removeItem(med);
                            _medicineLostAdapter.NotifyDataSetChanged();
                            Storage.GetInstance().removeMedSer(med.Uuid);


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
            _medicationsLost = new List<MedicationSchedule>();
             rvMedLost = view.FindViewById<RecyclerView>(Resource.Id.rv_medlost);
            cwEmpty = view.FindViewById<CardView>(Resource.Id.cw_empty);
            cwEmpty.Visibility = ViewStates.Gone;

            layoutManager = new LinearLayoutManager(Activity);
            rvMedLost.SetLayoutManager(layoutManager);
            _medicineLostAdapter = new MedicineLostAdapter(_medicationsLost);
            rvMedLost.SetAdapter(_medicineLostAdapter);
            _medicineLostAdapter.SetListener(this);
            _medicineLostAdapter.NotifyDataSetChanged();
            
        }

        #region life cycle

        public async override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE LOST LIFE CYCLE", "on pause called , count: " + _medicationsLost.Count);
            //            if (_medicationsLost.Count != 0) return;
            //            _medicationsLost = await Storage.GetInstance().readMedSer();
            //            //            _medicineServerAdapter.setMedsList(_medicationsLost);
            //            _medicineLostAdapter.NotifyDataSetChanged();
            //            cwEmpty.Visibility = _medicationsLost.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        public override void OnResume()
        {
            base.OnResume();
            _medicineLostAdapter.NotifyDataSetChanged();

            Log.Error("MEDICINE LOST LIFE CYCLE", "on resume called, count: " + _medicationsLost.Count + " adapter count: " + _medicineLostAdapter.ItemCount);

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
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/0", Utils.GetDefaults("Token"));
                    if (res != null)
                    {
                        Log.Error(" MEDICINE LOST RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;
                        _medicationsLost = ParseResultFromUrl(res);
                        Log.Error("MEDICATION SERVER ", " count: " + _medicationsLost.Count);
                        _medicineLostAdapter.NotifyDataSetChanged();
                        dialog.Dismiss();
                    }
                    else
                    {
                        _medicationsLost = await Storage.GetInstance().readMedSer();
                        Log.Error("MEDICATION SERVER ", " count: " + _medicationsLost.Count);
                        Activity.RunOnUiThread(() =>
                        {
                            Toast.MakeText(Activity, "Nu se poate conecta la server", ToastLength.Short).Show();
                            dialog.Dismiss();

                        });
                    }
                }
                catch (Exception e)
                {
                    Log.Error("MEDICINE LOST ERR", e.Message);
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
                    Log.Error("MEDICINE LOST ERR", ex.Message);
                }
            }


            //------------------------------------------------
            if (Storage.GetInstance().GetListOfDiseasesFromFile(Context) != null)
            {
                var LD = Storage.GetInstance().GetDiseases();
                Log.Error("MEDICINE LOST", "no of items in storage LD: " + LD.Count);
                var listWithAllElements = ConvertPersonalMedicationListToMedicationSchedules(LD);

                //adding server's meds
                foreach (var item in _medicineLostAdapter.getList())
                {
                    listWithAllElements.Add(item);
                }
                listWithAllElements = listWithAllElements.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
                foreach (var item in listWithAllElements)
                {
                    Log.Error("item sorted: ", item.ToString());
                }
            }
            else
            {
                Log.Error("MEDICINE LOST", "no of items in storage: 0 ");
            }

            //----------------------------------------------------------------
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
                                if (days >= itemMed.NumberOfDays)
                                {
                                    //add in for numberofDays
                                    days = itemMed.NumberOfDays;
                                }
                                else
                                {
                                    //substract days in plus
                                    days = itemMed.NumberOfDays - days;
                                }
                               
                            }
                            Log.Error("MEDICINE LOST DAYS", "days: " + days);
                            var objMedSch = new MedicationSchedule("disease" + item.Id + "med" + itemMed.IdMed + "hour" + itemHour.Id, dtMed.ToString(), item.DiseaseName, itemMed.Name, 5, 0);
                            listMedSchPersonal.Add(objMedSch);
                            for (int j = 1; j < days; j++)
                            {
                                Log.Error("MEDICINE LOST DAYS", "days j: " + j);

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
            var list = new List<MedicationSchedule>();
            Log.Error("MEDICATION LOST", _medicationsLost.Count + " size " + size);
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token"));//this should be here
                    if (!string.IsNullOrEmpty(res))
                    {
                        countReq = 0;
                        Log.Error("MEDICATION LOST RESULT_FOR_MEDICATIE", res);
                        if (res.Equals("[]")) return;
                        list = ParseResultFromUrl(res);
                    }
                    else
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            Log.Error("MEDICATION LOST RESULT_FOR_MEDICATIE", "nu se poate conecta la server");
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