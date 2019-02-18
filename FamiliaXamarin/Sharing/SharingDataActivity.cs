using System;
using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.OS;
using FamiliaXamarin.Sharing;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content.PM;
using Android.Support.V4.View;
using String = Java.Lang.String;


namespace FamiliaXamarin.Services
{
    [Activity(Label = "SharingDataActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SharingDataActivity : AppCompatActivity
    {
        private ViewPager viewPager;
        private Android.Support.Design.Widget.BottomNavigationView bottomNavigation;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fragment_sharing_data);

            SetToolbar();

            bottomNavigation =
                FindViewById<Android.Support.Design.Widget.BottomNavigationView>(Resource.Id
                    .bottom_navigation);

            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            viewPager = (ViewPager) FindViewById(Resource.Id.pager);
            SharingPagerAdapter myPagerAdapter = new SharingPagerAdapter(SupportFragmentManager);
            myPagerAdapter.AddFragment(new Tab1Fragment(), new String("Cauta persoana"));
            myPagerAdapter.AddFragment(new Tab2Fragment(), new String("Lista conexiuni"));
            viewPager.Adapter = myPagerAdapter;
            viewPager.PageSelected +=
                delegate(object sender, ViewPager.PageSelectedEventArgs args)
                {
                    if (args.Position == 0)
                    {
                        bottomNavigation.SelectedItemId = Resource.Id.menu_tab1;
                    }
                    else
                    {
                        bottomNavigation.SelectedItemId = Resource.Id.menu_tab2;
                    }
                };
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
            //intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        private void BottomNavigation_NavigationItemSelected(object sender,
            Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            LoadFragment(e.Item.ItemId);
        }

        void LoadFragment(int id)
        {
            switch (id)
            {
                case Resource.Id.menu_tab1:
                    viewPager.CurrentItem = 0;
                    break;
                case Resource.Id.menu_tab2:
                    viewPager.CurrentItem = 1;
                    break;
            }
        }
    }
}