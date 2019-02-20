using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Active_Conversations;
using FamiliaXamarin.Asistenta_sociala;
using FamiliaXamarin.Chat;
using FamiliaXamarin.Devices;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Login_System;
using FamiliaXamarin.Medicatie;
using FamiliaXamarin.Services;
using FamiliaXamarin.Settings;
using Refractored.Controls;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Android.Util;
using Com.Bumptech.Glide;
using FamiliaXamarin.Location;
using FamiliaXamarin.Sharing;
using Org.Json;

namespace FamiliaXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        Intent _loacationServiceIntent;
        Intent _webSocketServiceIntent;
//        Intent _medicationServiceIntent;
        Intent _smartBandServiceIntent;
        private FusedLocationProviderClient _fusedLocationProviderClient;
        //public static bool FromDisease;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            var headerView = navigationView.GetHeaderView(0);
            var profileImageView = headerView.FindViewById<CircleImageView>(Resource.Id.menu_profile_image);
            var avatar = Utils.GetDefaults("Avatar", this);

            _loacationServiceIntent = new Intent(this, typeof(LocationService));
            _webSocketServiceIntent = new Intent(this, typeof(WebSocketService));
            _smartBandServiceIntent = new Intent(this, typeof(SmartBandService));
            //_medicationServiceIntent = new Intent(this, typeof(MedicationService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Context ctx = ApplicationContext;
                StartForegroundService(_loacationServiceIntent);
                StartForegroundService(_webSocketServiceIntent);
               // StartForegroundService(_smartBandServiceIntent);
                   // StartForegroundService(_medicationServiceIntent);
                
            }
            else
            {
                    StartService(_loacationServiceIntent);
                    StartService(_webSocketServiceIntent);
                    //StartService(_smartBandServiceIntent);
                   // StartService(_medicationServiceIntent);

            }

            Glide.With(this).Load(avatar).Into(profileImageView);

            var lbNume = headerView.FindViewById<TextView>(Resource.Id.lbNume);
            //var lbEmail = headerView.FindViewById<TextView>(Resource.Id.lbEmail);
            lbNume.Text = Utils.GetDefaults("Name", this);
            //lbEmail.Text = Utils.GetDefaults("Email", this);
            profileImageView.Click += delegate
            {
                //TODO: Implementateaza acivitaste pentru profil 
            };

             if (Intent.GetBooleanExtra("FromChat", false))
             {
                 SupportFragmentManager.BeginTransaction()
                     .Replace(Resource.Id.fragment_container, new ConversationsFragment())
                     .AddToBackStack(null).Commit();
                 Title = "Conversatii active";
             }
            //_isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(this);
            if (!Utils.IsGooglePlayServicesInstalled(this)) return;
            new LocationRequest()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(1000)
                .SetFastestInterval(1000);
            new FusedLocationProviderCallback(this);

            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            // Utils.CreateNotificationChannel();
            GetLastLocationButtonOnClick();

            if (Intent.HasExtra("extra_health_device"))
            {
                OpenHealthDeviceFragment();
            }

        }

        private async void GetLastLocationButtonOnClick()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
        }

        private async Task GetLastLocationFromDevice()
        {
            var location = await _fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // Seldom happens, but should code that handles this scenario
                Log.Error("Location is null", "******************");
            }
            else
            {
                Log.Debug("Sample", "The Latitude is " + location.Latitude);
                Log.Debug("Sample", "The Longitude is " + location.Longitude);
                JSONObject obj = new JSONObject().Put("latitude", (double) location.Latitude).Put("longitude", (double) location.Longitude);
                JSONObject finalObj = new JSONObject().Put("idUser", Utils.GetDefaults("IdClient", this)).Put("location", obj);
                try
                {
                    await Task.Run(async () =>
                    {
                        string p = await WebServices.Post(Constants.PublicServerAddress + "/api/updateLocation", finalObj, Utils.GetDefaults("Token", this));
                        Log.Debug("Latitude ", location.Latitude.ToString());
                        Log.Debug("Longitude", location.Longitude.ToString());
                    });
                }
                catch (Exception e)
                {
                    Log.Error("****************************", e.Message);
                }

            }
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
            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            var id = item.ItemId;

            switch (id)
            {
                case Resource.Id.harta:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new FindUsersFragment())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.nav_devices:
                    OpenHealthDeviceFragment();
                    break;
                case Resource.Id.medicatie:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new MedicineFragment())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.chat:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new ConversationsFragment())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.nav_manage:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new SettingsFragment())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.partajare_date:
                   
                    StartActivity(new Intent(this, typeof(SharingDataActivity)));

                    break;
                case Resource.Id.nav_asistenta:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentForm())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.nav_monitorizare:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new MonitoringFragment())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.nav_QRCode:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new QrCodeGenerator())
                        .AddToBackStack(null).Commit();
                    break;
                case Resource.Id.logout:

                    Utils.RemoveDefaults();
                    WebSocketClient.Disconect();
                    //Process.KillProcess(Process.MyPid());
                        StopService(_loacationServiceIntent);
                        StopService(_webSocketServiceIntent);
                       // StopService(_medicationServiceIntent);
   
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

        private void OpenHealthDeviceFragment()
        {
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, new HealthDevicesFragment())
                .AddToBackStack(null).Commit();
        }
    }
}



