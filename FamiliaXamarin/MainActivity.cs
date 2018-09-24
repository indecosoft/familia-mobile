using System;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;
using Refractored.Controls;
using Square.Picasso;
using String = System.String;

namespace FamiliaXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Dark")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

//            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
//            fab.Click += FabOnClick;

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            var headerView = navigationView.GetHeaderView(0);
            var profileImageView = headerView.FindViewById<CircleImageView>(Resource.Id.menu_profile_image);
            var avatar = Utils.GetDefaults("Avatar", this);
            Log.Error("Avatar", avatar);
            StartService(new Intent(this, typeof(LocationService)));
            Picasso.With(this)
                .Load(avatar)
                .Resize(100, 100)
                .CenterCrop()
                .Into(profileImageView);

            var lbNume = headerView.FindViewById<TextView>(Resource.Id.lbNume);
            var lbEmail = headerView.FindViewById<TextView>(Resource.Id.lbEmail);
            lbNume.Text = Utils.GetDefaults("Nume", this);
            lbEmail.Text = Utils.GetDefaults("Email", this);
            profileImageView.Click += delegate
            {
//                startActivity(new Intent(MenuActivity.this, ProfileActivity.class));
            };
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
                Utils.HideKeyboard(this);
            }
            else
            {
                Finish();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            switch (id)
            {
                case Resource.Id.harta:
                    // Handle the camera action
                    break;
                case Resource.Id.nav_devices:
                    break;
                case Resource.Id.chat:
                    break;
                case Resource.Id.nav_manage:
                    break;
                case Resource.Id.nav_asistenta:
                    var fragmentAsist = new AsistentForm();
                    var fragmentManagerAsist = SupportFragmentManager;
                    var fragmentTransactionAsist = fragmentManagerAsist.BeginTransaction();
                    fragmentTransactionAsist.Replace(Resource.Id.fragment_container, fragmentAsist);
                    fragmentTransactionAsist.AddToBackStack(null);
                    fragmentTransactionAsist.Commit();
                    break;
                case Resource.Id.nav_QRCode:
                    var fragment = new QrCodeGenerator();
                    var fragmentManager = SupportFragmentManager;
                    var fragmentTransaction = fragmentManager.BeginTransaction();
                    fragmentTransaction.Replace(Resource.Id.fragment_container, fragment);
                    fragmentTransaction.AddToBackStack(null);
                    fragmentTransaction.Commit();
                    break;
                case Resource.Id.logout:

                    Utils.SetDefaults("Token", null, this);
                    //WebSoketClientClass.mSocket.disconnect();
                    //stopService(new Intent(this, WebSoketService.class));
                    StartActivity(typeof(LoginActivity));
                    Finish();
                    break;
            }
            // Highlight the selected item has been done by NavigationView
            item.SetChecked(true);
            // Set action bar title
            Title = item.ToString();
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}

