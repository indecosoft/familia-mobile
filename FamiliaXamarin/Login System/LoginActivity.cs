using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Hardware.Fingerprints;
using Android.OS;
using Android.Preferences;
using Android.Security.Keystore;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.App;
using Android.Widget;
using FamiliaXamarin.Helpers;
using Java.Security;
using Javax.Crypto;
using Org.Json;
using Permission = Android.Content.PM.Permission;

namespace FamiliaXamarin
{
    [Activity(Label = "Familia", Theme = "@style/AppTheme.Dark", MainLauncher = true)]
    public class LoginActivity : AppCompatActivity
    {
        private ConstraintLayout _layout;

        private EditText _usernameEditText;
        private EditText _passwordEditText;
        private TextView _registerTextView;
        private TextView _pwdResetTextView;
        private Button _loginButton;
#pragma warning disable 618
        private ProgressDialog _progressDialog;
#pragma warning restore 618
        private KeyStore keyStore;
        private Cipher cipher;
        private string KEY_NAME = "EDMTDev";
        private readonly string[] _permissionsLocation =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        //private readonly IWebServices _webServices = new WebServices();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            bool fingerprint;

            if (string.IsNullOrEmpty(Utils.GetDefaults("fingerprint", this))) 
                fingerprint = false;
            else
                fingerprint = Convert.ToBoolean(Utils.GetDefaults("fingerprint", this));

            FingerprintManagerCompat checkHardware = FingerprintManagerCompat.From(this);
            KeyguardManager keyguardManager1 = (KeyguardManager)GetSystemService(KeyguardService);

