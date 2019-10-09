using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Familia.Profile.Data;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Medicatie;
using Org.Json;
using Refractored.Controls;
using Exception = System.Exception;

namespace Familia.Profile
{
    [Activity(Label = "UpdateProfileActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdateProfileActivity : AppCompatActivity, View.IOnClickListener
    {

        private PersonalData personalData;
        private PersonView personView;
        private CircleImageView ciwProfileImage;
        private TextView tvBirthDate;
        private TextView tvName;
        private EditText etName;
        private TextView tvBirthdate;
        private string gender;
        private string name;
        private string birthdate;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_update_profile);
            FindViewById<Button>(Resource.Id.btn_closeit).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.btn_save).SetOnClickListener(this);

            ciwProfileImage = FindViewById<CircleImageView>(Resource.Id.profile_image);
            tvName = FindViewById<EditText>(Resource.Id.et_name);
            tvBirthDate = FindViewById<TextView>(Resource.Id.tv_birthdate);
            tvBirthDate.SetOnClickListener(this);
            etName = FindViewById<EditText>(Resource.Id.et_name);

            personView = new PersonView(Intent.GetStringExtra("name"),
                                        Intent.GetStringExtra("email"),
                                        Intent.GetStringExtra("birthdate"),
                                        Intent.GetStringExtra("gender"),
                                        Intent.GetStringExtra("avatar"),
                                        null);

            LoadModel();

        }

        public void OnClick(View v)
        {
            Intent returnIntent = new Intent();

            switch (v.Id)
            {
                case Resource.Id.btn_closeit:
                    returnIntent = new Intent();
                    SetResult(Result.Canceled, returnIntent);
                    Finish();
                    break;
                case Resource.Id.btn_save:
                    Log.Error("UpdateProfileActivity", "saving..");
                    // TODO photo
                    name = tvName.Text;
                    birthdate = tvBirthDate.Text;

                    if (FindViewById<RadioButton>(Resource.Id.rb_female).Checked == true)
                    {
                        gender = "Feminin";
                    }

                    if (FindViewById<RadioButton>(Resource.Id.rb_male).Checked == true)
                    {
                        gender = "Masculin";
                    }

                    returnIntent = new Intent();
                    returnIntent.PutExtra("result", "here i am from update");
                    SetResult(Result.Ok, returnIntent);

//                    CallServerToSendData();
                    Finish();
                    break;

                case Resource.Id.tv_birthdate:
                    OnDateClick();
                    break;

            }
        }

        async void CallServerToSendData()
        {
            try
            {
                Log.Error("Update Profile data", "start");
                if (personalData != null && personalData.listOfPersonalDiseases.Count != 0)
                {

                    JSONArray jsonArray = new JSONArray();

                    for (int i = 0; i < personalData.listOfPersonalDiseases.Count - 1; i++)
                    {
                        JSONObject disease = new JSONObject().Put("cod", personalData.listOfPersonalDiseases[i].Cod);
                        jsonArray.Put(disease);
                    }

                    JSONObject jsonObject = new JSONObject()
                        .Put("Base64Image", personalData.Base64Image)
                        .Put("ImageName", "patricia.mic@indecosoft.ro") //from get: avatar
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
                Log.Error("UpdateProfileActivity ERR", e.Message);
            }
        }


        private void OnDateClick()
        {
            var frag = DatePickerMedicine.NewInstance(delegate (DateTime time)
            {
                tvBirthDate.Text = time.ToString("dd/MM/yyyy");
            });
            frag.Show(this.SupportFragmentManager, DatePickerMedicine.TAG);
        }


        public async void LoadModel()
        {
            try
            {
                ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
                dialog.Show();

                personalData = await ProfileStorage.GetInstance().read();
                Log.Error("UpdateProfileActivity", "diseases count: ", personalData.listOfPersonalDiseases.Count);

                Glide.With(this).Load(personView.Avatar).Into(ciwProfileImage);
                etName.Text = personView.Name;
                tvBirthDate.Text = personView.Birthdate;
                SetGender(personView.Gender);

                RunOnUiThread(() => dialog.Dismiss());

            }
            catch (Exception e)
            {
                Log.Error("UpdateProfileActivity ERR", e.Message);
            }

            
        }

        private void SetGender(string gender)
        {
            if (gender.Equals("Feminin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_female).Checked = true;
            }

            if (gender.Equals("Masculin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_male).Checked = true;
            }
        }
    }
}