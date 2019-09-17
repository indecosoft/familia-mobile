using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.Profile.Data;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;

namespace Familia.Profile
{


    class ProfileStorage
    {


        private static ProfileStorage Instance;
        private static readonly object padlock = new object();
        private FirstSetupModel model;

        public PersonalData personalData { get; set; }

        private SqlHelper<ProfileDataModel> _dbProfile;
        private SqlHelper<DiseaseDataModel> _dbProfileDisease;
       
        private ProfileStorage()
        {
            personalData = new PersonalData();
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var numeDB = "devices_data.db";
        }

        public static ProfileStorage GetInstance()
        {
            lock (padlock)
            {
                if (Instance == null)
                {
                    Instance = new ProfileStorage();
                }
            }
            return Instance;
        }

        public async Task<bool> clearDb()
        {
            try
            {
                var sqlHelper = await SqlHelper<ProfileDataModel>.CreateAsync();
                sqlHelper.DropTables(typeof(ProfileDataModel));

                var sqlHelp = await SqlHelper<DiseaseDataModel>.CreateAsync();
                sqlHelp.DropTables(typeof(DiseaseDataModel));
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Logout Clear Profile Storage Error", e.Message);
                return false;
            }
        }

        public async Task<bool> save()
        {
            try
            {
                if (personalData == null) return false;

                logData();
                Log.Error("ProfileStorage", "saving data ...");
                _dbProfile = await SqlHelper<ProfileDataModel>.CreateAsync();
                await _dbProfile.Insert(new ProfileDataModel()
                {
                    Base64Image = personalData.Base64Image,
                    DateOfBirth = personalData.DateOfBirth,
                    Gender = personalData.Gender,
                    ImageExtension = personalData.ImageExtension,
                    ImageName = personalData.ImageName
                });

                foreach (var item in personalData.listOfPersonalDiseases)
                {
                    _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
                    await _dbProfileDisease.Insert(new DiseaseDataModel()
                    {
                       Name = item.Name,
                       Cod = item.Cod
                    });
                }
                Log.Error("ProfileStorage", "saved");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("ERR", ex.Message);
                return false;
            }
        }

        public async Task<PersonalData> read()
        {
            Log.Error("ProfileStorage", "reading...");
            var list = await GetDiseasesFromDb();
            var listDiseases = new List<PersonalDisease>();
            foreach (var item in list)
            {
                var obj = new PersonalDisease(item.Cod, item.Name);
                obj.Id = item.Id;
                listDiseases.Add(obj);
            }
            
            var listProfile = await GetProfileInfoFromDb();
            foreach (var item in listProfile)
            {
                personalData = new PersonalData(
                    listDiseases,
                    item.Base64Image,
                    item.DateOfBirth,
                    item.Gender,
                    item.ImageName,
                    item.ImageExtension
                );
            }

           
            Log.Error("ProfileStorage", "read");

            if (listDiseases.Count > 0 )
            {
                logData();
                return personalData;
            }

            return null;
        }

        private async Task<IEnumerable<DiseaseDataModel>> GetDiseasesFromDb()
        {
            _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
            var list = await _dbProfileDisease.QueryValuations("select * from DiseaseDataModel");
            return list;
        }

        private async Task<IEnumerable<ProfileDataModel>> GetProfileInfoFromDb()
        {
            _dbProfile = await SqlHelper<ProfileDataModel>.CreateAsync();
            var list = await _dbProfile.QueryValuations("select * from ProfileDataModel");
            return list;
        }


        public void logData()
        {

            Log.Error("ProfileStorage ", "Personal data:");
            foreach (var item in personalData.listOfPersonalDiseases)
            {
                Log.Error("Disease: ", item.Name + item.Cod);
            }

            Log.Error("date: ", personalData.DateOfBirth);
            Log.Error("gender: ", personalData.Gender);
            Log.Error("base64 first 20 characters: ", personalData.Base64Image.Substring(0, 20));
            Log.Error("imgExtension: ", personalData.ImageExtension);
            Log.Error("imgName: ", personalData.ImageName);
        }
    }
}