﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Signature;
using Familia.Helpers;
using Familia.Profile.Data;
using Org.Json;
using Refractored.Controls;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Profile
{
    [Activity(Label = "Profile", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProfileActivity : AppCompatActivity, View.IOnClickListener
    {
        private PersonalData personalData;
        private CircleImageView ciwProfileImage;
        private TextView tvName;
        private TextView tvEmail;
        //private TextView tvBirtdate;
        private TextView tvGender;
        private TextView tvDateOftBirth;
        private TextView tvAge;
        private ImageView iwGender;
        private RecyclerView rv;

        public static string ImageUpdated = "sign";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_profile);
            SetToolbar();
            InitView();
            LoadModel(false);
        }

        private void InitView()
        {
            FindViewById<Button>(Resource.Id.btn_update).SetOnClickListener(this);
            ciwProfileImage = FindViewById<CircleImageView>(Resource.Id.profile_image);
            tvName = FindViewById<TextView>(Resource.Id.tv_name);
            tvEmail = FindViewById<TextView>(Resource.Id.tv_email);
            tvGender = FindViewById<TextView>(Resource.Id.tv_gender);
            iwGender = FindViewById<ImageView>(Resource.Id.iw_gender);
            tvDateOftBirth = FindViewById<TextView>(Resource.Id.tv_labelDate);
            tvAge = FindViewById<TextView>(Resource.Id.tv_age);
            rv = FindViewById<RecyclerView>(Resource.Id.rv_diseases);
            var layoutManager = new LinearLayoutManager(this);
            rv.SetLayoutManager(layoutManager);

            var tvDeviceId = FindViewById<TextView>(Resource.Id.tv_device_id_value);
            string deviceId = Utils.GetDeviceIdentificator(this);
            if (!string.IsNullOrEmpty(deviceId))
            {
                tvDeviceId.Text = deviceId;
            }

            //TODO hide "afectiuni" for correct type of user
        }
        

        private async Task<PersonView> CallServerToGetData()
        {
            Log.Error("ProfileActivity", "task started");
            PersonView person = null;
            await Task.Run(async () =>
            {
                try
                {
                    string res = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/myProfile", Utils.GetDefaults("Token"));
                    if (res != null)
                    {
                        Log.Error("ProfileActivity", res);
                        person = parseResultFromUrl(res);
                        if (person != null && person.ListOfPersonalDiseases != null)
                        {
                            await ProfileStorage.GetInstance().saveDiseases(person.ListOfPersonalDiseases);
                        }
                    }
                    else
                    {
                        Log.Error("ProfileActivity", "nu se poate conecta la server");
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ProfileActivity Err", e.Message);
                }
            });
            return person;
        }

        private PersonView parseResultFromUrl(string res)
        {
            if (res != null)
            {
                var result = new JSONObject(res);

                string name = result.GetString("nume");
                string email = result.GetString("email");
                string birthdate = result.GetString("dataNastere");
                string gender = result.GetString("sex");
                string avatar = Utils.GetDefaults("Avatar");
                var list = new List<PersonalDisease>();

                JSONArray jsonList = result.GetJSONArray("afectiuni");
                for (var i = 0; i < jsonList.Length(); i++)
                {
                    var jsonObj = (JSONObject)jsonList.Get(i);
                    int cod = jsonObj.GetInt("id");
                    string nameDisease = jsonObj.GetString("denumire");

                    list.Add(new PersonalDisease(cod, nameDisease));
                }
                var obj = new PersonView(name, email, birthdate, gender, avatar, list, "none");
                return obj;
            }
            return null;
        }


        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                OnBackPressed();
            };
            Title = "Profilul meu";
        }

        public override void OnBackPressed()
        {
            var returnIntent = new Intent();
            SetResult(Result.Ok, returnIntent);
            base.OnBackPressed();

        }

        public async void LoadModel(bool updated)
        {
            var dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
            dialog.Show();
            PersonView person = await CallServerToGetData();
            var adapter = new DiseasesAdapter(this);

            if (person != null)
            {
                LoadModelInView(person.Avatar, person.Name, person.Email, person.Gender, person.Birthdate, updated);
                adapter = new DiseasesAdapter(this, person.ListOfPersonalDiseases);
            }
            else
            {
                personalData = await ProfileStorage.GetInstance().read();
                if (personalData == null)
                {
                    Log.Error("ProfileActivity", "data from db is null");
                    Toast.MakeText(this, "Nu există date despre profilul dumneavoastră. Incercați să vă reautentificați.", ToastLength.Long);
                }
                else
                {
                    LoadModelInView(Utils.GetDefaults("Avatar"), Utils.GetDefaults("Name"), Utils.GetDefaults("Email"), personalData.Gender, personalData.DateOfBirth, updated);
                    adapter = new DiseasesAdapter(this, personalData.listOfPersonalDiseases);
                }
            }

            FindViewById<TextView>(Resource.Id.tv_empty).Visibility = adapter.ItemCount == 0
                ? ViewStates.Visible
                : ViewStates.Gone;
            
            rv.SetAdapter(adapter);
            adapter.NotifyDataSetChanged();

          

            RunOnUiThread(() => dialog.Dismiss());
        }

        private void LoadModelInView(string avatar, string name, string email, string gender, string birthdate, bool updated)
        {

            if (updated)
            {
                ImageUpdated = DateTime.Now.Millisecond.ToString();
            }
       
            Glide.With(this)
                .Load(avatar)
                .Apply(RequestOptions.SignatureOf(new ObjectKey(ImageUpdated)))
                .Into(ciwProfileImage);

            Log.Error("ProfileActivity Glide ImgKey", ImageUpdated);


            tvName.Text = name;
            tvEmail.Text = email;
            SetGender(gender);
            if (convertDateToStringFormat(birthdate) != null)
            {
                tvDateOftBirth.Text = convertDateToStringFormat(birthdate);
            }
            else
            {
                tvDateOftBirth.Text = birthdate;
            }

            tvAge.Text = GetAge(birthdate) + " ani";


            if (int.Parse(Utils.GetDefaults("UserType")) == 2)
            {
                var rlCWAfectiuni = FindViewById<RelativeLayout>(Resource.Id.cw_diseases);
                rlCWAfectiuni.Visibility = ViewStates.Gone;
            }

        }

        private void SetGender(string gender)
        {
            if (gender.Equals("Feminin"))
            {
                iwGender.SetImageResource(Resource.Drawable.human_female);
                tvGender.Text = "Feminin";
            }
            else
            {
                iwGender.SetImageResource(Resource.Drawable.human_male);
                tvGender.Text = "Masculin";
            }
        }
        
        public int GetAge(string dateString)
        {
            
            try
            {
                var birthdate = Convert.ToDateTime(dateString);
         
                DateTime today = DateTime.Today;
                int age = today.Year - birthdate.Year;
                if (birthdate.Date > today.AddYears(-age)) age--;
               
                return age;
            }
            catch (Exception e)
            {
                Log.Error("ProfileActivity", "convert date 1: " + e.Message);

                try
                {
                    var refactor = dateString.Split("/");
                    var time = Convert.ToDateTime(refactor[1] + "/" + refactor[0] + "/" + refactor[2]);
                    dateString = time.ToString("MM/dd/yyyy");

                    var birthdate = Convert.ToDateTime(dateString);
                    DateTime today = DateTime.Today;
                    int age = today.Year - birthdate.Year;
                    if (birthdate.Date > today.AddYears(-age)) age--;

                    return age;

                }
                catch (Exception ex)
                {
                    Log.Error("ProfileActivity", "convert date 2: " + ex.Message);
                }

                return 0;
            }
        }

        public string convertDateToStringFormat(string dateString)
        {
            try
            {
                //format datetime de pe server
                var birthdate = Convert.ToDateTime(dateString);
                return birthdate.Day + "/" + birthdate.Month + "/" + birthdate.Year;
            }
            catch (Exception e)
            {
                Log.Error("ProfileActivity", "birthdate convert: " + e.Message);

                try
                {   //format zi/luna/an
                    var refactor = dateString.Split("/");
                    string dt;
                    DateTime time;
                    time = new DateTime(int.Parse(refactor[2]), int.Parse(refactor[1]), int.Parse(refactor[0]));
                    dt = time.ToString("MM/dd/yyyy");
                    return time.Day + "/" + time.Month + "/" + time.Year;

                }
                catch (Exception ex)
                {
                    Log.Error("ProfileActivity ERR", ex.Message);
                }
                return null;
            }

        }

        public string GetDate(string dateString)
        {

            try
            {
                //format datetime de pe server
                var birthdate = Convert.ToDateTime(dateString);
                return birthdate.Month + "/" + birthdate.Day + "/" + birthdate.Year;
            }
            catch (Exception e)
            {
                Log.Error("ProfileActivity", "birthdate convert: " + e.Message);

                try
                {   //format zi/luna/an
                    var refact = dateString.Split("/");
                    string dt;
                    DateTime mytime;
                    mytime = new DateTime(int.Parse(refact[2]), int.Parse(refact[1]), int.Parse(refact[0]));
                    dt = mytime.ToString("MM/dd/yyyy");
                    return mytime.Month + "/" + mytime.Day + "/" + mytime.Year;

                }
                catch (Exception ex)
                {
                    Log.Error("eProfileActivity ERR", ex.Message);
                }
                return null;
            }
        }

     

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_update:
                    var intent = new Intent(this, typeof(UpdateProfileActivity));
                    intent.PutExtra("name", tvName.Text);
                    intent.PutExtra("birthdate", tvDateOftBirth.Text);
                    intent.PutExtra("gender", tvGender.Text);
                    intent.PutExtra("avatar", Utils.GetDefaults("Avatar"));
                    StartActivityForResult(intent, 1);
                    break;
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 1)
            {
                if (resultCode == Result.Ok)
                {
                    var dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
                    dialog.Show();

                    Log.Error("ProfileActivity", "birthdate: get string extra " + data.GetStringExtra("birthdate"));

                    personalData = await ProfileStorage.GetInstance().read();
                    await RefreshAdapter();

                    if (await CallServerToSendData(personalData.Base64Image,
                        personalData.ImageName,
                        data.GetStringExtra("name"),
                        data.GetStringExtra("birthdate"),
                        data.GetStringExtra("gender"),
                        personalData.listOfPersonalDiseases))
                    {
                        
                        LoadModelInView(Utils.GetDefaults("Avatar"),
                            data.GetStringExtra("name"),
                            Utils.GetDefaults("Email"),
                            data.GetStringExtra("gender"),
                            data.GetStringExtra("birthdate"),
                            true);
                        
                        Utils.SetDefaults("Name", data.GetStringExtra("name"));
                    }
                    else
                    {
                        Toast.MakeText(this, "S-a intampinat o eroare la salvarea datelor.", ToastLength.Long).Show();
                    }
                    
                    RunOnUiThread(() => dialog.Dismiss());
                }

                if (resultCode == Result.Canceled)
                {
                    Log.Error("ProfileActivity", "cancel update");
                }
            }
        }

        private async Task RefreshAdapter()
        {
            var adapter = new DiseasesAdapter(this);
           
            if (personalData != null)
            {
                adapter = new DiseasesAdapter(this, personalData.listOfPersonalDiseases);
            }
            
            FindViewById<TextView>(Resource.Id.tv_empty).Visibility = personalData.listOfPersonalDiseases.Count == 0
                ? ViewStates.Visible
                : ViewStates.Gone;

            rv.SetAdapter(adapter);
            adapter.NotifyDataSetChanged();
        }

        async Task<bool> CallServerToSendData(string imgBase64, string imageName, string name, string birthdate, string gender, List<PersonalDisease> list)
        {
            try
            {
                Log.Error("ProfileActivity data", "start");
                if (personalData != null && personalData.listOfPersonalDiseases != null)
                {

                    var jsonArray = new JSONArray();


                    foreach (PersonalDisease item in list)
                    {
                        JSONObject disease = new JSONObject().Put("cod", item.Cod);
                        jsonArray.Put(disease);
                    }

                    string dt = GetDate(birthdate);
                    if (dt == null) {
                        dt = birthdate;
                    }

                    Log.Error("ProfileActivity data", "date: extra brth" + birthdate);
                    Log.Error("ProfileActivity data", "date: extra" + dt);
                    JSONObject jsonObject = new JSONObject()
                        .Put("Base64Image", imgBase64)
                        .Put("ImageName", imageName)
                        .Put("nume", name)
                        .Put("dataNastere", dt)
                        .Put("sex", gender)
                        .Put("afectiuni", jsonArray);
                        

                    if (Utils.CheckNetworkAvailability())
                    {
                        string result = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/myProfile", jsonObject, Utils.GetDefaults("Token"));
                        if (result != null)
                        {
                            Log.Error("ProfileActivity data", result);
                            switch (result)
                            {
                                case "Done":
                                case "done":
                                    break;
                            }

                            return true;
                        }
                        
                        Log.Error("ProfileActivity data", "res e null");
                    }
                }
                else
                {
                    Log.Error("ProfileActivity data", "list is empty");

                }
            }
            catch (Exception e)
            {
                Log.Error("ProfileActivity ERR", e.Message);
            }
            return false;
        }
    }
}