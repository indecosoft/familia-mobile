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
using FamiliaXamarin.Helpers;
using Refractored.Controls;

namespace Familia.Profile
{
    [Activity(Label = "UpdateProfileActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdateProfileActivity : AppCompatActivity, View.IOnClickListener
    {
        private PersonalData personalData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_update_profile);
            FindViewById<Button>(Resource.Id.btn_closeit).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.btn_save).SetOnClickListener(this);

            GetData();

        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_closeit:
                    Finish();
                    break;
                case Resource.Id.btn_save:
                    Log.Error("UpdateProfileActivity", "saving..");
                    Log.Error("UpdateProfileActivity", FindViewById<EditText>(Resource.Id.et_name).Text);
                    Log.Error("UpdateProfileActivity", FindViewById<TextView>(Resource.Id.tv_birthdate).Text);

                    if (FindViewById<RadioButton>(Resource.Id.rb_female).Checked == true)
                    {
                        Log.Error("UpdateProfileActivity", "Feminin");
                    }

                    if (FindViewById<RadioButton>(Resource.Id.rb_male).Checked == true)
                    {
                        Log.Error("UpdateProfileActivity", "Masculin");
                    }


                    Finish();
                    break;

            }
        }


        public async void GetData()
        {
            ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", this, false);
            dialog.Show();
            personalData = await ProfileStorage.GetInstance().read();

            var profileImageView = FindViewById<CircleImageView>(Resource.Id.profile_image);
            var avatar = Utils.GetDefaults("Avatar");
            Glide.With(this).Load(avatar).Into(profileImageView);

            FindViewById<EditText>(Resource.Id.et_name).Text = Utils.GetDefaults("Name");
            FindViewById<TextView>(Resource.Id.tv_birthdate).Text = personalData.DateOfBirth;


            if (personalData.Gender.Equals("Feminin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_female).Checked = true;
            }

            if (personalData.Gender.Equals("Masculin"))
            {
                FindViewById<RadioButton>(Resource.Id.rb_male).Checked = true;
            }

            


            dialog.Dismiss();
        }
    }
}