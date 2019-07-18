using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.OS;
using FamiliaXamarin.Sharing;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content.PM;
using Android.Support.V4.View;
using Android.Util;
using Familia;
using FamiliaXamarin;
using FamiliaXamarin.Medicatie;
using String = Java.Lang.String;

namespace Familia.Medicatie
{

    [Activity(Label = "MedicineBaseActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    class MedicineBaseActivity : AppCompatActivity
    {
        private ViewPager viewPager;
        private Android.Support.Design.Widget.BottomNavigationView bottomNavigation;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MedicineFragmentBase);

            SetToolbar();

            bottomNavigation =
                FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id.bottom_navigation);

            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            viewPager = (ViewPager)FindViewById(Resource.Id.pager);
            SharingPagerAdapter myPagerAdapter = new SharingPagerAdapter(SupportFragmentManager);
            myPagerAdapter.AddFragment(new MedicineLostFragment(), new String("Lost"));
            myPagerAdapter.AddFragment(new MedicineServerFragment(), new String("Current"));
            myPagerAdapter.AddFragment(new MedicineFragment(), new String("Personal"));
            viewPager.Adapter = myPagerAdapter;
            viewPager.PageSelected +=
                delegate (object sender, ViewPager.PageSelectedEventArgs args)
                {
                    switch (args.Position)
                    {
                        case 0: bottomNavigation.SelectedItemId = Resource.Id.menu_tab1; break;
                        case 1: bottomNavigation.SelectedItemId = Resource.Id.menu_tab2; break;
                        case 2: bottomNavigation.SelectedItemId = Resource.Id.menu_tab3; break;
                    }
                    
                };
            viewPager.OffscreenPageLimit = 3;
            LoadFragment(Resource.Id.menu_tab2);
        }


        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                //                var intent = new Intent(this, typeof(MainActivity));
                //                intent.AddFlags(ActivityFlags.ClearTop);
                //                StartActivity(intent);
                OnBackPressed();
            };
            Title = "Medicatie";
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            //var intent = new Intent(this, typeof(MainActivity));
            //intent.AddFlags(ActivityFlags.ClearTop);
            //StartActivity(intent);
        }

        private void BottomNavigation_NavigationItemSelected(object sender,
            Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
            Log.Error("LOAD FRAGMENT", e.Item.ItemId + "");

        }

        void LoadFragment(int id)
        {
            switch (id)
            {
                case Resource.Id.menu_tab1:
//                    viewPager.CurrentItem = 0;
                    viewPager.SetCurrentItem(0, true);
                    Log.Error("LOAD FRAGMENT", "tab 1");
                    break;
                case Resource.Id.menu_tab2:
//                    viewPager.CurrentItem = 1;
                    viewPager.SetCurrentItem(1, true);

                    Log.Error("LOAD FRAGMENT", "tab 2");

                    break;
                case Resource.Id.menu_tab3:
//                    viewPager.CurrentItem = 2;
                    viewPager.SetCurrentItem(2, true);

                    Log.Error("LOAD FRAGMENT", "tab 3");
                    break;
            }
        }


    }
}