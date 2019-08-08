using System;
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
    }
}