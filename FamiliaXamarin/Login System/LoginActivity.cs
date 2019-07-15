using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Hardware.Fingerprints;
using Android.OS;
using Android.Security.Keystore;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Login_System;
using Java.Security;
using Javax.Crypto;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Exception = System.Exception;
using Permission = Android.Content.PM.Permission;
using Android.Util;
using Familia.LoginSystem;

namespace Familia.Login_System
{
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark", MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : AppCompatActivity
    {
        private ConstraintLayout _layout;

        private EditText _usernameEditText;
        private EditText _passwordEditText;
        private AppCompatButton _registerButton;
        private TextView _pwdResetTextView;
        private AppCompatButton _loginButton;
        private KeyStore _keyStore;
        private Cipher _cipher;
        private readonly string _keyName = "EDMTDev";
        private ProgressBarDialog _progressBarDialog;

        private readonly string[] _permissionsArray =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        protected override void OnResume()
        {
            base.OnResume();

            var fingerprint = !string.IsNullOrEmpty(Utils.GetDefaults("fingerprint")) &&
                              Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

            if (!fingerprint && !string.IsNullOrEmpty(Utils.GetDefaults("UserPin")))
            {
                StartActivity(typeof(PinActivity));
                return;
            }
            var checkHardware = FingerprintManagerCompat.From(this);
            var keyguardManager1 = (KeyguardManager)GetSystemService(KeyguardService);
            if (!fingerprint || !checkHardware.IsHardwareDetected ||
                !keyguardManager1.IsKeyguardSecure) return;
            SetContentView(Resource.Layout.activity_finger);
            var animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            //Using the Android Support Library v4
            var keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
            var fingerprintManager = (FingerprintManager)GetSystemService(FingerprintService);
            var btn = FindViewById<Button>(Resource.Id.btn_pin);
            btn.Click += (sender, e) =>
            {
                StartActivity(typeof(PinActivity));
            };


            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.UseFingerprint) !=
                (int)Permission.Granted)
                return;
            if (!fingerprintManager.IsHardwareDetected)
            {
                Toast.MakeText(this,
                    "Nu exista permisiuni pentru autentificare utilizand amprenta",
                    ToastLength.Long).Show();
                LoadLoginUi();
            }
            else
            {
                if (!fingerprintManager.HasEnrolledFingerprints)
                {
                    Toast.MakeText(this,
                        "Nu ati inregistrat nici o amprenta in setari",
                        ToastLength.Long).Show();
                    LoadLoginUi();
                }
                else
                {
                    if (!keyguardManager.IsKeyguardSecure)
                    {
                        Toast.MakeText(this,
                            "Telefonul trebuie sa fie securizat utilizand senzorul de amprente",
                            ToastLength.Long).Show();
                        LoadLoginUi();
                    }
                    else
                        GenKey();

                    if (!CipherInit()) return;
                    var cryptoObject = new FingerprintManager.CryptoObject(_cipher);
                    var helper = new FingerprintHandler(this);

                    helper.StartAuthentication(fingerprintManager, cryptoObject);
                    helper.FingerprintAuth += delegate (object sender,
                        FingerprintHandler.FingerprintAuthEventArgs args)
                    {
                        if (args.Status)
                        {
                            StartActivity(typeof(MainActivity));
                            Finish();
                        }
                        else
                        {
                            var filterError =
                                new SimpleColorFilter(
                                    ContextCompat.GetColor(this, Resource.Color.accent));
                            animationView.AddValueCallback(new KeyPath("**"),
                                LottieProperty.ColorFilter,
                                new LottieValueCallback(filterError));
                            var vibrator = (Vibrator)GetSystemService(VibratorService);
                            vibrator?.Vibrate(VibrationEffect.CreateOneShot(100,
                                VibrationEffect.DefaultAmplitude));
                            if (args.ErrorsCount != 5) return;
                            Toast.MakeText(this, "5 incercari gresite de verificare a amprentelor!",
                                ToastLength.Long).Show();
                        }
                    };
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitLogin();

        }


        private void InitLogin()
        {
            var fingerprint = !string.IsNullOrEmpty(Utils.GetDefaults("fingerprint")) &&
                              Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

            if (!fingerprint && !string.IsNullOrEmpty(Utils.GetDefaults("UserPin")))
            {
                StartActivity(typeof(PinActivity));
                return;
            }
            var checkHardware = FingerprintManagerCompat.From(this);
            var keyguardManager1 = (KeyguardManager)GetSystemService(KeyguardService);

            if (fingerprint && checkHardware.IsHardwareDetected &&
                keyguardManager1.IsKeyguardSecure)
            {
                SetContentView(Resource.Layout.activity_finger);
                var animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
                var filter =
                    new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.colorAccent));
                animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                    new LottieValueCallback(filter));
                //Using the Android Support Library v4
                var keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
                var fingerprintManager = (FingerprintManager)GetSystemService(FingerprintService);
                var btn = FindViewById<Button>(Resource.Id.btn_pin);
                btn.Click += (sender, e) =>
                {
                    StartActivity(typeof(PinActivity));
                };

                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.UseFingerprint) !=
                    (int)Permission.Granted)
                    return;
                if (!fingerprintManager.IsHardwareDetected)
                {
                    Toast.MakeText(this,
                        "Nu exista permisiuni pentru autentificare utilizand amprenta",
                        ToastLength.Long).Show();
                    LoadLoginUi();
                }
                else
                {
                    if (!fingerprintManager.HasEnrolledFingerprints)
                    {
                        Toast.MakeText(this,
                            "Nu ati inregistrat nici o amprenta in setari",
                            ToastLength.Long).Show();
                        LoadLoginUi();
                    }
                    else
                    {
                        if (!keyguardManager.IsKeyguardSecure)
                        {
                            Toast.MakeText(this,
                                "Telefonul trebuie sa fie securizat utilizand senzorul de amprente",
                                ToastLength.Long).Show();
                            LoadLoginUi();
                        }
                        else
                            GenKey();

                        if (!CipherInit()) return;
                        var cryptoObject = new FingerprintManager.CryptoObject(_cipher);
                        var helper = new FingerprintHandler(this);

                        helper.StartAuthentication(fingerprintManager, cryptoObject);
                        helper.FingerprintAuth += delegate (object sender,
                            FingerprintHandler.FingerprintAuthEventArgs args)
                        {
                            if (args.Status)
                            {
                                StartActivity(typeof(MainActivity));
                                Finish();
                            }
                            else
                            {
                                var filterError =
                                    new SimpleColorFilter(
                                        ContextCompat.GetColor(this, Resource.Color.accent));
                                animationView.AddValueCallback(new KeyPath("**"),
                                    LottieProperty.ColorFilter,
                                    new LottieValueCallback(filterError));
                                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                                vibrator?.Vibrate(VibrationEffect.CreateOneShot(100,
                                    VibrationEffect.DefaultAmplitude));
                                if (args.ErrorsCount != 5) return;
                                Toast.MakeText(this,
                                    "5 incercari gresite de verificare a amprentelor!",
                                    ToastLength.Long).Show();
                            }
                        };
                    }
                }
            }
            else if (!fingerprint)
            {
                LoadLoginUi();
            }
            else if (!checkHardware.IsHardwareDetected)
            {
                Toast.MakeText(this,
                        "Nu aveti senzor de amprente pe telefon", ToastLength.Long)
                    .Show();
                LoadLoginUi();
            }
            else if (!keyguardManager1.IsKeyguardSecure)
            {
                Toast.MakeText(this,
                    "Telefonul trebuie sa fie securizat utilizand senzorul de amprente",
                    ToastLength.Long).Show();
                LoadLoginUi();
            }
        }

        private void LoadLoginUi()
        {
            SetContentView(Resource.Layout.activity_login); //
            // Create your application here
            InitUi();
            InitListeners();

            const string permission = Manifest.Permission.ReadPhoneState;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                RequestPermissions(_permissionsArray, 0);
            }


            try
            {
                if (!bool.Parse(Utils.GetDefaults("Logins")) ||
                    Utils.GetDefaults("Token") == null) return;
                StartActivity(typeof(MainActivity));
                Finish();
            }
            catch

            {
                // ignored
            }
        }

        private bool CipherInit()
        {
            try
            {
                _cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                                             + "/"
                                             + KeyProperties.BlockModeCbc
                                             + "/"
                                             + KeyProperties.EncryptionPaddingPkcs7);
                _keyStore.Load(null);
                var key = _keyStore.GetKey(_keyName, null);
                _cipher.Init(CipherMode.EncryptMode, key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void GenKey()
        {
            _keyStore = KeyStore.GetInstance("AndroidKeyStore");
            var keyGenerator =
                KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            _keyStore.Load(null);
            keyGenerator.Init(new KeyGenParameterSpec.Builder(_keyName, (KeyStorePurpose)3)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .Build());
            keyGenerator.GenerateKey();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {

            if (grantResults[0] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru telefon refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru locatie refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[3] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru camera refuzate",
                    Snackbar.LengthShort);
                snack.Show();
            }
        }

        private void InitUi()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);

            _usernameEditText = FindViewById<EditText>(Resource.Id.et_email);
            _passwordEditText = FindViewById<EditText>(Resource.Id.et_password);

            _loginButton = FindViewById<AppCompatButton>(Resource.Id.btn_login);

            _registerButton = FindViewById<AppCompatButton>(Resource.Id.btn_register);
            _pwdResetTextView = FindViewById<TextView>(Resource.Id.tv_password_forgot);

            _progressBarDialog =
                new ProgressBarDialog(
                    "Va rugam asteptati", "Autentificare...", this, false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
        }

        private void InitListeners()
        {
            _loginButton.Click += BtnOnClick;
            _registerButton.Click += RegisterButtonOnClick;
            _pwdResetTextView.Click += PwdResetTextViewOnClick;
        }

        private void PwdResetTextViewOnClick(object sender, EventArgs e)
        {
            StartActivity(typeof(PwdResetActivity));
        }

        private void RegisterButtonOnClick(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

        private async void BtnOnClick(object sender, EventArgs e)
        {
            Utils.HideKeyboard(this);
            _progressBarDialog.Show();
            await Task.Run(async () =>
            {
                try
                {
                    var dataToSend = new JSONObject().Put("email", _usernameEditText.Text)
                    .Put("password", _passwordEditText.Text).Put("imei",
                        Utils.GetImei(this));

                    var response =
                        await WebServices.Post(Constants.PublicServerAddress + "/api/login",
                            dataToSend);
                    if (response != null)
                    {
                        var responseJson = new JSONObject(response);
                        switch (responseJson.GetInt("status"))
                        {
                            case 0:
                                Snackbar.Make(_layout, "Nu esti autorizat sa faci acest request!",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 1:
                                Snackbar.Make(_layout, "Eroare la comunicarea cu serverul",
                                    Snackbar.LengthLong).Show();
                                break;
                            case 2:
                                var token = new JSONObject(response).GetString("token");
                                var nume = new JSONObject(response).GetString("nume");
                                var logins = new JSONObject(response).GetBoolean("logins");
                                var avatar = new JSONObject(response).GetString("avatar");
                                var id = new JSONObject(response).GetString("id");
                                var type = new JSONObject(response).GetString("tip");

                                Utils.SetDefaults("Token", token);
                                Utils.SetDefaults("Imei", Utils.GetImei(this));
                                Utils.SetDefaults("Email", _usernameEditText.Text);
                                Utils.SetDefaults("Logins", logins.ToString());
                                Utils.SetDefaults("Name", nume);
                                Utils.SetDefaults("Avatar", $"{Constants.PublicServerAddress}/{avatar}");
                                Utils.SetDefaults("IdClient", id);
                                Utils.SetDefaults("UserType", type);

                                StartActivity(logins ? typeof(MainActivity) : typeof(FirstSetup));

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
                        }
                    }
                    else
                    {
                        var snack = Snackbar.Make(_layout, "Nu se poate conecta la server!",
                            Snackbar.LengthLong);
                        snack.Show();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Eroare la parsarea Jsonului", ex.Message);
                }

            });
            _progressBarDialog.Dismiss();
        }
    }
}