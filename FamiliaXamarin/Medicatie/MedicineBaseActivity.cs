﻿using System;
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
using FamiliaXamarin.Helpers;
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
        private Android.Support.Design.Widget.BottomNavigationView bottomNavigationTip4;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MedicineFragmentBase);

            SetToolbar();

            bottomNavigation =
                FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id.bottom_navigation);

            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            bottomNavigationTip4 =
                FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id.bottom_navigation_tip4);

            bottomNavigationTip4.NavigationItemSelected += BottomNavigation_NavigationItemSelectedTip4;

            Log.Error("tip user", Utils.GetDefaults("UserType"));


            viewPager = (ViewPager)FindViewById(Resource.Id.pager);
            SharingPagerAdapter myPagerAdapter = new SharingPagerAdapter(SupportFragmentManager);

            if (int.Parse(Utils.GetDefaults("UserType")) == 3)
            {
                bottomNavigationTip4.Visibility = ViewStates.Gone;

                

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
            else
            {
                if (int.Parse(Utils.GetDefaults("UserType")) == 4)
                {

                    bottomNavigation.Visibility = ViewStates.Gone;

                  

                    myPagerAdapter.AddFragment(new MedicineLostFragment(), new String("Lost"));
                    myPagerAdapter.AddFragment(new MedicineFragment(), new String("Personal"));
                    viewPager.Adapter = myPagerAdapter;
                    viewPager.PageSelected +=
                        delegate (object sender, ViewPager.PageSelectedEventArgs args)
                        {
                            switch (args.Position)
                            {
                                case 0: bottomNavigationTip4.SelectedItemId = Resource.Id.menu_tab1; break;
                                case 1: bottomNavigationTip4.SelectedItemId = Resource.Id.menu_tab2; break;
                            }

                        };
                    viewPager.OffscreenPageLimit = 2;
                    LoadFragmentTip4(Resource.Id.menu_tab1);
                }
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
            Title = "Medicatie";
        }

        

      

        private void BottomNavigation_NavigationItemSelected(object sender,
            Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
            Log.Error("LOAD FRAGMENT", e.Item.ItemId + "");
        }

        private void BottomNavigation_NavigationItemSelectedTip4(object sender,
            Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragmentTip4(e.Item.ItemId);
            Log.Error("LOAD FRAGMENT", e.Item.ItemId + "");
        }

        void LoadFragment(int id)
        {
            switch (id)
            {
                case Resource.Id.menu_tab1:
                    viewPager.SetCurrentItem(0, true);
                    Log.Error("LOAD FRAGMENT", "tab 1");
                    break;
                case Resource.Id.menu_tab2:
                    viewPager.SetCurrentItem(1, true);

                    Log.Error("LOAD FRAGMENT", "tab 2");

                    break;
                case Resource.Id.menu_tab3:
                    viewPager.SetCurrentItem(2, true);

                    Log.Error("LOAD FRAGMENT", "tab 3");
                    break;
            }
        }

        void LoadFragmentTip4(int id)
        {
            switch (id)
            {
                case Resource.Id.menu_tab1:
                    viewPager.SetCurrentItem(0, true);
                    Log.Error("LOAD FRAGMENT", "tab 1");
                    break;
                case Resource.Id.menu_tab2:
                    viewPager.SetCurrentItem(1, true);
                    Log.Error("LOAD FRAGMENT", "tab 2");
                    break;
            }
        }


    }
}