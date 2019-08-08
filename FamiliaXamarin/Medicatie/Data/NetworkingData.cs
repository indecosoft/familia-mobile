using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Entities;
using Org.Json;

namespace Familia.Medicatie.Data
{
    class NetworkingData
    {
        private static NetworkingData _instance;
        private static readonly object Padlock = new object();
        private List<MedicationSchedule> _medicationSchedules;
        private SqlHelper<MedicineServerRecords> _db;

        public NetworkingData()
        {
            _medicationSchedules = new List<MedicationSchedule>();
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
        }

//        public static NetworkingData GetInstance()
//        {
//            lock (Padlock)
//            {
//                if (_instance == null)
//                {
//                    _instance = new NetworkingData();
//                }
//            }
//
//            return _instance;
//        }

        private async Task<List<MedicationSchedule>> CallServerFutureData(int size)
        {
            var medications = new List<MedicationSchedule>();
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/medicineList/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token")); //this should be here
                    if (res != null)
                    {
                        Log.Error("Networking data", " RESULT_FOR_MEDICATIE" + res);
                        medications = ParseResultFromUrl(res);
                        Log.Error("Networking data", " count: " + medications.Count);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("MEDICATION SERVER ERR", e.Message);
                }
            });
            return medications;
        }

        private async Task<List<MedicationSchedule>> CallServerPastData(int size)
        {
            var medications = new List<MedicationSchedule>();
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/missedMedicine/{Utils.GetDefaults("IdClient")}/{size}", Utils.GetDefaults("Token")); //this should be here
                    if (res != null)
                    {
                        Log.Error("Networking data", " RESULT_FOR_MEDICATIE" + res);
                        medications = ParseResultFromUrl(res);
                        Log.Error("Networking data", " count: " + medications.Count);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("MEDICATION SERVER ERR", e.Message);
                }
            });
            return medications;
        }

        public async Task<List<MedicationSchedule>> ReadFutureDataTask(int size)
        {
            _medicationSchedules = new List<MedicationSchedule>(await CallServerFutureData(size));
            if (_medicationSchedules.Count != 0)
            {
                bool finished = await SaveListInDBTask(_medicationSchedules);
                Log.Error("NetworkingData class", "saved: " + finished);

            }
            else
            {
                if (size == 0)
                {
                    Log.Error("NetworkingData class", "reading from local db..");
                    _medicationSchedules = new List<MedicationSchedule>(await ReadListFromDbFutureDataTask());
                    Log.Error("NetworkingData class", "reading finish");
                }
            }

            Log.Error("NetworkingData class", "count from reading" + _medicationSchedules.Count);
            return ExtractFutureData(_medicationSchedules.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList());
        }

        public async Task<List<MedicationSchedule>> ReadPastDataTask(int size)
        {
            _medicationSchedules = new List<MedicationSchedule>(await CallServerPastData(size));
            if (_medicationSchedules.Count != 0)
            {
                bool finished = await SaveListInDBTask(_medicationSchedules);
                Log.Error("NetworkingData class", "saved: " + finished);
            }
            else
            {
                if (size == 0)
                {
                    Log.Error("NetworkingData class", "reading from local db..");
                    _medicationSchedules = new List<MedicationSchedule>(await ReadListFromDbPastDataTask());
                    Log.Error("NetworkingData class", "reading finish");
                }
            }

            Log.Error("NetworkingData class", "count from reading" + _medicationSchedules.Count);
            return _medicationSchedules.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
        }

        public async Task<bool> SaveListInDBTask(List<MedicationSchedule> list)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();

            foreach (var element in list)
            {
                Log.Error("NetworkingData class saving..", element.ToString());
                if (!(await SearchItemTask(element.Uuid)))
                {
                    

                    var objMed = await getElementByUUID(element.Uuid);
                    if (objMed != null && element.IdNotification == 0)
                    {
                        element.IdNotification = objMed.IdNotification;
                    }
                    Log.Error("NetworkingData class", "inserting in db..");
                    await _db.Insert(new MedicineServerRecords()
                    {
                        Title = element.Title,
                        Content = element.Content,
                        DateTime = element.Timestampstring,
                        Uuid = element.Uuid,
                        Postpone = element.Postpone + "",
                        IdNotification = "" + element.IdNotification
                    });
                }
            }

