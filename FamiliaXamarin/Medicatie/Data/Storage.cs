﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie.Entities;
using Java.IO;
using Org.Json;
using SQLite;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Task = System.Threading.Tasks.Task;

namespace FamiliaXamarin.Medicatie.Data
{
    class Storage
    {

        private static Storage Instance;
        private List<Disease> DiseaseList;
        private static readonly object padlock = new object();

        // will be removed
        private SqlHelper<MedicineServerRecords> _db;
        private List<MedicationSchedule> _medicationSchedules;
        private Storage()
        {
            this.DiseaseList = new List<Disease>();
            // will be removed
            _medicationSchedules = new List<MedicationSchedule>();
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
        }

        public static Storage GetInstance()
        {
            lock (padlock)
            {
                if (Instance == null)
                {
                    Instance = new Storage();
                }
            }
            return Instance;
        }

        // will be updated to save Storage type of data - personal medication 
        public async Task<bool> saveMedSer(List<MedicationSchedule> list)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            _medicationSchedules = list;
            foreach (var element in _medicationSchedules)
            {
                var objMed = await getElementByUUID(element.Uuid);
                if (objMed != null && element.IdNotification == 0)
                {
                    element.IdNotification = objMed.IdNotification;
                }
                var c = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{element.Uuid}'");
                Log.Error("Count current saveMedSer", c.Count() + "");
                if (c.Count() == 0)
                {
                    Log.Error("STORAGE", "se introduc date in DB..");
                    await _db.Insert(new MedicineServerRecords()
                    {
                        Title = element.Title,
                        Content = element.Content,
                        DateTime = element.Timestampstring,
                        Uuid = element.Uuid,
                        Postpone = element.Postpone + "",
                        IdNotification = element.IdNotification + ""
                    });
                }
            }
            Log.Error("STORAGE", "finalizare");
            return true;
        }

        // will be removed & moved to NetworkingData class
        public async void saveElementMedSer(MedicationSchedule med)
        {
            try
            {
                var c = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{med.Uuid}'");
                Log.Error("Count current save Element", c.Count() + "");
                if (c.Count() == 0)
                {
                    Log.Error("STORAGE", "se introduc date in DB..");
                    await _db.Insert(new MedicineServerRecords()
                    {
                        Title = med.Title,
                        Content = med.Content,
                        DateTime = med.Timestampstring,
                        Uuid = med.Uuid,
                        Postpone = med.Postpone + "",
                        IdNotification = med.IdNotification + ""

                    });
                    _medicationSchedules.Add(med);
                    Log.Error("STORAGE", _medicationSchedules.Count() + "");
                }
            }
            catch (Exception e)
            {
                Log.Error("ERR", e.ToString());
            }
        }

