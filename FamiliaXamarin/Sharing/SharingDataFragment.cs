﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Fragment = Android.Support.V4.App.Fragment;


namespace FamiliaXamarin.Sharing
{
   
   
    public class SharingDataFragment : FragmentActivity
    {
       
        private Android.Support.Design.Widget.BottomNavigationView bottomNavigation;
        private FrameLayout frameLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.fragment_sharing_data);
            frameLayout = FindViewById<FrameLayout>(Resource.Id.content_frame);


//            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
//            if (toolbar != null)
//            {
//                SetSupportActionBar(toolbar);
//                SupportActionBar.SetDisplayHomeAsUpEnabled(false);
//                SupportActionBar.SetHomeButtonEnabled(false);
//            }

            bottomNavigation = FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id.bottom_navigation);

            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            // Load the first fragment on creation
            LoadFragment(Resource.Id.menu_tab1);
        }

//        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
//        {
//            View view = inflater.Inflate(Resource.Layout.fragment_sharing_data, container, false);
//            return view;
//        }
        private void BottomNavigation_NavigationItemSelected(object sender, Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
        }

        void LoadFragment(int id)
        {
           Fragment fragment = null;
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