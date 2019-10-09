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
using Android.Provider;
using FamiliaXamarin;
using Org.Json;


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
        private TextView tvBirtdate;
        private TextView tvGender;
        private TextView tvDateOftBirth;
        private TextView tvAge;
        private ImageView iwGender;
        private RecyclerView rv;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_profile);
            SetToolbar();
            InitView();
            LoadModel();
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

           //TODO hide "afectiuni" for correct type of user
        }


        private async Task<PersonView> CallServerToGetData()
        {
            Log.Error("Profile SERVER", "task started");
            PersonView person = null;
            await Task.Run(async () =>
            {
                try
                {
                    var res = await WebServices.Get($"{Constants.PublicServerAddress}/api/myProfile", Utils.GetDefaults("Token"));
                    if (res != null)
                    {
                        Log.Error("ProfileActivity", res);
                        person = parseResultFromUrl(res);
                        if (person != null && person.ListOfPersonalDiseases.Count !=0)
                        {
                            await ProfileStorage.GetInstance().saveDiseases(person.ListOfPersonalDiseases);
                        }
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

                var name = result.GetString("nume");
                var email = result.GetString("email");
                var birthdate = result.GetString("dataNastere");
                var gender = result.GetString("sex");
                var avatar = Utils.GetDefaults("Avatar");
                var list = new List<PersonalDisease>();

                JSONArray jsonList = result.GetJSONArray("afectiuni");
                for (var i = 0; i < jsonList.Length(); i++)
                {
                    var jsonObj = (JSONObject)jsonList.Get(i);
                    var cod = jsonObj.GetInt("id");
                    var nameDisease = jsonObj.GetString("denumire");

                    list.Add(new PersonalDisease(cod, nameDisease));
                }
                var obj = new PersonView(name, email, birthdate, gender, avatar, list);
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

        public async void LoadModel()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
            dialog.Show();
            var person = await CallServerToGetData();
            DiseasesAdapter adapter = new DiseasesAdapter(this);

            if (person != null)
            {
                LoadModelInView(person.Avatar, person.Name, person.Email, person.Gender, person.Birthdate);
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
                    LoadModelInView(Utils.GetDefaults("Avatar"), Utils.GetDefaults("Name"), Utils.GetDefaults("Email"), personalData.Gender, personalData.DateOfBirth);
                    adapter = new DiseasesAdapter(this, personalData.listOfPersonalDiseases);
                }
            }

            rv.SetAdapter(adapter);
            adapter.NotifyDataSetChanged();
            RunOnUiThread(() => dialog.Dismiss());
        }

        private void LoadModelInView(string avatar, string name, string email, string gender, string birthdate)
        {
            Glide.With(this).Load(avatar).Into(ciwProfileImage);
            tvName.Text = name;
            tvEmail.Text = email;
            SetGender(gender);
            tvDateOftBirth.Text = convertDateToStringFormat(birthdate);
            tvAge.Text = getAge(birthdate) + " ani";
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
                tvGender.Text = "Masculin";
            }
        }

       


        public int getAge(string dateString)
        {
            var birthdate = DateTime.Parse(dateString);
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public string convertDateToStringFormat(string dateString)
        {
            var birthdate = DateTime.Parse(dateString);
            return birthdate.Day + "/" + birthdate.Month + "/" + birthdate.Year;
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_update:
                    Intent intent = new Intent(this, typeof(UpdateProfileActivity));
                    intent.PutExtra("name", tvName.Text);
                    intent.PutExtra("birthdate", tvDateOftBirth.Text);
                    intent.PutExtra("gender", tvGender.Text);
                    intent.PutExtra("avatar", Utils.GetDefaults("Avatar"));
                    StartActivityForResult(intent, 1);
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            Log.Error("ProfileActivity", "requestCode: " + requestCode);

            if (requestCode == 1)
            {
                if (resultCode == Result.Ok)
                {
                    Log.Error("ProfileActivity", "result updated: " + data.GetStringExtra("result"));
                }

                if (resultCode == Result.Ok)
                {
                    Log.Error("ProfileActivity", "cancel update");

                }
            }
        }
    }
}