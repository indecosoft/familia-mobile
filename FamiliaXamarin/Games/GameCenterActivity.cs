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
using Android.Webkit;
using Android.Widget;
using System.IO;
using Android.Util;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Games
{
    [Activity(Label = "GameCenterActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameCenterActivity : AppCompatActivity, View.IOnClickListener
    {
      
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game_center);
            SetToolbar();

            var rvBall = FindViewById<RelativeLayout>(Resource.Id.rl_ball);
            rvBall.SetOnClickListener(this);
            
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.rl_ball:
                    StartActivity(new Intent(this, typeof(GameActivity)));
                    break;
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
            Title = "Jocuri";
        }
    }
}