//            var l = await ReadListFromDbFutureDataTask();
//            Log.Error("NeworkingData class", "data from db");
//
//            foreach (var el in l)
//            {
//                Log.Error("NetwokingData class", el.ToString());
//            }

            Log.Error("NeworkingData class", "saved");

            return true;
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
                    medicationScheduleList.Add(new MedicationSchedule(uuid, timestampString, title, content, postpone, 0));
                }
                return medicationScheduleList;
            }
            return null;
        }

      

        public async Task<MedicationSchedule> getElementByUUID(string UUIDmed)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{UUIDmed}'");

            if (list.Count() != 0)
            {
                foreach (var item in list)
                {
                    return new MedicationSchedule(item.Uuid, item.DateTime, item.Title, item.Content, int.Parse(item.Postpone), int.Parse(item.IdNotification));
                }
            }
            return null;
        }


        public async Task<List<MedicationSchedule>> ReadListFromDbFutureDataTask()
        {
            var list = await GetDataFromDb();
            var currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();

            foreach (var elem in list)
            {
                try
                {
                    var medDate = Convert.ToDateTime(elem.DateTime);
                    if (medDate >= currentDate)
                    {
                        listMedSch.Add(new MedicationSchedule(elem.Uuid, elem.DateTime, elem.Title, elem.Content,
                            int.Parse(elem.Postpone), int.Parse(elem.IdNotification)));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ERR", e.ToString());
                }
            }

            Log.Error("NetworkingData class", "future data list count from db" + listMedSch.Count);
            return listMedSch;
        }

        private static List<MedicationSchedule> ExtractFutureData(List<MedicationSchedule> list)
        {
            var currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();

            foreach (var elem in list)
            {
                try
                {
                    var medDate = Convert.ToDateTime(elem.Timestampstring);
                    if (medDate >= currentDate)
                    {
                        listMedSch.Add(new MedicationSchedule(elem.Uuid, elem.Timestampstring, elem.Title, elem.Content,
                            elem.Postpone, elem.IdNotification));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ERR", e.ToString());
                }
            }

            Log.Error("NetworkingData class", "future data list count from db" + listMedSch.Count);
            return listMedSch;
        }

        public async Task<List<MedicationSchedule>> ReadListFromDbPastDataTask()
        {
            var list = await GetDataFromDb();
            var currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();

            foreach (var elem in list)
            {
                try
                {
                    var medDate = Convert.ToDateTime(elem.DateTime);
                    if (medDate <= currentDate)
                    {
                        listMedSch.Add(new MedicationSchedule(elem.Uuid, elem.DateTime, elem.Title, elem.Content,
                            int.Parse(elem.Postpone), int.Parse(elem.IdNotification)));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ERR", e.ToString());
                }
            }
            Log.Error("NetworkingData class", "past data list count from db" + listMedSch.Count);
            return listMedSch;
        }

        private async Task<IEnumerable<MedicineServerRecords>> GetDataFromDb()
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations("select * from MedicineServerRecords");
            return list;
        }

        public async void removeMedSer(string UUIDmed)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations($"delete from MedicineServerRecords where Uuid ='{UUIDmed}'");
            Log.Error("STORAGE", "item deleted");
            _medicationSchedules.Remove(await getElementByUUID(UUIDmed));
        }

        public async Task<bool> SearchItemTask(string UUIDmed)
        {
            var found = false;
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var c = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{UUIDmed}'");
            if (c.Count() != 0)
            {
                found = true;
                Log.Error("NetworkingData class", "item exists");
            }
            return found;
        }
    }
}