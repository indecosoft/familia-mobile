using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Util;
using Familia.DataModels;
using Familia.Helpers;
using Familia.Medicatie.Entities;
using Org.Json;

namespace Familia.Medicatie.Data
{
    class NetworkingData
    {
        private List<MedicationSchedule> _medicationSchedules;
        private SqlHelper<MedicineServerRecords> _db;

        public NetworkingData()
        {
            _medicationSchedules = new List<MedicationSchedule>();
        }

        private async Task<List<MedicationSchedule>> CallServerFutureData(int size)
        {
            var serverItemsError = false;
            var medications = new List<MedicationSchedule>();
            await Task.Run(async () =>
            {
                try
                {
                    string res = await WebServices.WebServices.Get($"api/medicineList/{Utils.GetDefaults("Id")}/{size}", Utils.GetDefaults("Token")); //this should be here
                    if (res != null)
                    {
                        Log.Error("Networking data FUTURE", " RESULT_FOR_MEDICATIE" + res);
                        medications = ParseResultFromUrl(res);
                        Log.Error("Networking data FUTURE", " count: " + medications.Count);
                    }
                    else
                    {
                        serverItemsError = true;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("MEDICATION SERVER ERR FUTURE", e.Message);
                }
            });

            if (serverItemsError)
            {
                Log.Error("MSF FUTURE", "methos id returning null bc server did not respond");
                return null;
            }

            return medications;
        }

        private async Task<List<MedicationSchedule>> CallServerPastData(int size)
        {
            var serverItemsError = false;
            var medications = new List<MedicationSchedule>();
            await Task.Run(async () =>
            {
                try
                {
                    string res = await WebServices.WebServices.Get($"/api/missedMedicine/{Utils.GetDefaults("Id")}/{size}", Utils.GetDefaults("Token")); //this should be here
                    if (res != null)
                    {
                        Log.Error("Networking data", " RESULT_FOR_MEDICATIE" + res);
                        medications = ParseResultFromUrl(res);
                        Log.Error("Networking data", " count: " + medications.Count);
                    }
                    else
                    {
                        serverItemsError = true;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("MEDICATION SERVER ERR", e.Message);
                }
            });

            if (serverItemsError)
            {
                return null;
            }

            return medications;
        }

        public async Task<List<MedicationSchedule>> ReadFutureDataTask(int size)
        {
            _medicationSchedules = await CallServerFutureData(size);
            if (_medicationSchedules == null)
            {
                if (size == 0)
                {
                    try
                    {
                        _medicationSchedules = new List<MedicationSchedule>(await ReadListFromDbFutureDataTask());
                    }
                    catch (Exception e)
                    {
                        Log.Error("NetworkingData class", "err " + e.Message);
                    }

                }
            }
            else
            if (_medicationSchedules.Count != 0)
            {
                bool finished = await SaveListInDBTask(_medicationSchedules);
                Log.Error("NetworkingData class", "saved: " + finished);
            }



            if (_medicationSchedules != null)
            {
                Log.Error("NetworkingData class FUTURE", "count from reading" + _medicationSchedules.Count);

                return ExtractFutureData(_medicationSchedules.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList());
            }
            else {
                return new List<MedicationSchedule>();
            }
        }

        public async Task<List<MedicationSchedule>> ReadPastDataTask(int size)
        {
            _medicationSchedules = await CallServerPastData(size);
            if (_medicationSchedules == null)
            {
                if (size == 0)
                {
                    try {
                        Log.Error("NetworkingData class", "reading from local db..");
                        _medicationSchedules = new List<MedicationSchedule>(await ReadListFromDbPastDataTask());
                        Log.Error("NetworkingData class", "reading finish");
                    } catch (Exception e) {
                        Log.Error("NetworkingData class", "err " + e.Message);
                    }
                   
                }
            }
            else
            if (_medicationSchedules.Count != 0)
            {
                bool finished = await SaveListInDBTask(_medicationSchedules);
                await deleteStinkyItems(_medicationSchedules, false);
                Log.Error("NetworkingData class", "saved: " + finished);
            }



            if (_medicationSchedules != null) {
                Log.Error("NetworkingData class", "count from reading" + _medicationSchedules.Count);

                return _medicationSchedules.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
            }
            else
            {
                return new List<MedicationSchedule>();
            }
        }

        public async Task<bool> SaveListInDBTask(List<MedicationSchedule> list)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();

            foreach (MedicationSchedule element in list)
            {
                Log.Error("NetworkingData class saving..", element.ToString());
                if (!(await SearchItemTask(element.Uuid)))
                {
                    MedicationSchedule objMed = await getElementByUUID(element.Uuid);

                    Log.Error("NetworkingData class", "saving obj pi id " + element.IdNotification);
                    if (objMed != null && element.IdNotification == 0)
                    {
                        element.IdNotification = objMed.IdNotification;
                        Log.Error("NetworkingData class", "saving obj pi id " + objMed.IdNotification);
                    }
                    Log.Error("NetworkingData class", "inserting in db..");
                    await _db.Insert(new MedicineServerRecords
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
                    string uuid = obj.GetString("uuid");
                    string timestampString = obj.GetString("timestamp");
                    string title = obj.GetString("title");
                    string content = obj.GetString("content");
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
                foreach (MedicineServerRecords item in list)
                {
                    return new MedicationSchedule(item.Uuid, item.DateTime, item.Title, item.Content, int.Parse(item.Postpone), int.Parse(item.IdNotification));
                }
            }
            return null;
        }


        public async Task<List<MedicationSchedule>> ReadListFromDbFutureDataTask()
        {
            var list = await GetDataFromDb();
            
            DateTime currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();

            foreach (MedicineServerRecords elem in list)
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
            DateTime currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();

            foreach (MedicationSchedule elem in list)
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
            Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 1");
            List<MedicineServerRecords> list = null;
            try {
                list =(List<MedicineServerRecords>) await GetDataFromDb();
            } catch (Exception e) {
                Log.Error("NetworkingData class", " ERR " + e.Message);
            }
             
            Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 2");
            DateTime currentDate = DateTime.Now;
            var listMedSch = new List<MedicationSchedule>();
            if (list != null) {
                Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 3");
                foreach (MedicineServerRecords elem in list)
                {
                    try
                    {
                        Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 4 / " + elem.Uuid);
                        var medDate = Convert.ToDateTime(elem.DateTime);
                        if (medDate <= currentDate)
                        {
                            Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 5");
                            listMedSch.Add(new MedicationSchedule(elem.Uuid, elem.DateTime, elem.Title, elem.Content,
                                int.Parse(elem.Postpone), int.Parse(elem.IdNotification)));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("NetworkingData class", " ReadListFromDbPastDataTask 6");
                        Log.Error("ERR", e.ToString());
                    }
                }
            }
            
            Log.Error("NetworkingData class", "past data list count from db" + listMedSch.Count);
            return listMedSch;
        }

        private async Task<IEnumerable<MedicineServerRecords>> GetDataFromDb()
        {
            try {
                _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
                var list = await _db.QueryValuations("select * from MedicineServerRecords");
                return list;
            }
            catch (Exception e) {
                Log.Error("NetworkingData class", " ERR " + e.Message);
                return null;
            }
            
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

        public async Task deleteStinkyItems(List<MedicationSchedule> medications, bool isFutureRequired)
        {
            List<MedicationSchedule> localList = isFutureRequired ? await ReadListFromDbFutureDataTask() : await ReadListFromDbPastDataTask();
            Log.Error("NetworkingData", "deleteStinkyItems 1");
            foreach (MedicationSchedule itemLocal in localList)
            {
               // Log.Error("NetworkingData", "deleteStinkyItems 2 / " + itemLocal.Uuid);
                if (!isItemInListOrListIsEmpty(itemLocal, medications))
                {
                    Log.Error("NetworkingData ", "this item will be deleted " + itemLocal.Title + ", " + itemLocal.Timestampstring + ", idNotification " + itemLocal.IdNotification + ", " + itemLocal.Postpone + ", UUID: " + itemLocal.Uuid);
                    removeMedSer(itemLocal.Uuid);
                }
            }
        }

        private bool isItemInListOrListIsEmpty(MedicationSchedule item, List<MedicationSchedule> list)
        {

            if (list.Count == 0)
            {
                return false;
            }

            foreach (MedicationSchedule ms in list)
            {
                if (ms.Uuid == item.Uuid)
                {
                    return true;
                }
            }

            return false;
        }

    }
}