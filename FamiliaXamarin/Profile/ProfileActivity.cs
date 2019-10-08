using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Familia.Profile.Data;
using FamiliaXamarin.Helpers;
using Java.Util;
using Refractored.Controls;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Utils = FamiliaXamarin.Helpers.Utils;
using System.Threading.Tasks;
using FamiliaXamarin;
using Org.Json;

namespace Familia.Profile
{
    [Activity(Label = "Profile", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProfileActivity : AppCompatActivity, View.IOnClickListener
    {
        private PersonalData personalData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_profile);
            SetToolbar();
            FindViewById<Button>(Resource.Id.btn_update).SetOnClickListener(this);
            getData();
            CallServerToGetData(); //for test
        }


        private async void CallServerToGetData()
        {
            Log.Error("Profile SERVER", "task started");
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/myProfile", Utils.GetDefaults("Token"));
                    if (res != null)
                    {
                        Log.Error("Profile SERVER", res);

                    }
                }
                catch (Exception e)
                {
                    Log.Error("Profile SERVER ERR", e.Message);
                }
            });

        }

        async void CallServerToSendData()
        {
            try
            {


                Log.Error("Update Profile data", "start");
                if (personalData != null && personalData.listOfPersonalDiseases.Count != 0)
                {

                    JSONArray jsonArray = new JSONArray();

                    for (int i=0; i< personalData.listOfPersonalDiseases.Count - 1; i++) {
                        JSONObject disease = new JSONObject().Put("cod", personalData.listOfPersonalDiseases[i].Cod);
                        jsonArray.Put(disease);
                    }

                   

                    JSONObject jsonObject = new JSONObject()
                        .Put("imageBase64", "none")
                        .Put("nume", "Mic Patriciaa")
                        .Put("dataNastere", personalData.DateOfBirth)
                        .Put("sex", "Masculin")
                        .Put("afectiuni", jsonArray);



                    if (Utils.CheckNetworkAvailability())
                    {
                        var result = await WebServices.Post($"{Constants.PublicServerAddress}/api/myProfile", jsonObject, Utils.GetDefaults("Token"));
                        if (result != null)
                        {
                            Log.Error("Update Profile data", result);
                            switch (result)
                            {
                                case "Done":
                                case "done":
                                    break;
                            }
                        }
                        else
                        {
                            Log.Error("Update Profile data", "res e null");

                        }


                    }

                }
                else
                {
                    Log.Error("Update Profile data", "list is nempty");

                }

            }
            catch (Exception e)
            {
                Log.Error("Update Profile data ERR", e.Message);
            }
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

        public async void getData()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
            dialog.Show();
            personalData = await ProfileStorage.GetInstance().read();

            if (personalData == null)
            {
                Log.Error("ProfileStorage", "obj is null");
                dialog.Dismiss();
                Toast.MakeText(this, "Nu există date despre profilul dumneavoastră. Incercați să vă reautentificați.", ToastLength.Long);
            }
            else
            if (personalData != null)
            {

                var profileImageView = FindViewById<CircleImageView>(Resource.Id.profile_image);
                var avatar = Utils.GetDefaults("Avatar");
                Glide.With(this).Load(avatar).Into(profileImageView);
                var tvName = FindViewById<TextView>(Resource.Id.tv_name);
                tvName.Text = Utils.GetDefaults("Name");
                var tvEmail = FindViewById<TextView>(Resource.Id.tv_email);
                tvEmail.Text = Utils.GetDefaults("Email");

                var tvGender = FindViewById<TextView>(Resource.Id.tv_gender);

                if (personalData.Gender.Equals("Feminin"))
                {
                    var iwGender = FindViewById<ImageView>(Resource.Id.iw_gender);
                    iwGender.SetImageResource(Resource.Drawable.human_female);
                    tvGender.Text = "Feminin";
                }
                else
                {
                    tvGender.Text = "Masculin";
                }

                var tvDateOftBirth = FindViewById<TextView>(Resource.Id.tv_labelDate);
                tvDateOftBirth.Text = convertDateToStringFormat();

                var age = getAge();
                var tvAge = FindViewById<TextView>(Resource.Id.tv_age);
                tvAge.Text = age + " ani";

                var rv = FindViewById<RecyclerView>(Resource.Id.rv_diseases);


                var layoutManager = new LinearLayoutManager(this);
                rv.SetLayoutManager(layoutManager);

                var adapter = new DiseasesAdapter(this, personalData.listOfPersonalDiseases);
                rv.SetAdapter(adapter);
                adapter.NotifyDataSetChanged();

                RunOnUiThread(() =>
                {
                    dialog.Dismiss();
                });

                CallServerToSendData();


            }
            else
            {
                RunOnUiThread(() =>
                {
                    dialog.Dismiss();
                    Toast.MakeText(this, "Nu există date despre profilul dumneavoastră. Incercați să vă reautentificați.", ToastLength.Long);
                });
            }
        }



        public int getAge()
        {
            var birthdate = DateTime.Parse(personalData.DateOfBirth);
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public string convertDateToStringFormat()
        {
            var birthdate = DateTime.Parse(personalData.DateOfBirth);
            return birthdate.Day + "/" + birthdate.Month + "/" + birthdate.Year;
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_update:

                    //                    CallServerToSendData(); // for test

                    Intent intent = new Intent(this, typeof(UpdateProfileActivity));
                    StartActivity(intent);
                    break;
            }
        }
    }
}