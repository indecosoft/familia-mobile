using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie;
using Refractored.Controls;
using Square.Picasso;

namespace FamiliaXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Dark")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Intent _loacationServiceIntent;
        private Intent _webSocketServiceIntent;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
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
            //            StartService(new Intent(this, typeof(LocationService)));
            //            StartForegroundService(new Intent(this, typeof(LocationService)));
            _loacationServiceIntent = new Intent(this, typeof(LocationService));
            _webSocketServiceIntent = new Intent(this, typeof(WebSocketService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                    StartForegroundService(_loacationServiceIntent);
                    StartForegroundService(_webSocketServiceIntent);
            }
            else
            {
                    StartService(_loacationServiceIntent);
                    StartService(_webSocketServiceIntent);
            }
            //_socketClient.Connect(Constants.WebSocketAddress, Constants.WebSocketPort);
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
            var id = item.ItemId;
            return id == Resource.Id.action_settings || base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            var id = item.ItemId;

            switch (id)
            {
                case Resource.Id.harta:
                    var fragmentMap = new FindUsersFragment();
                    var fragmentManagerMap = SupportFragmentManager;
                    var fragmentTransactionMap = fragmentManagerMap.BeginTransaction();
                    fragmentTransactionMap.Replace(Resource.Id.fragment_container, fragmentMap);
                    fragmentTransactionMap.AddToBackStack(null);
                    fragmentTransactionMap.Commit();
                    break;
                case Resource.Id.nav_devices:
                    var devicesFragment = new HealthDevicesFragment();
                    var supportFragmentManager = SupportFragmentManager;
                    var beginTransaction = supportFragmentManager.BeginTransaction();
                    beginTransaction.Replace(Resource.Id.fragment_container, devicesFragment);
                    beginTransaction.AddToBackStack(null);
                    beginTransaction.Commit();
                    break;
                case Resource.Id.medicatie:
                    var medFragment = new MedicineFragment();
                    var medsupportFragmentManager = SupportFragmentManager;
                    var medbeginTransaction = medsupportFragmentManager.BeginTransaction();
                    medbeginTransaction.Replace(Resource.Id.fragment_container, medFragment);
                    medbeginTransaction.AddToBackStack(null);
                    medbeginTransaction.Commit();
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
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}