            if (fingerprint && checkHardware.IsHardwareDetected && keyguardManager1.IsKeyguardSecure)
            {

                SetContentView(Resource.Layout.activity_finger);
                //Using the Android Support Library v4
                KeyguardManager keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
                FingerprintManager fingerprintManager = (FingerprintManager)GetSystemService(FingerprintService);

                if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.UseFingerprint) != (int)Permission.Granted)
                    return;
                if (!fingerprintManager.IsHardwareDetected)
                    Toast.MakeText(this, "Nu exista permisiuni pentru autentificare utilizand amprenta", ToastLength.Long).Show();
                else
                {
                    if (!fingerprintManager.HasEnrolledFingerprints)
                        Toast.MakeText(this, "Nu ati inregistrat nici o amprenta in setari", ToastLength.Long).Show();
                    else
                    {
                        if (!keyguardManager.IsKeyguardSecure)
                            Toast.MakeText(this, "Telefonul trebuie sa fie securizat utilizand senzorul de amprente", ToastLength.Long).Show();
                        else
                            GenKey();
                        if (CipherInit())
                        {
                            FingerprintManager.CryptoObject cryptoObject = new FingerprintManager.CryptoObject(cipher);
                            FingerprintHandler helper = new FingerprintHandler(this);
                            helper.StartAuthentication(fingerprintManager, cryptoObject);
                        }
                    }
                }
            }
            else if (!fingerprint)
            {
                SetContentView(Resource.Layout.activity_login);
                // Create your application here
                InitUi();
                InitListeners();

                const string permission = Manifest.Permission.ReadPhoneState;
                if (CheckSelfPermission(permission) != (int)Permission.Granted)
                {
                    RequestPermissions(_permissionsLocation, 0);
                }


                try
                {
                    if (!bool.Parse(Utils.GetDefaults("Logins", this)) || Utils.GetDefaults("Token", this) == null) return;
                    StartActivity(typeof(MainActivity));
                    Finish();
                }
                catch
                {
                    // ignored
                }
            }
            else if (!checkHardware.IsHardwareDetected)
            {
                Toast.MakeText(this, "Nu aveti senzor de amprente pe telefon", ToastLength.Long).Show();
            }
            else if (!keyguardManager1.IsKeyguardSecure)
            {
                Toast.MakeText(this, "Telefonul trebuie sa fie securizat utilizand senzorul de amprente", ToastLength.Long).Show();
            }


        }
        private bool CipherInit()
        {
            try
            {
                cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                                            + "/"
                                            + KeyProperties.BlockModeCbc
                                            + "/"
                                            + KeyProperties.EncryptionPaddingPkcs7);
                keyStore.Load(null);
                IKey key = (IKey)keyStore.GetKey(KEY_NAME, null);
                cipher.Init(CipherMode.EncryptMode, key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void GenKey()
        {
            keyStore = KeyStore.GetInstance("AndroidKeyStore");
            KeyGenerator keyGenerator = null;
            keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            keyStore.Load(null);
            keyGenerator.Init(new KeyGenParameterSpec.Builder(KEY_NAME, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .Build());
            keyGenerator.GenerateKey();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (grantResults[0] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru telefon refuzate", Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[1] != Permission.Granted || grantResults[2] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru locatie refuzate", Snackbar.LengthShort);
                snack.Show();
            }
            else if (grantResults[3] != Permission.Granted)
            {
                var snack = Snackbar.Make(_layout, "Permisiuni pentru camera refuzate", Snackbar.LengthShort);
                snack.Show();
            }

        }
        private void InitUi()
        {
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            _layout = FindViewById<ConstraintLayout>(Resource.Id.layout);

            _usernameEditText = FindViewById<EditText>(Resource.Id.UsernameEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.PasswordEditText);

            _loginButton = FindViewById<Button>(Resource.Id.btnLogin);

            _registerTextView = FindViewById<TextView>(Resource.Id.Signup);
            _pwdResetTextView = FindViewById<TextView>(Resource.Id.PasswordReset);

#pragma warning disable 618
            _progressDialog = new ProgressDialog(this);
#pragma warning restore 618
            _progressDialog.SetTitle("Va rugam asteptati ...");
            _progressDialog.SetMessage("Autentificare");
            _progressDialog.SetCancelable(false);


        }

        private void InitListeners()
        {
            _loginButton.Click += BtnOnClick;
            _registerTextView.Click += RegisterTextViewOnClick;
            _pwdResetTextView.Click += PwdResetTextViewOnClick;

        }

        private void PwdResetTextViewOnClick(object sender, EventArgs e)
        {
           StartActivity(typeof(PwdResetActivity));
        }

        private void RegisterTextViewOnClick(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

        private async void BtnOnClick(object sender, EventArgs e)
        {
            Utils.HideKeyboard(this);
            _progressDialog.Show();

            await Task.Run(async () => {
                var dataToSend = new JSONObject().Put("email", _usernameEditText.Text)
                    .Put("password", _passwordEditText.Text).Put("imei", Utils.GetImei(this));

                string response = await WebServices.Post(Constants.PublicServerAddress + "/api/login", dataToSend);
                if (response != null)
                {
                    Snackbar snack;
                    var responseJson = new JSONObject(response);
                    switch (responseJson.GetInt("status"))
                    {
                        case 0:
                            snack = Snackbar.Make(_layout, "Wrong Username or Password", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 1:
                            snack = Snackbar.Make(_layout, "Internal Server Error", Snackbar.LengthLong);
                            snack.Show();
                            break;
                        case 2:
                            var token = new JSONObject(response).GetString("token");
                            var nume = new JSONObject(response).GetString("nume");
                            var logins = new JSONObject(response).GetBoolean("logins");
                            var avatar = new JSONObject(response).GetString("avatar");
                            var id = new JSONObject(response).GetString("id");
                    
                            Utils.SetDefaults("Token", token, this);
                            Utils.SetDefaults("Imei", Utils.GetImei(this), this);
                            Utils.SetDefaults("Email", _usernameEditText.Text, this);
                            Utils.SetDefaults("Logins", logins.ToString(), this);
                            Utils.SetDefaults("HourName", nume, this);
                            Utils.SetDefaults("Avatar", Constants.PublicServerAddress + avatar, this);
                            Utils.SetDefaults("IdClient", id, this);

                            if (logins)
                                StartActivity(typeof(MainActivity));
                            else
                                StartActivity(typeof(FirstSetup));

                            Finish();
                            break;
                    }
                }
                else
                {
                    var snack = Snackbar.Make(_layout, "Unable to reach the server!", Snackbar.LengthLong);
                    snack.Show();
                }
            });
            _progressDialog.Dismiss();
 
        }
    }
}