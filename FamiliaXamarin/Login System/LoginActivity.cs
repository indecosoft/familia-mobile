using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Fingerprints;
using Android.OS;
using Android.Security.Keystore;
using Android.Util;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Familia.Helpers;
using Familia.Devices.Alarm;
using Familia.Services;
using Java.Security;
using Javax.Crypto;
using Org.Json;
using Permission = Android.Content.PM.Permission;
using Google.Android.Material.Snackbar;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Core.Content;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Familia.Login_System {
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark", MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : AppCompatActivity {
        private ConstraintLayout _layout;

        private EditText _usernameEditText, _passwordEditText;
        private AppCompatButton _loginButton, _registerButton;
        private TextView _pwdResetTextView;
        private KeyStore _keyStore;
        private Cipher _cipher;
        private readonly string _keyName = "EDMTDev";
        private ProgressBarDialog _progressBarDialog;
        private Button _pinButton;
        private LottieAnimationView _animationView;


        protected override void OnResume() {
            base.OnResume();
            if (IsAuthWithFingerprintEnabled()) {
                AuthWithFingerprint();
            } else {
                if (IsAuthWithPin()) {
                    StartActivity(typeof(PinActivity));
                } else {
                    LoadLoginUi();
                }
            }
        }


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            if (IsAuthWithFingerprintEnabled()) {
                AuthWithFingerprint();
            } else {
                if (IsAuthWithPin()) {
                    StartActivity(typeof(PinActivity));
                } else {
                    LoadLoginUi();
                }
            }
        }

        private static bool IsAuthWithFingerprintEnabled() =>
            !string.IsNullOrEmpty(Utils.GetDefaults("fingerprint")) &&
            Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

        private static bool IsAuthWithPin() => !string.IsNullOrEmpty(Utils.GetDefaults("UserPin"));

        private void AuthWithFingerprint() {
            SetContentView(Resource.Layout.activity_finger);
            InitFingerprintUi();
            InitFingerprintListeners();
            var filter = new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            //Using the Android Support Library v4
            var keyguardManager = (KeyguardManager) GetSystemService(KeyguardService);
            var fingerprintManager = (FingerprintManager) GetSystemService(FingerprintService);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.UseFingerprint) !=
                (int) Permission.Granted)
                return;
            if (fingerprintManager != null && !fingerprintManager.IsHardwareDetected) {
                Toast.MakeText(this,
                    "Nu exista permisiuni pentru autentificare utilizand amprenta",
                    ToastLength.Long)?.Show();
                LoadLoginUi();
            } else {
                if (fingerprintManager != null && !fingerprintManager.HasEnrolledFingerprints) {
                    Toast.MakeText(this,
                        "Nu ati inregistrat nici o amprenta in setari",
                        ToastLength.Long)?.Show();
                    LoadLoginUi();
                } else {
                    if (keyguardManager != null && !keyguardManager.IsKeyguardSecure) {
                        Toast.MakeText(this,
                            "Telefonul trebuie sa fie securizat utilizand senzorul de amprente",
                            ToastLength.Long)?.Show();
                        LoadLoginUi();
                    } else {
                        GenKey();
                    }

                    if (!CipherInit()) return;

                    var helper = new FingerprintHandler(this);
                    helper.StartAuthentication(fingerprintManager, new FingerprintManager.CryptoObject(_cipher));
                    helper.FingerprintAuth += delegate(object sender,
                        FingerprintHandler.FingerprintAuthEventArgs args) {
                        if (args.Status) {
                            StartActivity(typeof(MainActivity));
                            Finish();
                        } else {
                            var filterError =
                                new SimpleColorFilter(
                                    ContextCompat.GetColor(this, Resource.Color.accent));
                            _animationView.AddValueCallback(new KeyPath("**"),
                                LottieProperty.ColorFilter,
                                new LottieValueCallback(filterError));
                            var vibrator = (Vibrator) GetSystemService(VibratorService);
                            vibrator?.Vibrate(VibrationEffect.CreateOneShot(100,
                                VibrationEffect.DefaultAmplitude));
                            if (args.ErrorsCount != 5) return;
                            Toast.MakeText(this, "5 incercari gresite de verificare a amprentelor!",
                                ToastLength.Long)?.Show();
                        }
                    };
                }
            }
        }

        private void LoadLoginUi() {
            SetContentView(Resource.Layout.activity_login); //
            InitUi();
            InitListeners();
            const string permission = Manifest.Permission.ReadPhoneState;
            if (CheckSelfPermission(permission) != Permission.Granted) {
                RequestPermissions(Constants.PermissionsArray, 0);
            }

            try {
                if (!bool.Parse(Utils.GetDefaults("Logins")) ||
                    Utils.GetDefaults("Token") == null) return;
                StartActivity(typeof(MainActivity));
                Finish();
            } catch (Exception ex) {
                Log.Error("loginActivity Error la verificare login", ex.Message);
            }
        }

        private bool CipherInit() {
            try {
                _cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                                             + "/"
                                             + KeyProperties.BlockModeCbc
                                             + "/"
                                             + KeyProperties.EncryptionPaddingPkcs7);
                _keyStore.Load(null);
                IKey key = _keyStore.GetKey(_keyName, null);
                _cipher?.Init(CipherMode.EncryptMode, key);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private void GenKey() {
            _keyStore = KeyStore.GetInstance("AndroidKeyStore");
            var keyGenerator =
                KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            _keyStore?.Load(null);
            keyGenerator?.Init(new KeyGenParameterSpec.Builder(_keyName, (KeyStorePurpose) 3)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .Build());
            keyGenerator?.GenerateKey();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults) {
            if (grantResults[0] != Permission.Granted) {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru telefon refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            } else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted) {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru locatie refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            } else if (grantResults[3] != Permission.Granted) {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru camera refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            }
        }

        private void InitUi() {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);

            _usernameEditText = FindViewById<EditText>(Resource.Id.et_email);
            _passwordEditText = FindViewById<EditText>(Resource.Id.et_password);

            _loginButton = FindViewById<AppCompatButton>(Resource.Id.btn_login);
            _pinButton = FindViewById<AppCompatButton>(Resource.Id.btn_pin);
            _registerButton = FindViewById<AppCompatButton>(Resource.Id.btn_register);
            _pwdResetTextView = FindViewById<TextView>(Resource.Id.tv_password_forgot);

            _progressBarDialog =
                new ProgressBarDialog(
                    "Va rugam asteptati", "Autentificare...", this, false);

            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
        }

        private void InitFingerprintUi() {
            _pinButton = FindViewById<AppCompatButton>(Resource.Id.btn_pin);
            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
        }

        private void InitListeners() {
            _loginButton.Click += BtnOnClick;
            _registerButton.Click += RegisterButtonOnClick;
            _pwdResetTextView.Click += PwdResetTextViewOnClick;
        }

        private void InitFingerprintListeners() {
            _pinButton.Click += PinButtonOnClick;
        }

        private void PinButtonOnClick(object sender, EventArgs e) {
            StartActivity(typeof(PinActivity));
        }

        private void PwdResetTextViewOnClick(object sender, EventArgs e) {
            StartActivity(typeof(PwdResetActivity));
        }

        private void RegisterButtonOnClick(object sender, EventArgs e) {
            StartActivity(typeof(RegisterActivity));
        }

        private async void BtnOnClick(object sender, EventArgs e) {
            Utils.HideKeyboard(this);
            _progressBarDialog.Show();
            await Task.Run(async () => {
                try {
                    JSONObject dataToSend = new JSONObject().Put("email", _usernameEditText.Text)
                        .Put("password", _passwordEditText.Text).Put("imei",
                            Utils.GetDeviceIdentificator(this));

                    string response = await WebServices.WebServices.Post("/api/login", dataToSend);
                    Log.Error("LoginActivity", response);
                    if (response != null) {
                        var responseJson = new JSONObject(response);
                        Log.Error("LoginActivity", "req response: " + responseJson);
                        switch (responseJson.GetInt("status")) {
                            case 0:
                                Snackbar.Make(_layout, "Nu esti autorizat sa faci acest request!",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 1:
                                Snackbar.Make(_layout, "Eroare la comunicarea cu serverul",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 2:
                                JSONObject payload = new JSONObject(response);
                                string token = payload.IsNull("token")
                                    ? null
                                    : payload.GetString("token");
                                string nume = payload.IsNull("nume")
                                    ? null
                                    : payload.GetString("nume");
                                bool logins = !payload.IsNull("logins") &&
                                              payload.GetBoolean("logins");
                                string avatar = payload.IsNull("avatar")
                                    ? null
                                    : payload.GetString("avatar");
                                string id = payload.IsNull("id")
                                    ? null
                                    : payload.GetString("id");
                                string idClient = payload.IsNull("idClient")
                                    ? null
                                    : payload.GetString("idClient");
                                string idPersoana = payload.IsNull("idPersAsisoc")
                                    ? null
                                    : payload.GetString("idPersAsisoc");
                                int type = payload.IsNull("tip") ? -1 : payload.GetInt("tip");

                                Log.Error("IdPers", idPersoana + "");
                                Utils.SetDefaults("Token", token ?? string.Empty);
                                Utils.SetDefaults("Imei", Utils.GetDeviceIdentificator(this) ?? string.Empty);
                                Utils.SetDefaults("Email", _usernameEditText.Text ?? string.Empty);
                                Utils.SetDefaults("Logins", logins.ToString() ?? false.ToString());
                                Utils.SetDefaults("Name", nume ?? string.Empty);
                                Utils.SetDefaults("Avatar", $"{Constants.PublicServerAddress}/{avatar}");
                                Utils.SetDefaults("Id", id ?? string.Empty);
                                Utils.SetDefaults("IdClient", idClient ?? string.Empty);
                                Utils.SetDefaults("IdPersoana", idPersoana ?? string.Empty);
                                Utils.SetDefaults("UserType", type.ToString());

                                StartActivity(logins ? typeof(MainActivity) : typeof(FirstSetup));


                                if (logins) {
                                    if ((UsersTypes) int.Parse(Utils.GetDefaults("UserType")) == UsersTypes.Pacient) {
                                        var medicationServerServiceIntent =
                                            new Intent(this, typeof(MedicationServerService));
                                        StartService(medicationServerServiceIntent);
                                        StartConfigReceiver();
                                    }
                                }

                                Finish();
                                break;
                            case 3:
                                Snackbar.Make(_layout, "Dispozitivul nu este inregistrat!",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 4:
                                Snackbar.Make(_layout, "Nume de utilizator sau parola incorecte!",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 5:
                                string cod = new JSONObject(response).GetString("codActiv");
                                ShowInactiveUserDialog(cod);
                                break;
                        }
                    } else {
                        var snack = Snackbar.Make(_layout, "Nu se poate conecta la server!",
                            Snackbar.LengthLong);
                        snack.Show();
                    }
                } catch (Exception ex) {
                    Log.Error("Eroare la parsarea Jsonului", ex.Message);
                }
            });
            _progressBarDialog.Dismiss();
        }

        private void StartConfigReceiver() {
            var am = (AlarmManager) Application.Context.GetSystemService(AlarmService);
            var pi = PendingIntent.GetBroadcast(Application.Context,
                ConfigReceiver.IdPendingIntent,
                new Intent(this, typeof(ConfigReceiver)),
                PendingIntentFlags.UpdateCurrent);
            am.SetExact(AlarmType.RtcWakeup, 0, pi);
        }

        private void ShowInactiveUserDialog(string cod) {
            RunOnUiThread(() => {
                var alert = new AlertDialog.Builder(this);
                alert.SetMessage(
                    "Nu puteti utiliza aplicatia in momentul de fata pentru ca dispozitivul este asignat unui alt cont." +
                    "Verificati setarile din Sistemul de Monitorizare Pacienti - id:  " + cod);
                alert.SetPositiveButton("Ok",
                    (senderAlert, args) => { Log.Error("LoginActivity", "Ok"); });
                Dialog dialog = alert.Create();
                dialog.Show();
            });
        }
    }

    public enum UsersTypes {
        Undefined = -1,
        Unknown = 1,
        Asistent = 2,
        Pacient = 3,
        SelfRegistered = 4,
        MOB= 5,
        MOBWEB = 56
    }
}
