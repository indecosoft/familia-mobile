using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Entities;
using Java.IO;
using Org.Json;
using File = Java.IO.File;
using IOException = Java.IO.IOException;

namespace FamiliaXamarin.Medicatie.Data
{
    class Storage
    {
       
        private static Storage Instance;
        private List<Disease> DiseaseList;
        private static readonly object padlock = new object();

        private Storage()
        {
            this.DiseaseList = new List<Disease>();
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
            JSONArray data = new JSONArray();
            for (int i = 0; i < DiseaseList.Count; i++)
            {
                data.Put(CreateJsonObject(DiseaseList[i]));
            }

            File file = new File(context.FilesDir, Constants.File);
            try
            {
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
                    medicament.Put("dataMedicament", disease.ListOfMedicines[i].Date);
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
                JSONArray listOfDiseases;
                if (data != null)
                {
                    listOfDiseases = new JSONArray(data);
                }
                else
                {
                    listOfDiseases = new JSONArray();
                }

                listOfDiseases.Put(CreateJsonObject(disease));

                File file = new File(context.FilesDir, Constants.File);
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
            Stream fis;
            try
            {
                fis = context.OpenFileInput(Constants.File);
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
                JSONArray diseases;
                if (data != null)
                {
                    diseases = new JSONArray(data);
                }
                else
                {
                    diseases = new JSONArray();
                }

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
                        m.IdMed= (string)((JSONObject)arrayOfMedicines.Get(j)).Get("idMedicament");
                        m.Name = (string)((JSONObject)arrayOfMedicines.Get(j)).Get("numeMedicament");
                        m.Date = (string)((JSONObject)arrayOfMedicines.Get(j)).Get("dataMedicament");
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