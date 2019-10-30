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
using Android.Locations;
using Android.Support.V4.Content;
using Android.Telephony;
using Android.Util;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Signature;
using Familia.Active_Conversations;
using Familia.Chat;
using Familia.DataModels;
using Familia.Games;
using Familia.Login_System;
using Familia.Medicatie;
using Familia.Profile;
using Familia.Services;
using Familia.Sharing;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Location;
using FamiliaXamarin.Sharing;
using Org.Json;
using AlertDialog = Android.App.AlertDialog;
using Resource = Familia.Resource;
using Familia.WebSocket;

namespace FamiliaXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener 
    {
        Intent _loacationServiceIntent;
        Intent _webSocketServiceIntent;
        Intent _medicationServiceIntent;
        Intent _smartBandServiceIntent;
        Intent _medicationServerServiceIntent;
        private IMenu _menu;
        private FusedLocationProviderClient _fusedLocationProviderClient;
        //public static bool FromDisease;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Log.Error("InResult", "Inainte de if");
            if (requestCode == 215)
            {
                if (Utils.CheckIfLocationIsEnabled())
                {
                    StartForegroundService(_loacationServiceIntent);
                    if(int.Parse(Utils.GetDefaults("UserType")) == 4 || int.Parse(Utils.GetDefaults("UserType")) == 3)
                        StartForegroundService(_smartBandServiceIntent);
                }
                else
                {
                    Toast.MakeText(Application.Context, "Locatie dezactivata", ToastLength.Long).Show();
                }
                
            }

            if (resultCode == Result.Ok)
            {
                if (requestCode == 466)
                {
                    var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                    navigationView.SetNavigationItemSelectedListener(this);
                    var headerView = navigationView.GetHeaderView(0);
                    var profileIW = headerView.FindViewById<CircleImageView>(Resource.Id.menu_profile_image);
                    var avt = Utils.GetDefaults("Avatar");

                    Glide.With(this)
                        .Load(avt)
                        .Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated)))
                        .Into(profileIW);

                    var lbName = headerView.FindViewById<TextView>(Resource.Id.lbNume);
                    lbName.Text = Utils.GetDefaults("Name");
                }
            }
        }
            

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            
            if (string.IsNullOrEmpty(Utils.GetDefaults("Token")) || string.IsNullOrEmpty(Utils.GetDefaults("UserType")))
            {    var intent = new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
                
            }
            bool ok = int.TryParse(Utils.GetDefaults("UserType"), out var type);
            if (!ok)
            {
                Utils.RemoveDefaults();
                ClearBluetoothDevices();
                ClearMedicationStorages();
                Task.Run(() =>
                {
                    Glide.Get(this).ClearDiskCache();
                    //
                });
                Glide.Get(this).ClearMemory();
                    
                StartActivity(typeof(LoginActivity));
                Finish();
                return;
            }
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
            var avatar = Utils.GetDefaults("Avatar");

            _loacationServiceIntent = new Intent(this, typeof(LocationService));
            _webSocketServiceIntent = new Intent(this, typeof(WebSocketService));
            _smartBandServiceIntent = new Intent(this, typeof(SmartBandService));
            _medicationServerServiceIntent = new Intent(this, typeof(MedicationServerService));
            _medicationServiceIntent = new Intent(this, typeof(MedicationService));
            var menuNav = navigationView.Menu;


