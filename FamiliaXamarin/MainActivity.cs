using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Signature;
using Familia.Active_Conversations;
using Familia.Activity_Tracker;
using Familia.Asistenta_sociala;
using Familia.Chat;
using Familia.DataModels;
using Familia.Devices;
using Familia.Devices.DevicesAsistent;
using Familia.Devices.DevicesManagement;
using Familia.Games;
using Familia.Helpers;
using Familia.Login_System;
using Familia.Medicatie;
using Familia.Medicatie.Alarm;
using Familia.Medicatie.Data;
using Familia.Medicatie.Entities;
using Familia.Profile;
using Familia.Services;
using Familia.Settings;
using Familia.Sharing;
using Familia.WebSocket;
using Google.Android.Material.Navigation;
using Refractored.Controls;
using SQLite;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace Familia {
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener {
        Intent _loacationServiceIntent;
        Intent _webSocketServiceIntent;
        Intent _smartBandServiceIntent;
        Intent _stepCounterServiceIntent;
        CircleImageView _profileImageView;
        UsersTypes _userType;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            Log.Error("InResult", "Inainte de if");
            if (requestCode == 215) {
                if (Utils.CheckIfLocationIsEnabled()) {
                    StartForegroundService(_loacationServiceIntent);
                } else {
                    Toast.MakeText(Application.Context, "Locatie dezactivata", ToastLength.Long).Show();
                }
            }

            if (resultCode == Result.Ok) {
                if (requestCode == 466) {
                    var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                    navigationView.SetNavigationItemSelectedListener(this);
                    View headerView = navigationView.GetHeaderView(0);
                    var profileIW = headerView.FindViewById<CircleImageView>(Resource.Id.menu_profile_image);
                    string avt = Utils.GetDefaults("Avatar");

                    Glide.With(this).Load(avt)
                        .Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated))).Into(profileIW);

                    var lbName = headerView.FindViewById<TextView>(Resource.Id.lbNume);
                    lbName.Text = Utils.GetDefaults("Name");
                }
            }
        }


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (string.IsNullOrEmpty(Utils.GetDefaults("Token")) ||
                string.IsNullOrEmpty(Utils.GetDefaults("UserType"))) {
                var intent = new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
            }

            bool ok = int.TryParse(Utils.GetDefaults("UserType"), out int type);
            _userType = (UsersTypes) type;
            if (!ok) {
                _ = ClearStorage();
                StartActivity(typeof(LoginActivity));
                Finish();
                return;
            }

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            View headerView = navigationView.GetHeaderView(0);
            IMenu menuNav = navigationView.Menu;
            _profileImageView = headerView.FindViewById<CircleImageView>(Resource.Id.menu_profile_image);

            CreateNotificationsChanelsBasedOnUserType(_userType);
            LoadUiBasedonUserType(_userType, menuNav);
            StartServicesBasedOnUserType(_userType);


            var lbNume = headerView.FindViewById<TextView>(Resource.Id.lbNume);
            lbNume.Text = Utils.GetDefaults("Name");
            _profileImageView.Click += delegate {
                StartActivityForResult(new Intent(this, typeof(ProfileActivity)), 466);
            };

            if (Intent.GetBooleanExtra("FromChat", false)) {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.fragment_container, new ConversationsFragment()).AddToBackStack(null).Commit();
                Title = "Conversatii active";
            }

            if (Intent.GetBooleanExtra("FromMedicine", false)) {
                StartActivity(new Intent(this, typeof(MedicineBaseActivity)));
                Log.Error("MAIN ACTIVITY", "on back pressed");
                Title = "Medicatie";
            }

            if (Intent.GetBooleanExtra("FromSmartband", false)) {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.fragment_container, new HealthDevicesFragment()).AddToBackStack(null).Commit();
                Title = "Dispozitive de masurare";
            }

            if (Intent.HasExtra("extra_health_device")) {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.fragment_container, new HealthDevicesFragment()).AddToBackStack(null).Commit();
            }
        }

        private void CreateNotificationsChanelsBasedOnUserType(UsersTypes userType) {
            switch (userType) {
                case UsersTypes.Unknown:
                case UsersTypes.Pacient:
                case UsersTypes.SelfRegistered:
                    CreateAlarmMedicationChannel();
                    break;
                default:
                    Log.Error("User type undefined", $"User of type {userType} connot be found in the system");
                    break;
            }

            CreateSimpleChannelForServices();
            CreateNonstopChannelForServices();
        }

        private void StartServicesBasedOnUserType(UsersTypes userType) {
            _loacationServiceIntent = new Intent(this, typeof(LocationService));
            _webSocketServiceIntent = new Intent(this, typeof(WebSocketService));
            _smartBandServiceIntent = new Intent(this, typeof(SmartBandService));
            _stepCounterServiceIntent = new Intent(this, typeof(TrackerActivityService));
            //_medicationServerServiceIntent = new Intent(this, typeof(MedicationServerService));

            switch (userType) {
                case UsersTypes.Unknown:
                case UsersTypes.Pacient:
                case UsersTypes.SelfRegistered:
                    StartForegroundService(_webSocketServiceIntent);
                    if (IsActivityRecognitionPermissionGranted()) {
                        StartForegroundService(_stepCounterServiceIntent);
                    }

                    if (!string.IsNullOrEmpty(Utils.GetDefaults(GetString(Resource.String.smartband_device)))) {
                        StartForegroundService(_smartBandServiceIntent);
                    }

                    break;
                case UsersTypes.Asistent:
                case UsersTypes.Ong:
                    StartForegroundService(_webSocketServiceIntent);
                    break;
                default:
                    Log.Error("User type undefined", $"User of type {userType} connot be found in the system");
                    break;
            }

            if (!Utils.CheckIfLocationIsEnabled()) {
                _ = new AlertDialog.Builder(this).SetMessage("Locatia nu este activata").SetPositiveButton("Activare",
                    (sender, args) => {
                        StartActivityForResult(new Intent(Android.Provider.Settings.ActionLocationSourceSettings), 215);
                    }).SetNegativeButton("Anulare", (sender, args) => { }).Show();
            } else {
                StartForegroundService(_loacationServiceIntent);
            }
        }

        private void StopServicesBasedOnUserType(UsersTypes userType) {
            _loacationServiceIntent.PutExtra("IsShouldStop", true);
            _webSocketServiceIntent.PutExtra("IsShouldStop", true);
            _smartBandServiceIntent.PutExtra("IsShouldStop", true);
            _stepCounterServiceIntent.PutExtra("IsShouldStop", true);

            switch (userType) {
                case UsersTypes.Unknown:
                case UsersTypes.Pacient:
                case UsersTypes.SelfRegistered:
                    StartForegroundService(_webSocketServiceIntent);
                    StartForegroundService(_smartBandServiceIntent);
                    StartForegroundService(_stepCounterServiceIntent);
                    break;
                case UsersTypes.Asistent:
                case UsersTypes.Ong:
                    StartForegroundService(_webSocketServiceIntent);
                    break;
                default:
                    Log.Error("User type undefined", $"User of type {userType} connot be found in the system");
                    break;
            }

            StartForegroundService(_loacationServiceIntent);
        }


        private bool IsActivityRecognitionPermissionGranted() {
            //Consiliere de activitate ------
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ActivityRecognition) == Permission.Denied) {
                RequestPermissions(new string[] {Manifest.Permission.ActivityRecognition}, 2);
                Log.Error("StepCounter Permission", "DENIDED");
                return false;
            } else {
                Log.Error("StepCounter Permission", "ACCEPTED");
                return true;
            }
        }

        private void LoadUiBasedonUserType(UsersTypes userType, IMenu menuNav) {
            string avatar = Utils.GetDefaults("Avatar");
            Glide.With(this).Load(avatar).Apply(RequestOptions.SignatureOf(new ObjectKey(ProfileActivity.ImageUpdated)))
                .Into(_profileImageView);
            switch (userType) {
                case UsersTypes.Unknown:
                    menuNav.FindItem(Resource.Id.nav_asistenta).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_devices).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_monitorizare).SetVisible(false);
                    menuNav.FindItem(Resource.Id.nav_devices_asistent).SetVisible(false);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new QrCodeGenerator()).AddToBackStack(null)
                        .Commit();
                    Title = "Generare cod QR";
                    break;
                case UsersTypes.Asistent:

                    menuNav.FindItem(Resource.Id.nav_asistenta).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_monitorizare).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_devices_asistent).SetVisible(true);
                    menuNav.FindItem(Resource.Id.partajare_date).SetVisible(true);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentForm()).AddToBackStack(null).Commit();
                    Title = "Asistenta sociala";
                    break;
                case UsersTypes.Pacient:
                    menuNav.FindItem(Resource.Id.nav_QRCode).SetVisible(true);
                    menuNav.FindItem(Resource.Id.harta).SetVisible(true);
                    menuNav.FindItem(Resource.Id.chat).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_statistics).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_devices).SetVisible(true);
                    menuNav.FindItem(Resource.Id.medicatie).SetVisible(true);
                    menuNav.FindItem(Resource.Id.partajare_date).SetVisible(true);
                    menuNav.FindItem(Resource.Id.games).SetVisible(true);
                    menuNav.FindItem(Resource.Id.activity_tracker).SetVisible(true);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new HealthDevicesFragment()).AddToBackStack(null)
                        .Commit();
                    Title = "Dispozitive de masurare";
                    break;
                case UsersTypes.SelfRegistered:
                    menuNav.FindItem(Resource.Id.nav_QRCode).SetVisible(true);
                    menuNav.FindItem(Resource.Id.harta).SetVisible(true);
                    menuNav.FindItem(Resource.Id.chat).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_statistics).SetVisible(true);
                    menuNav.FindItem(Resource.Id.nav_devices).SetVisible(true);
                    menuNav.FindItem(Resource.Id.medicatie).SetVisible(true);
                    menuNav.FindItem(Resource.Id.partajare_date).SetVisible(true);
                    menuNav.FindItem(Resource.Id.activity_tracker).SetVisible(true);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new FindUsersFragment()).AddToBackStack(null)
                        .Commit();
                    Title = "Cauta prieteni";
                    break;
                case UsersTypes.Ong:
                    menuNav.FindItem(Resource.Id.nav_ong_benefits).SetVisible(true);
                    menuNav.FindItem(Resource.Id.activity_tracker).SetVisible(true);

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new OngBenefits.FragmentOngBenefits())
                        .AddToBackStack(null).Commit();
                    Title = "Acorda beneficii";
                    break;
                default:
                    Log.Error("User type undefined", $"User of type {userType} connot be found in the system");
                    break;
            }
        }


        private void CreateSimpleChannelForServices() {
            var channel = new NotificationChannel(App.SimpleChannelIdForServices, "Simple",
                NotificationImportance.Default);
            ((NotificationManager) GetSystemService(NotificationService))?.CreateNotificationChannel(channel);
            Log.Error("App CreateChannel", "Test simple channel created");
        }

        private void CreateNonstopChannelForServices() {
            var channel = new NotificationChannel(App.NonStopChannelIdForServices,
                "Nonstop", NotificationImportance.Default);
            ((NotificationManager) GetSystemService(NotificationService))?.CreateNotificationChannel(channel);
            Log.Error("App CreateChannel", "Test nonstop channel created");
        }


        private void CreateAlarmMedicationChannel() {
            Uri sound = Uri.Parse(ContentResolver.SchemeAndroidResource + "://" + Application.Context.PackageName +
                                  "/" + Resource.Raw
                                      .alarm); //Here is FILE_NAME is the name of file that you want to play
            AudioAttributes attributes = new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification)?.Build();

            var channel = new NotificationChannel(App.AlarmMedicationChannelId,
                "Alarm medication", NotificationImportance.High);

            channel.SetSound(sound, attributes);

            ((NotificationManager) GetSystemService(NotificationService))?.CreateNotificationChannel(channel);
            Log.Error("MainActivity App CreateChannel", "Test alarm medication channel created");
        }

        public override void OnBackPressed() {
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer != null && drawer.IsDrawerOpen(GravityCompat.Start)) {
                drawer.CloseDrawer(GravityCompat.Start);
                Utils.HideKeyboard(this);
            } else {
                Finish();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            base.OnCreateOptionsMenu(menu);
            return true;
        }

        public bool OnNavigationItemSelected(IMenuItem item) {
            int id = item.ItemId;
            switch (id) {
                case Resource.Id.harta:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new FindUsersFragment()).AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_devices:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new HealthDevicesFragment()).AddToBackStack(null)
                        .Commit();
                    Title = item.ToString();
                    break;

                case Resource.Id.nav_devices_asistent:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentHealthDevicesFragment())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    Toast.MakeText(this, "Devices Asistent", ToastLength.Long)?.Show();
                    break;
                case Resource.Id.medicatie:
                    StartActivity(typeof(MedicineBaseActivity));
                    break;
                case Resource.Id.chat:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new ConversationsFragment()).AddToBackStack(null)
                        .Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_manage:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new SettingsFragment()).AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.partajare_date:
                    if (int.Parse(Utils.GetDefaults("UserType")) == 2) {
                        SupportFragmentManager.BeginTransaction()
                            .Replace(Resource.Id.fragment_container, new Tab1Fragment()).AddToBackStack(null).Commit();
                        Title = item.ToString();
                    } else {
                        StartActivity(new Intent(this, typeof(SharingDataActivity)));
                    }
                    break;
                case Resource.Id.games:
                    StartActivity(new Intent(this, typeof(GameCenterActivity)));
                    break;
                case Resource.Id.activity_tracker:
                    StartActivity(new Intent(this, typeof(TrackerActivity)));
                    break;
                case Resource.Id.nav_asistenta:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new AsistentForm()).AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_monitorizare:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new MonitoringFragment()).AddToBackStack(null)
                        .Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_statistics:
                    var intent = new Intent(this, typeof(SharingMenuActivity));
                    intent.PutExtra("Name", "Masuratori personale");
                    intent.PutExtra("Email", Utils.GetDefaults("Email"));
                    intent.PutExtra("Imei", Utils.GetDeviceIdentificator(this));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_QRCode:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new QrCodeGenerator()).AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.nav_ong_benefits:
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.fragment_container, new OngBenefits.FragmentOngBenefits())
                        .AddToBackStack(null).Commit();
                    Title = item.ToString();
                    break;
                case Resource.Id.logout:

                    LogOut();
                    break;
            }

            item.SetChecked(true);
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer?.CloseDrawer(GravityCompat.Start);
            return true;
        }

        private void LogOut() {
            StopServicesBasedOnUserType(_userType);
            _ = ClearStorage();
            StartActivity(typeof(LoginActivity));

            Finish();
        }

        private async Task cancelPendingIntentsForMedicationSchedule() {
            Log.Error("MainActivity", "start canceling pending intents");
            try {
                NetworkingData networking = new NetworkingData();
                List<MedicationSchedule> list =
                    new List<MedicationSchedule>(await networking.ReadListFromDbFutureDataTask());
                var am = (AlarmManager) Application.Context.GetSystemService(AlarmService);

                foreach (MedicationSchedule item in list) {
                    Log.Error("MainActivity", "canceling pi .. " + item.IdNotification);
                    var intent = new Intent(Application.Context, typeof(AlarmBroadcastReceiverServer));
                    PendingIntent pi = PendingIntent.GetBroadcast(Application.Context, item.IdNotification, intent, 0);

                    if (pi == null) {
                        Log.Error("MainActivity", "pi is null");
                    } else {
                        Log.Error("MainActivity", "pi is not null");
                    }

                    //pi.Cancel();
                    am.Cancel(pi);
                    pi.Cancel();
                    //networking.removeMedSer(item.Uuid);
                }

                Log.Error("MainActivity", "finish canceling pending intents");
            } catch (Exception e) {
                Log.Error("MainActivity", "canceling pi ERROR" + e.Message);
            }
        }

        private async Task ClearStorage() {
            Utils.RemoveDefaults();
            await ClearBluetoothDevices();
            await ClearMedicationStorages();
            await ClearConversationsStorages();
            await Task.Run(() => { Glide.Get(this).ClearDiskCache(); });
            Glide.Get(this).ClearMemory();
        }

        private async Task ClearBluetoothDevices() {
            try {
                var sqlHelper = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                sqlHelper.DropTables(typeof(BluetoothDeviceRecords));
            } catch (Exception e) {
                Log.Error("Logout Clear Device Error", e.Message);
            }
        }

        private async Task ClearMedicationStorages() {
            try {
                await cancelPendingIntentsForMedicationSchedule();
                Log.Error("MainActivity", "start deleting db for medication");
                //var sqlHelper = await SqlHelper<MedicineServerRecords>.CreateAsync();
                //sqlHelper.DropTables(typeof(MedicineServerRecords));
            } catch (Exception e) {
                Log.Error("Logout Clear Medication Error", e.Message);
            }
        }

        private async Task ClearConversationsStorages() {
            try {
                var sqlHelper = await SqlHelper<ConversationsRecords>.CreateAsync();
                sqlHelper.DropTables(typeof(ConversationsRecords));
            } catch (Exception e) {
                Log.Error("Logout Clear Conversations Error", e.Message);
            }
        }
    }
}
