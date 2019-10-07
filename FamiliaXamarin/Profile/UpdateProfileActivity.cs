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
using Android.Views;
using Android.Widget;

namespace Familia.Profile
{
    [Activity(Label = "UpdateProfileActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class UpdateProfileActivity : AppCompatActivity, View.IOnClickListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_update_profile);
            FindViewById<Button>(Resource.Id.btn_closeit).SetOnClickListener(this);
            FindViewById<Button>(Resource.Id.btn_save).SetOnClickListener(this);


        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_closeit:
                    Finish();
                    break;
                case Resource.Id.btn_save:
                    Finish();
                    break;

            }
        }
    }
}