//            make it hidden for release bc is not done yet
            menuNav.FindItem(Resource.Id.games).SetVisible(false);


            switch (type)
            {
                case 1:
                    Toast.MakeText(this, "1", ToastLength.Long).Show();
                    menuNav.FindItem(Resource.Id.nav_asistenta).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_devices).SetVisible(false);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new QrCodeGenerator())
                        .AddToBackStack(null).Commit();
                    Title = "Generare cod QR";

                    StartForegroundService(_webSocketServiceIntent);
                    StartService(_medicationServerServiceIntent);
                    StartService(_medicationServiceIntent);


                    break;
                case 2:
                    Toast.MakeText(this, "2", ToastLength.Long).Show();
                    menuNav.FindItem(Resource.Id.nav_monitorizare).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_QRCode).SetVisible(false);
                    menuNav.FindItem(Resource.Id.harta).SetVisible(false);
                    menuNav.FindItem(Resource.Id.chat).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_devices).SetVisible(false);
                    menuNav.FindItem(Resource.Id.medicatie).SetVisible(false);
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentForm())
                        .AddToBackStack(null).Commit();
                    Title = "Asistenta sociala";
                    StartForegroundService(_webSocketServiceIntent);
                    break;
                case 3:
                    Toast.MakeText(this, "3", ToastLength.Long).Show();
                    menuNav.FindItem(Resource.Id.nav_asistenta).SetVisible(false);
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new HealthDevicesFragment())
                        .AddToBackStack(null).Commit();
                    Title = "Dispozitive de masurare";
                    StartForegroundService(_webSocketServiceIntent);
                    StartService(_medicationServerServiceIntent);
                    StartService(_medicationServiceIntent);
                    //commet for push

                    break;
                case 4:
                    Toast.MakeText(this, "4", ToastLength.Long).Show();
                    menuNav.FindItem(Resource.Id.nav_asistenta).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_monitorizare)?.SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_QRCode)?.SetVisible(false);
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new FindUsersFragment())
                        .AddToBackStack(null).Commit();
                    Title = "Cauta prieteni";
                    StartForegroundService(_webSocketServiceIntent);
                    StartService(_medicationServerServiceIntent);
                    StartService(_medicationServiceIntent);


                    break;
            }

            if (!Utils.CheckIfLocationIsEnabled())
            {
                new AlertDialog.Builder(this)
                    .SetMessage("Locatia nu este activata")
                    .SetPositiveButton("Activare", (sender, args) =>
                    {
                        StartActivityForResult(new Intent(Android.Provider.Settings.ActionLocationSourceSettings), 215);
                    })
                    .SetNegativeButton("Anulare", (sender, args) => { })
                    .Show();
            }
            else
            {
                StartForegroundService(_loacationServiceIntent);
                if (int.Parse(Utils.GetDefaults("UserType")) == 4 || int.Parse(Utils.GetDefaults("UserType")) == 3)
                    StartForegroundService(_smartBandServiceIntent);
            }


            // StartForegroundService(_loacationServiceIntent);

            //StartForegroundService(_smartBandServiceIntent);
            // StartForegroundService(_medicationServiceIntent);

            Log.Error("LoginActivity Glide ImgKey", ProfileActivity.ImageUpdated);
   
            Glide.With(this)
                .Load(avatar)
                .Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated)))
                .Into(profileImageView);

            var lbNume = headerView.FindViewById<TextView>(Resource.Id.lbNume);
            //var lbEmail = headerView.FindViewById<TextView>(Resource.Id.lbEmail);
            lbNume.Text = Utils.GetDefaults("Name");
            //lbEmail.Text = Utils.GetDefaults("Email", this);
            profileImageView.Click += delegate
            {
                StartActivityForResult(new Intent(this, typeof(ProfileActivity)), 466);
            };

             if (Intent.GetBooleanExtra("FromChat", false))
             {
                 SupportFragmentManager.BeginTransaction()
                     .Replace(Resource.Id.fragment_container, new ConversationsFragment())
                     .AddToBackStack(null).Commit();
                 Title = "Conversatii active";
             }
             if (Intent.GetBooleanExtra("FromMedicine", false))
             {
                 StartActivity(new Intent(this, typeof(MedicineBaseActivity)));
                 Log.Error("MAIN ACTIVITY", "on back pressed");
                Title = "Medicatie";
             }
             if (Intent.GetBooleanExtra("FromSmartband", false))
             {
                 SupportFragmentManager.BeginTransaction()
                     .Replace(Resource.Id.fragment_container, new HealthDevicesFragment())
                     .AddToBackStack(null).Commit();
                 Title = "Dispozitive de masurare";
             }
            //_isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(this);
