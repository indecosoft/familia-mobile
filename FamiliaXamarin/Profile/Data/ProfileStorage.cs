using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Util;
using Familia.Helpers;
using Familia.JsonModels;
using Familia.Profile.Data.db;

namespace Familia.Profile.Data
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
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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
            var res = false;
            try
            {

                if (personalData != null)
                {

                    if (await DropProfileDataTableTask())
                    {
                        Log.Error("ProfileStorage", "saving data ...");
                        _dbProfile = await SqlHelper<ProfileDataModel>.CreateAsync();

                        await _dbProfile.Insert(new ProfileDataModel {
                            Base64Image = personalData.Base64Image,
                            DateOfBirth = personalData.DateOfBirth,
                            Gender = personalData.Gender,
                            ImageExtension = personalData.ImageExtension,
                            ImageName = personalData.ImageName
                        });

                        if (await DropDiseasesTableTask())
                        {
                            foreach (PersonalDisease item in personalData.listOfPersonalDiseases)
                            {
                                _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
                                await _dbProfileDisease.Insert(new DiseaseDataModel {
                                    Name = item.Name,
                                    Cod = item.Cod
                                });
                            }
                        }

                        Log.Error("ProfileStorage", "saved");
                        res =  true;
                    }
                }
                
//            });
                }
                catch (Exception ex)
            {
                Log.Error("ERR", ex.Message);
            }

            return res;
        }

        public async Task<bool> saveDiseases(List<PersonalDisease> list)
        {
            try
            {
                if (list == null) return false;
                Log.Error("ProfileStorage", "saving diseases ...");

                if (await DropDiseasesTableTask())
                {
                    _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
                    
                    foreach (PersonalDisease item in list)
                    {
                            Log.Error("ProfileStorage", "inserting in db..");
                            await _dbProfileDisease.Insert(new DiseaseDataModel {
                                Name = item.Name,
                                Cod = item.Cod
                            });
                    }

                    Log.Error("ProfileStorage", "saved");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("ProfileStorage ERR", ex.Message);
                return false;
            }
        }

        private async Task<bool> DropDiseasesTableTask()
        {
            try
            {
                var sqlHelper = await SqlHelper<DiseaseDataModel>.CreateAsync();
                sqlHelper.DropTables(typeof(DiseaseDataModel));
                return true;
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage Err", e.Message);
                return false;
            }
        }

        private async Task<bool> DropProfileDataTableTask()
        {
            try
            {
                var sqlHelper = await SqlHelper<ProfileDataModel>.CreateAsync();
                sqlHelper.DropTables(typeof(ProfileDataModel));
                return true;
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage Err", e.Message);
                return false;
            }
        }


        public async Task<bool> SearchDiseaseInDbTask(int cod)
        {
            var found = false;
            try
            {
                _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
                var c = await _dbProfileDisease.QueryValuations($"SELECT * from DiseaseDataModel WHERE Cod ='{cod}'");
                if (c.Count() != 0)
                {
                    found = true;
                    Log.Error("ProfileStorage", "item exists");
                }
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage ERR", e.Message);
                
            }
            
            return found;
        }


        public async Task<PersonalData> read()
        {
            try {
                Log.Error("ProfileStorage", "reading...");
                var list = await GetDiseasesFromDb();
                Log.Error("ProfileStorage", " after getting list for db");

                var listDiseases = new List<PersonalDisease>();
                if (list == null) return null;
                Log.Error("ProfileStorage", " start adding item in list");
                foreach (DiseaseDataModel item in list)
                {
                    var obj = new PersonalDisease(item.Cod, item.Name);
                    obj.Id = item.Id;
                    listDiseases.Add(obj);
                }
                Log.Error("ProfileStorage", "after adding all items in lists");
                Log.Error("ProfileStorage", " start getting profile info from db");
                var listProfile = await GetProfileInfoFromDb();
                if (listProfile == null) return null;
                Log.Error("ProfileStorage", " start adding item in list for profile");

                foreach (ProfileDataModel item in listProfile)
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
                Log.Error("ProfileStorage", " return");
                return personalData;

            }
            catch (Exception e) {
                Log.Error("ProfileStorage err", e.Message);
                return null;
            }
            
        }

        private async Task<IEnumerable<DiseaseDataModel>> GetDiseasesFromDb()
        {
            try
            {
                _dbProfileDisease = await SqlHelper<DiseaseDataModel>.CreateAsync();
               var  list = await _dbProfileDisease.QueryValuations("select * from DiseaseDataModel");
                return list;
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage ERR", e.Message);
                return null;
            }
            
        }

        private async Task<IEnumerable<ProfileDataModel>> GetProfileInfoFromDb()
        {
            try
            {
                _dbProfile = await SqlHelper<ProfileDataModel>.CreateAsync();
                var list = await _dbProfile.QueryValuations("select * from ProfileDataModel");
                return list;
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage ERR", e.Message);
                return null;
            }
        }


        public void logData()
        {

            Log.Error("ProfileStorage ", "Personal diseases:");
            foreach (PersonalDisease item in personalData.listOfPersonalDiseases)
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