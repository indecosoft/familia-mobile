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
        private int id;
        private static Storage instance;
        private List<Boala> boalaList;
        private static readonly object padlock = new object();

        private Storage()
        {
            this.boalaList = new List<Boala>();
        }
//        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Storage getInstance()
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Storage();
                }

            }

            return instance;
        }

        public List<Boala> getBoli()
        {
            return new List<Boala>(boalaList);
        }


        public void addBoala(Context context, Boala boala)
        {
            //boala.setId("boala" + id++);
            //Random r = new Random();
            //boala.Id = r.Next(0, int.MaxValue) * r.Next(0, int.MaxValue);
            boalaList.Add(boala);
            boala.Id = boalaList.IndexOf(boala).ToString();
            saveData(context, boala);



        }

        public Boala getBoala(string id)
        {
            foreach (Boala item in boalaList)
            {
                if (item.Id.Equals(id))
                {
                    return item;
                }
            }
            return null;
        }

        public Boala removeBoala(Context context, Boala boala)
        {
            boalaList.Remove(boala);
            saveCurrentData(context);
            return boala;
        }


        public Boala updateBoala(Context context, Boala boala)
        {
            foreach (Boala item in boalaList)
            {
                if (item.Id.Equals(boala.Id))
                {
                    item.NumeBoala = boala.NumeBoala;
                    item.MedicamentList = boala.MedicamentList;
                }
            }
            saveCurrentData(context);
            return boala;

//            for (Boala item : boalaList)
//            {
//                if (item.getId().equals(boala.getId()))
//                {
//                    item.setNumeBoala(boala.getNumeBoala());
//                    item.setMedicamentList(boala.getMedicamentList());
//                }
//            }
//            saveCurrentData(context);
//            return boala;
        
    }

        public void saveCurrentData(Context context)
        {
            JSONArray data = new JSONArray();
            for (int i = 0; i < boalaList.Count; i++)
            {
                data.Put(createJsonObject(boalaList[i]));
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

        private JSONObject createJsonObject(Boala boala)
        {
            JSONObject jsonObject = new JSONObject();
            try
            {
                jsonObject.Put("idBoala", boala.Id);

                jsonObject.Put("numeBoala", boala.NumeBoala);

                JSONArray listaMedicamente = new JSONArray();
                for (int i = 0; i < boala.MedicamentList.Count; i++)
                {
                    JSONObject medicament = new JSONObject();
                    medicament.Put("idMedicament", boala.MedicamentList[i].IdMed);
                    medicament.Put("numeMedicament", boala.MedicamentList[i].Name);
                    medicament.Put("dataMedicament", boala.MedicamentList[i].Date);
                    medicament.Put("nrZileMedicament", boala.MedicamentList[i].NrZile);
                    medicament.Put("intervalZi", boala.MedicamentList[i].IntervalZi);

                    JSONArray arrayOre = new JSONArray();

                    for (int j = 0; j < boala.MedicamentList[i].Hours.Count; j++)
                    {
                        arrayOre.Put(new JSONObject().Put("idOra", boala.MedicamentList[i].Hours[j].Id).Put("numeOra", boala.MedicamentList[i].Hours[j].Nume));
                    }

                    medicament.Put("listaOre", arrayOre);
                    listaMedicamente.Put(medicament);
                }

                jsonObject.Put("listaMedicamente", listaMedicamente);
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            return jsonObject;
        }

        public void saveData(Context context, Boala boala)
        {
            try
            {
                string data = readData(context);
                JSONArray listaDeBoli;
                if (data != null)
                {
                    listaDeBoli = new JSONArray(data);
                }
                else
                {
                    listaDeBoli = new JSONArray();
                }

                listaDeBoli.Put(createJsonObject(boala));

                File file = new File(context.FilesDir, Constants.File);
                FileWriter fileWriter = new FileWriter(file);
                BufferedWriter outBufferedWriter = new BufferedWriter(fileWriter);
                outBufferedWriter.Write(listaDeBoli.ToString());
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

        public string readData(Context context)
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

        public List<Boala> getBoliTest(Context context)
        {
            try
            {
                boalaList.Clear();

                string data = readData(context);
                JSONArray boli;
                if (data != null)
                {
                    boli = new JSONArray(data);
                }
                else
                {
                    boli = new JSONArray();
                }

                for (int i = 0; i < boli.Length(); i++)
                {
                    Boala b = new Boala();
                    JSONObject bTemp = (JSONObject)boli.Get(i);

                    b.Id = bTemp.GetString("idBoala");
                    b.NumeBoala = bTemp.GetString("numeBoala");

                    List<Medicament> lMed = new List<Medicament>();
                    JSONArray lMedAr = (JSONArray)bTemp.Get("listaMedicamente");

                    for (int j = 0; j < lMedAr.Length(); j++)
                    {
                        Medicament m = new Medicament();
                        m.IdMed= (string)((JSONObject)lMedAr.Get(j)).Get("idMedicament");
                        m.Name = (string)((JSONObject)lMedAr.Get(j)).Get("numeMedicament");
                        m.Date = (string)((JSONObject)lMedAr.Get(j)).Get("dataMedicament");
                        m.NrZile = (int)((JSONObject)lMedAr.Get(j)).Get("nrZileMedicament");
                        m.IntervalZi = (int)((JSONObject)lMedAr.Get(j)).Get("intervalZi");

                        List<Hour> lOre = new List<Hour>();
                        JSONArray lOreAr = (JSONArray)((JSONObject)lMedAr.Get(j)).Get("listaOre");

                        for (int k = 0; k < lOreAr.Length(); k++)
                        {
                            Hour h = new Hour();
                            h.Id = (string)((JSONObject)lOreAr.Get(k)).Get("idOra");
                            h.Nume = (string)((JSONObject)lOreAr.Get(k)).Get("numeOra");
                            lOre.Add(h);
                        }

                        m.Hours = lOre;

                        lMed.Add(m);
                    }
                    b.MedicamentList = lMed;
                    boalaList.Add(b);
                }
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            return new List<Boala>(boalaList);
        }
    }
}