//            if (!Utils.IsGooglePlayServicesInstalled(this)) return;
//            new LocationRequest()
//                .SetPriority(LocationRequest.PriorityHighAccuracy)
//                .SetInterval(1000)
//                .SetFastestInterval(1000);
//            new FusedLocationProviderCallback(this);
//
//            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            // Utils.CreateNotificationChannel();
            //GetLastLocationButtonOnClick();

            if (Intent.HasExtra("extra_health_device"))
            {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.fragment_container, new HealthDevicesFragment())
                    .AddToBackStack(null).Commit();
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
                JSONObject finalObj = new JSONObject().Put("idUser", Utils.GetDefaults("IdClient")).Put("location", obj);
                try
                {
                    await Task.Run(async () =>
                    {
                        await WebServices.Post(Constants.PublicServerAddress + "/api/updateLocation", finalObj, Utils.GetDefaults("Token"));
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
            if (drawer.IsDrawerOpen(GravityCompat.Start))
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
            base.OnCreateOptionsMenu(menu);
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
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_devices:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new HealthDevicesFragment())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.medicatie:
                    /*
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new MedicineFragment())
                        .AddToBackStack(null).Commit();*/
                    StartActivity(typeof(MedicineBaseActivity));
                    break;
                case Resource.Id.chat:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new ConversationsFragment())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_manage:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new SettingsFragment())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.partajare_date:
                   
                    StartActivity(new Intent(this, typeof(SharingDataActivity)));

                    break;
                case Resource.Id.games:
                    StartActivity(new Intent(this, typeof(GameCenterActivity)));
                    break;
                case Resource.Id.nav_asistenta:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentForm())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_monitorizare:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new MonitoringFragment())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_QRCode:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new QrCodeGenerator())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.logout:

                    Utils.RemoveDefaults();
                    WebSocketClient.Disconect();
                    //Process.KillProcess(Process.MyPid());
                        StopService(_loacationServiceIntent);
                        StopService(_webSocketServiceIntent);
                        StopService(_medicationServerServiceIntent);
                        StopService(_smartBandServiceIntent);
                       // StopService(_medicationServiceIntent);
                    ClearBluetoothDevices();
                    ClearMedicationStorages();
                    ClearConversationsStorages();
                    Task.Run(() =>
                    {
                        Glide.Get(this).ClearDiskCache();
                        //
                    });
                    Glide.Get(this).ClearMemory();
                    
                    StartActivity(typeof(LoginActivity));
                    Finish();
                    break;
            }
            // Highlight the selected item has been done by NavigationView
            item.SetChecked(true);
            // Set action bar title
            
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        private async void ClearBluetoothDevices()
        {
            try
            {
                var sqlHelper = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                sqlHelper.DropTables(typeof(BluetoothDeviceRecords));
            }
            catch (Exception e)
            {
                Log.Error("Logout Clear Device Error", e.Message);
            }
            
        } 
        private async void ClearMedicationStorages()
        {
            try
            {
                var sqlHelper = await SqlHelper<MedicineServerRecords>.CreateAsync();
                sqlHelper.DropTables(typeof(MedicineServerRecords));
            }
            catch (Exception e)
            {
                Log.Error("Logout Clear Medication Error", e.Message);
            }
            
        }
        private async void ClearConversationsStorages()
        {
            try
            {
                var sqlHelper = await SqlHelper<ConversationsRecords>.CreateAsync();
                sqlHelper.DropTables(typeof(ConversationsRecords));
            }
            catch (Exception e)
            {
                Log.Error("Logout Clear Conversations Error", e.Message);
            }

        }
    }
}