        // will be removed
        public async Task<List<MedicationSchedule>> readMedSer()
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations("select * from MedicineServerRecords");
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
            _medicationSchedules = listMedSch;
            return listMedSch;
        }

        // will be removed
        public async void removeMedSer(string UUIDmed)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations($"delete from MedicineServerRecords where Uuid ='{UUIDmed}'");
            Log.Error("STORAGE", "item deleted");
            _medicationSchedules.Remove(await getElementByUUID(UUIDmed));
        }

        // will be removed
        public async Task<bool> isHere(string UUIDmed)
        {
            var ok = false;
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var c = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{UUIDmed}'");

            if (c.Count() != 0)
            {
                ok = true;
                Log.Error("STORAGE", "item exists");
            }

            return ok;
        }

        public List<MedicationSchedule> getMSList()
        {
            return new List<MedicationSchedule>(_medicationSchedules);
        }

        // will be removed
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

        #region personal medication


        public async Task<List<MedicationSchedule>> GetPersonalMedicationConverted()
        {
            var listFromFileConverted = ConvertPersonalMedicationListToMedicationSchedules(GetInstance().GetDiseases());
            var listFromLocalDb = await ReadListFromDbPastDataTask();

            if (listFromLocalDb.Count == 0)
            {
                Log.Error("Storage", "saving in db.." + await SaveListInDBTask(listFromFileConverted));
                return listFromFileConverted;
            }

            if (listFromLocalDb.Count != 0)
            {
                Log.Error("STORAGE class", " selecting data from lists...");

                var finalList = new List<MedicationSchedule>();
                foreach (var fileItem in listFromFileConverted)
                {
                    var diseasemedFile = fileItem.Uuid.Split("hour")[0];
                    var hourFile = fileItem.Uuid.Split("hour")[1];

                    var listWithRemovedItems = new List<MedicationSchedule>();

                    for (var i = 0; i < listFromLocalDb.Count; i++)
                    {
                        var dbItem = listFromLocalDb[i];
                        var diseasemedDB = dbItem.Uuid.Split("hour")[0];
                        var hourDB = dbItem.Uuid.Split("hour")[1];

                        if (hourDB != null & hourDB.Contains("removed"))
                        {
                            Log.Error("STORAGE class REMOVED", "item removed: " + dbItem.ToString());
                            listWithRemovedItems.Add(dbItem);
                        }
                        var isItemRemoved = false;

                        if (listWithRemovedItems.Count != 0)
                        {
                            var itemToFind = fileItem;
                            itemToFind.Uuid += "removed";
                            Log.Error("STORAGE class REMOVED", "uuid to find " + itemToFind.ToString());

                            if (ItemInListFound(itemToFind, listWithRemovedItems))
                            {
                                Log.Error("STORAGE class REMOVED", "item removed: " + dbItem.ToString());
                                isItemRemoved = true;
                            }
                        }
                        
                        if (isItemRemoved == false)
                        {
                            if (diseasemedFile.Equals(diseasemedDB))
                            {
                                if (hourFile.Equals(hourDB))
                                {
                                    finalList.Add(dbItem);
                                    Log.Error("STORAGE class", "same hours, added to final list");
                                }
                                else
                                {
                                    if (!(ItemInListFound(dbItem, listFromFileConverted)))
                                    {
                                        Log.Error("STORAGE class", " item was deleted: " + dbItem.ToString());
                                    }
                                    else
                                    {
                                        if (!(ItemInListFound(fileItem, listFromLocalDb)))
                                        {
                                            //element updated
                                            if (fileItem.Uuid.Contains("removed"))
                                            {

                                            }
                                            else
                                            {
                                                Log.Error("STORAGE class UPDATE", "element to save: " + fileItem.ToString());
                                                var currentDate = DateTime.Now;
                                                var medDate = Convert.ToDateTime(fileItem.Timestampstring);
                                                if (medDate <= currentDate)
                                                {
                                                    var isSaved = await SaveItemInDBTask(fileItem);
                                                    if (isSaved)
                                                    {
                                                        listFromLocalDb.Add(fileItem);

                                                        finalList.Add(fileItem);
                                                    }
                                                    Log.Error("STORAGE class UPDATE", "count db list after adding element: " + listFromLocalDb.Count);
                                                    Log.Error("STORAGE class UPDATE", "list from local db");

                                                    foreach (var el in listFromLocalDb)
                                                    {
                                                        Log.Error("STORAGE class UPDATE", el.ToString());
                                                    }
                                                    finalList.Remove(fileItem);
                                                }
                                            }

                                           
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!(ItemInListFound(fileItem, listFromLocalDb)))
                                {
                                    //save element in db
                                    Log.Error("STORAGE class", "element to save: " + fileItem.ToString());
                                    var currentDate = DateTime.Now;
                                    var medDate = Convert.ToDateTime(fileItem.Timestampstring);
                                    if (medDate <= currentDate)
                                    {
                                        var isSaved = await SaveItemInDBTask(fileItem);
                                        if (isSaved)
                                        {
                                            listFromLocalDb.Add(fileItem);
                                            finalList.Add(fileItem);
                                        }
                                        Log.Error("STORAGE class", "count db list after adding element: " + listFromLocalDb.Count);
                                        Log.Error("STORAGE class", "list from local db");
                                        foreach (var el in listFromLocalDb)
                                        {
                                            Log.Error("STORAGE class", el.ToString());
                                        }
                                        finalList.Remove(fileItem);
                                    }
                                }
                            }
                        }
                    }
                }
                Log.Error("STORAGE class", "final list");
                foreach (var el in finalList)
                {
                    Log.Error("STORAGE class", el.ToString());
                }

                Log.Error("STORAGE class", "return final list");
                return finalList;
            }

            return listFromFileConverted;
            // id example: disease0medid0hour0
            // 1. lista din db = 0 => salvare in db med din file
            // 2.lista din db != 0 => parcurgere array-uri
            // 2.1. comparare uuid-uri: if disease0medid0 exists in both arrays, verify hour, if hours are different =: element updated
            // 2.1.1 if hour from db list doesn't exist don't add it to final array  
            // 2.2 else for 2.1 if disease0medid0 exists only in file list, save it in db 
        }

        public async Task<bool> RemoveItemFromDBTask(MedicationSchedule item)
        {
            try
            {
                if (item.Uuid.Contains("removed")) return false;
                _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
                var newUuid = item.Uuid + "removed";
                await _db.QueryValuations($"update MedicineServerRecords set Uuid='{newUuid}' where Uuid ='{item.Uuid}'");
                Log.Error("STORAGE deleted item new UUid: ", (await getElementByUUID(newUuid)).ToString());
            }
            catch (Exception e)
            {
                Log.Error("STORAGE class ERROR", e.Message);
                return false;
            }
            return true;
        }

        private bool ItemInListFound(MedicationSchedule item, List<MedicationSchedule> list)
        {
            foreach (var variable in list)
            {
                if (variable.Uuid.Equals(item.Uuid))
                {
                    return true;
                }
            }
            return false;
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
            Log.Error("Storage class", "past data list count from db" + listMedSch.Count);
            //            var arr = listMedSch.Where(c => !(c.Uuid.Contains("disease"))).ToList();
            return new List<MedicationSchedule>(listMedSch.Where(c => c.Uuid.Contains("disease")).ToList());
        }

        private async Task<IEnumerable<MedicineServerRecords>> GetDataFromDb()
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var list = await _db.QueryValuations("select * from MedicineServerRecords");
            return list;
        }

        public async Task<bool> SaveItemInDBTask(MedicationSchedule element)
        {
            try
            {
                _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
                Log.Error("Storage class saving element..", element.ToString());
                if (!(await SearchItemTask(element.Uuid)))
                {
                    Log.Error("Storage class", "inserting in db..");
                    await _db.Insert(new MedicineServerRecords()
                    {
                        Title = element.Title,
                        Content = element.Content,
                        DateTime = element.Timestampstring,
                        Uuid = element.Uuid,
                        Postpone = element.Postpone + "",
                        IdNotification = "" + element.IdNotification
                    });
                    Log.Error("Storage class", "element saved");
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error("STORAGE error", e.Message);
            }

            return false;
        }

        public async Task<bool> SaveListInDBTask(List<MedicationSchedule> list)
        {
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();

            foreach (var element in list)
            {
                Log.Error("Storage class saving..", element.ToString());
                if (!(await SearchItemTask(element.Uuid)))
                {


                    var objMed = await getElementByUUID(element.Uuid);
                    if (objMed != null && element.IdNotification == 0)
                    {
                        element.IdNotification = objMed.IdNotification;
                    }
                    Log.Error("Storage class", "inserting in db..");
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

            Log.Error("Storage class", "saved");

            return true;
        }

        public async Task<bool> SearchItemTask(string UUIDmed)
        {
            var found = false;
            _db = await SqlHelper<MedicineServerRecords>.CreateAsync();
            var c = await _db.QueryValuations($"SELECT * from MedicineServerRecords WHERE Uuid ='{UUIDmed}'");
            if (c.Count() != 0)
            {
                found = true;
                Log.Error("Storage class", "item exists");
            }
            return found;
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
                            var objMedSch = new MedicationSchedule("disease" + item.Id + "med" + itemMed.IdMed + "hour" + itemHour.Id + "time" + dtMed.ToString(), dtMed.ToString(), item.DiseaseName, itemMed.Name, 5, 0);
                            listMedSchPersonal.Add(objMedSch);
                            for (int j = 1; j < days; j++)
                            {
                                dtMed = dtMed.AddDays(1);
                                objMedSch = new MedicationSchedule("disease" + item.Id + "med" + itemMed.IdMed + "hour" + itemHour.Id + "time" + dtMed.ToString(), dtMed.ToString(), item.DiseaseName, itemMed.Name, 5, 0);
                                listMedSchPersonal.Add(objMedSch);
                            }
                        }
                    }
                }
            }
            return listMedSchPersonal.OrderBy(x => DateTime.Parse(x.Timestampstring)).ToList();
        }


        public List<Disease> GetDiseases()
        {
            return new List<Disease>(DiseaseList);
        }

        public void AddDisease(Context context, Disease disease)
        {
            DiseaseList.Add(disease);
            disease.Id = DiseaseList.IndexOf(disease).ToString();
            SaveData(context, disease);
        }

        public Disease GetDisease(string id)
        {
            foreach (Disease item in DiseaseList)
            {
                if (item.Id.Equals(id))
                {
                    return item;
                }
            }
            return null;
        }

        public Disease removeBoala(Context context, Disease disease)
        {
            DiseaseList.Remove(disease);
            SaveCurrentData(context);
            return disease;
        }

        public Disease updateBoala(Context context, Disease disease)
        {
            foreach (Disease item in DiseaseList)
            {
                if (item.Id.Equals(disease.Id))
                {
                    item.DiseaseName = disease.DiseaseName;
                    item.ListOfMedicines = disease.ListOfMedicines;
                }
            }
            SaveCurrentData(context);
            return disease;
        }

        public void SaveCurrentData(Context context)
        {
            try
            {
                File file = new File(context.FilesDir, Constants.MedicationFile);
                JSONArray data = new JSONArray();

                if (DiseaseList.Count == 0)
                {
                    file.Delete();
                    file = new File(context.FilesDir, Constants.MedicationFile);
                }

                for (int i = 0; i < DiseaseList.Count; i++)
                {
                    data.Put(CreateJsonObject(DiseaseList[i]));
                }

                FileWriter fileWriter = new FileWriter(file);
                BufferedWriter outBufferedWriter = new BufferedWriter(fileWriter);
                outBufferedWriter.Write(data.ToString());
                outBufferedWriter.Close();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        private JSONObject CreateJsonObject(Disease disease)
        {
            JSONObject jsonObject = new JSONObject();
            try
            {
                jsonObject.Put("idBoala", disease.Id);
                jsonObject.Put("numeBoala", disease.DiseaseName);
                JSONArray listOfMedicines = new JSONArray();

                for (int i = 0; i < disease.ListOfMedicines.Count; i++)
                {
                    JSONObject medicament = new JSONObject();
                    medicament.Put("idMedicament", disease.ListOfMedicines[i].IdMed);
                    medicament.Put("numeMedicament", disease.ListOfMedicines[i].Name);
                    medicament.Put("dataMedicament", disease.ListOfMedicines[i].Date.ToString());
                    medicament.Put("nrZileMedicament", disease.ListOfMedicines[i].NumberOfDays);
                    medicament.Put("intervalZi", disease.ListOfMedicines[i].IntervalOfDay);
                    JSONArray arrayOfHours = new JSONArray();

                    for (int j = 0; j < disease.ListOfMedicines[i].Hours.Count; j++)
                    {
                        arrayOfHours.Put(new JSONObject().Put("idOra", disease.ListOfMedicines[i].Hours[j].Id).Put("numeOra", disease.ListOfMedicines[i].Hours[j].HourName));
                    }

                    medicament.Put("listaOre", arrayOfHours);
                    listOfMedicines.Put(medicament);
                }
                jsonObject.Put("listaMedicamente", listOfMedicines);
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            return jsonObject;
        }

        public void SaveData(Context context, Disease disease)
        {
            try
            {
                string data = ReadData(context);
                JSONArray listOfDiseases = data != null ? new JSONArray(data) : new JSONArray();

                listOfDiseases.Put(CreateJsonObject(disease));

                File file = new File(context.FilesDir, Constants.MedicationFile);
                FileWriter fileWriter = new FileWriter(file);
                BufferedWriter outBufferedWriter = new BufferedWriter(fileWriter);
                outBufferedWriter.Write(listOfDiseases.ToString());
                outBufferedWriter.Close();
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        public string ReadData(Context context)
        {
            try
            {
                Stream fis = context.OpenFileInput(Constants.MedicationFile);
                InputStreamReader isr = new InputStreamReader(fis);
                BufferedReader bufferedReader = new BufferedReader(isr);
                StringBuilder sb = new StringBuilder();
                string line;
                while ((line = bufferedReader.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                fis.Close();
                return sb.ToString();
            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }
            return null;
        }

        public List<Disease> GetListOfDiseasesFromFile(Context context)
        {
            try
            {
                DiseaseList.Clear();
                string data = ReadData(context);
                JSONArray diseases = data != null ? new JSONArray(data) : new JSONArray();

                for (int i = 0; i < diseases.Length(); i++)
                {
                    Disease b = new Disease();
                    JSONObject bTemp = (JSONObject)diseases.Get(i);

                    b.Id = bTemp.GetString("idBoala");
                    b.DiseaseName = bTemp.GetString("numeBoala");

                    List<Medicine> listOfMedicines = new List<Medicine>();
                    JSONArray arrayOfMedicines = (JSONArray)bTemp.Get("listaMedicamente");

                    for (int j = 0; j < arrayOfMedicines.Length(); j++)
                    {
                        Medicine m = new Medicine();
                        m.IdMed = (string)((JSONObject)arrayOfMedicines.Get(j)).Get("idMedicament");
                        m.Name = (string)((JSONObject)arrayOfMedicines.Get(j)).Get("numeMedicament");
                        Log.Error("Storage conversion date", (string)((JSONObject)arrayOfMedicines.Get(j)).Get("dataMedicament"));
                        m.Date = DateTime.Parse((string)((JSONObject)arrayOfMedicines.Get(j)).Get("dataMedicament"));
                        Log.Error("Storage after conversion", m.Date.ToString());
                        m.NumberOfDays = (int)((JSONObject)arrayOfMedicines.Get(j)).Get("nrZileMedicament");
                        m.IntervalOfDay = (int)((JSONObject)arrayOfMedicines.Get(j)).Get("intervalZi");

                        List<Hour> ListOfHours = new List<Hour>();
                        JSONArray arrayOfHours = (JSONArray)((JSONObject)arrayOfMedicines.Get(j)).Get("listaOre");
                        for (int k = 0; k < arrayOfHours.Length(); k++)
                        {
                            Hour h = new Hour();
                            h.Id = (string)((JSONObject)arrayOfHours.Get(k)).Get("idOra");
                            h.HourName = (string)((JSONObject)arrayOfHours.Get(k)).Get("numeOra");
                            ListOfHours.Add(h);
                        }
                        m.Hours = ListOfHours;
                        listOfMedicines.Add(m);
                    }
                    b.ListOfMedicines = listOfMedicines;
                    DiseaseList.Add(b);
                }
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            return new List<Disease>(DiseaseList);
        }
        #endregion
    }
}