using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.OS;
using FamiliaXamarin.Sharing;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content.PM;

namespace FamiliaXamarin.Services
{
    [Activity(Label = "SharingDataActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SharingDataActivity : AppCompatActivity
    {
        private Android.Support.Design.Widget.BottomNavigationView bottomNavigation;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fragment_sharing_data);

            SetToolbar();

            bottomNavigation = FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id.bottom_navigation);

            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            // Load the first fragment on creation
            LoadFragment(Resource.Id.menu_tab1);

        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            };

            Title = "Partajare date";
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        private void BottomNavigation_NavigationItemSelected(object sender, Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
        }

        void LoadFragment(int id)
        {
            Android.Support.V4.App.Fragment fragment = null;
            switch (id)
            {
                case Resource.Id.menu_tab1:
                    fragment = new Tab1Fragment();
              
                    break;
                case Resource.Id.menu_tab2:
                    fragment = new Tab2Fragment();
                    break;

            }

            if (fragment == null)
                return;

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
        }
    }
}